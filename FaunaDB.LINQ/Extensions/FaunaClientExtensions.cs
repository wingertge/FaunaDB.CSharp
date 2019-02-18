using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using FaunaDB.Driver;
using FaunaDB.Driver.Errors;
using FaunaDB.LINQ.Errors;
using FaunaDB.LINQ.Modeling;
using FaunaDB.LINQ.Query;
using FaunaDB.LINQ.Types;

namespace FaunaDB.LINQ.Extensions
{
    public static class FaunaClientExtensions
    {
        private static object[] ObjToParamsOrSingle(object obj, IDbContext context)
        {
            return obj.GetType().Name.StartsWith("Tuple") || obj.GetType().Name.StartsWith("ValueTuple")
                ? obj.GetType().GetProperties().Select(a => context.ToFaunaObjOrPrimitive(a.GetValue(obj))).ToArray()
                : new[] { context.ToFaunaObjOrPrimitive(obj) };
        }

        public static IQueryable<T> Query<T>(this IDbContext context, Expression<Func<T, bool>> selector)
        {
            if (!(selector.Body is BinaryExpression binary)) throw new ArgumentException("Index selector must be binary expression.");

            return new FaunaQueryableData<T>(context, QueryModel.Map(WalkSelector(binary, context), QueryModel.Lambda("arg0", QueryModel.Get(QueryModel.Var("arg0")))));
        }

        private static object WalkSelector(BinaryExpression expression, IDbContext context)
        {
            switch (expression.Left)
            {
                case BinaryExpression leftExp when expression.Right is BinaryExpression rightExp:
                    var left = WalkSelector(leftExp, context);
                    var right = WalkSelector(rightExp, context);

                    switch (expression.NodeType)
                    {
                        case ExpressionType.Or:
                        case ExpressionType.OrElse:
                            return QueryModel.Union(left, right);
                        case ExpressionType.And:
                        case ExpressionType.AndAlso:
                            return QueryModel.Intersection(left, right);
                        default:
                            throw new UnsupportedMethodException(expression.NodeType.ToString());
                    }
                case MemberExpression _ when expression.Right is ConstantExpression:
                case ConstantExpression _ when expression.Right is MemberExpression:
                    var member = expression.Left is MemberExpression mem ? mem : (MemberExpression) expression.Right;
                    var constant = expression.Right is ConstantExpression con ? con : (ConstantExpression) expression.Left;
                    var args = ObjToParamsOrSingle(constant.Value, context);
                    var mapping = context.Mappings[member.Member.DeclaringType][member.GetPropertyInfo()];
                    if(mapping.Type != DbPropertyType.PrimitiveIndex || !(mapping is IndexPropertyInfo indexInfo))
                        throw new ArgumentException("Can't use unindexed property for selector!");
                    var indexName = indexInfo.IndexName;
                    return QueryModel.Match(QueryModel.Index(indexName), args);
                case MemberExpression _ when expression.Right is MethodCallExpression:
                case MethodCallExpression _ when expression.Right is MemberExpression:
                    var member1 = expression.Left is MemberExpression mem1 ? mem1 : (MemberExpression)expression.Right;
                    var method = expression.Right is MethodCallExpression meth ? meth : (MethodCallExpression)expression.Left;
                    var methodValue = Expression.Lambda(method).Compile().DynamicInvoke();
                    var args1 = ObjToParamsOrSingle(methodValue, context);
                    var indexAttr1 = member1.GetPropertyInfo().GetCustomAttribute<IndexedAttribute>();
                    if (indexAttr1 == null) throw new ArgumentException("Can't use unindexed property for selector!");
                    var indexName1 = indexAttr1.Name;
                    return QueryModel.Match(QueryModel.Index(indexName1), args1);
            }

            throw new ArgumentException("Invalid format for selector. Has to be tree of index selector operations.");
        }

        public static IQueryable<T> Query<T>(this IDbContext context, Expression<Func<T, object>> index, params object[] args)
        {
            if(!(index.Body is MemberExpression member)) throw new ArgumentException("Index selector must be a member.");

            var propInfo = member.GetPropertyInfo();
            var indexAttr = propInfo.GetCustomAttribute<IndexedAttribute>();
            if (indexAttr == null) throw new ArgumentException("Can't use unindexed property as selector!", nameof(index));
            var indexName = indexAttr.Name;

            return context.Query<T>(indexName, args);
        }

        public static IQueryable<T> Query<T>(this IDbContext context, string index, params object[] args)
        {
            return new FaunaQueryableData<T>(context, QueryModel.Map(QueryModel.Match(QueryModel.Index(index), args), QueryModel.Lambda("arg0", QueryModel.Get(QueryModel.Var("arg0")))));
        }

        public static IQueryable<T> Query<T>(this IDbContext context, string @ref)
        {
            return new FaunaQueryableData<T>(context, QueryModel.Get(QueryModel.Ref(@ref)));
        }

        public static Task<T> Create<T>(this IDbContext context, T obj)
        {
            return context.Query<T>(QueryModel.Create(obj.GetClassRef(), QueryModel.Obj("data", context.ToFaunaObj(obj))));
        }

        public static Task<T> Update<T>(this IDbContext context, T obj)
        {
            var mapping = context.Mappings[typeof(T)];
            var id = mapping.FirstOrDefault(a => a.Value.Type == DbPropertyType.Key);
            return context.Update(obj, id.Key.GetValue(obj).ToString());
        }

        public static Task<T> Update<T>(this IDbContext context, T obj, string id)
        {
            return context.Query<T>(QueryModel.Update(QueryModel.Ref(id), context.ToFaunaObj(obj)));
        }

        public static Task<T> Upsert<T>(this IDbContext context, T obj)
        {
            var mapping = context.Mappings[typeof(T)];
            var id = mapping.FirstOrDefault(a => a.Value.Type == DbPropertyType.Key);
            return context.Upsert(obj, id.Key.GetValue(obj).ToString());
        }

        public static Task<T> Upsert<T>(this IDbContext context, T obj, string id)
        {
            return context.Query<T>(QueryModel.If(QueryModel.Exists(QueryModel.Ref(id)),
                QueryModel.Update(QueryModel.Ref(id), context.ToFaunaObj(obj)),
                QueryModel.Create(obj.GetClassRef(), context.ToFaunaObj(obj))));
        }

        public static Task<T> Upsert<T>(this IDbContext context, T obj, string index, params object[] args)
        {
            return context.Query<T>(QueryModel.If(QueryModel.Exists(QueryModel.Match(QueryModel.Index(index), args)),
                QueryModel.Map(QueryModel.Match(QueryModel.Index(index), args), QueryModel.Lambda("arg0", QueryModel.Update(QueryModel.Var("arg0"), context.ToFaunaObj(obj)))),
                QueryModel.Create(obj.GetClassRef(), context.ToFaunaObj(obj))));
        }

        public static Task<T> Upsert<T>(this IDbContext context, T obj, Expression<Func<T, object>> indexSelector, params object[] args)
        {
            if (!(indexSelector.Body is MemberExpression member)) throw new ArgumentException("Index selector must be a member.");

            var propInfo = member.GetPropertyInfo();
            var indexAttr = propInfo.GetCustomAttribute<IndexedAttribute>();
            if (indexAttr == null) throw new ArgumentException("Can't use unindexed property as selector!", nameof(indexSelector));
            var indexName = indexAttr.Name;

            return Upsert(context, obj, indexName, args);
        }

        public static Task<T> Upsert<T>(this IDbContext context, T obj, Expression<Func<T, bool>> indices)
        {
            if (!(indices.Body is BinaryExpression binary)) throw new ArgumentException("Index selector must be binary expression.");
            var selectorExpr = WalkSelector(binary, context);

            return context.Query<T>(QueryModel.If(QueryModel.Exists(selectorExpr), QueryModel.Map(selectorExpr, QueryModel.Lambda("arg0", QueryModel.Update(QueryModel.Var("arg0"), context.ToFaunaObj(obj)))),
                QueryModel.Create(obj.GetClassRef(), context.ToFaunaObj(obj))));
        }

        public static Task Delete(this IDbContext context, object obj)
        {
            var mapping = context.Mappings[obj.GetType()];
            var id = mapping.FirstOrDefault(a => a.Value.Type == DbPropertyType.Key);
            return context.Delete(id.Key.GetValue(obj).ToString());
        }

        public static Task Delete(this IDbContext context, string id)
        {
            return context.Query<object>(QueryModel.Delete(QueryModel.Ref(id)));
        }

        public static Task<T> Get<T>(this IDbContext context, string @ref)
        {
            return context.Query<T>(QueryModel.Get(QueryModel.Ref(@ref)));
        }
    }
}