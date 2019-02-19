using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using FaunaDB.Driver;
using FaunaDB.Driver.Errors;
using FaunaDB.LINQ.Errors;
using FaunaDB.LINQ.Extensions;
using FaunaDB.LINQ.Modeling;
using FaunaDB.LINQ.Types;

namespace FaunaDB.LINQ.Query
{
    public class FaunaQueryParser
    {
        private int _lambdaIndex;
        private IDbContext _context;
        private const string CurrentlyUnsupportedError = "Currently unsupported by FaunaDB.";
        private const string UnsupportedError = "Unsupported by FaunaDB (likely won't change).";

        private static readonly Dictionary<(Type, string), Func<object, object, object>> BuiltInBinaryMethods = new Dictionary<(Type, string), Func<object, object, object>>
        {
            { (typeof(string), "Concat"), (left, right) => QueryModel.Concat(left, right) }
        };

        private static readonly Dictionary<(Type, string), Func<object[], object>> BuiltInFunctions = new Dictionary<(Type, string), Func<object[], object>>
        {
            { (typeof(string), "Concat"), QueryModel.Concat },
            { (typeof(Tuple), "Create"), a => a }
        };

        public static Expr Parse(object selector, Expression expr, IDbContext context)
        {
            return (Expr)new FaunaQueryParser(context).WalkExpression(selector, expr);
        }

        private FaunaQueryParser(IDbContext context)
        {
            _context = context;
            _lambdaIndex = 0;
        }

        private object WalkExpression(object selector, Expression expr)
        {
            if (!(expr is MethodCallExpression)) return selector;
            var methodExpr = (MethodCallExpression) expr;

            var argsArr = methodExpr.Arguments;
            var next = argsArr[0];
            var rest = WalkExpression(selector, next);
            var args = argsArr.Skip(1).Take(argsArr.Count - 1).ToArray();

            var method = methodExpr.Method;
            object current;

            switch (method.Name)
            {
                case "Where":
                    current = HandleWhere(args, rest);
                    break;
                case "Select":
                    current = HandleSelect(args, rest);
                    break;
                case "Paginate":
                    current = HandlePaginate(args, rest);
                    break;
                case "Skip":
                    current = QueryModel.Drop(args[0].GetConstantValue<int>(), rest);
                    break;
                case "Take":
                    current = QueryModel.Take(args[0].GetConstantValue<int>(), rest);
                    break;
                case "Distinct":
                    current = QueryModel.Distinct(rest);
                    break;
                case "Include":
                case "AlsoInclude":
                    current = HandleInclude(args, rest, _context);
                    break;
                case "FromQuery":
                    current = ((Func<object, Expr>)((ConstantExpression) args[0]).Value).Invoke(rest);
                    break;
                case "At":
                    current = QueryModel.At(_context.ToFaunaObjOrPrimitive((DateTime) ((ConstantExpression) args[0]).Value), rest);
                    break;
                default:
                    throw new UnsupportedMethodException($"Unsupported method {method}.");
            }

            return current;
        }

        private object HandleInclude(IReadOnlyList<Expression> args, object rest, IDbContext context)
        {
            var lambdaArgName = $"arg{++_lambdaIndex}";
            var lambda = args[0] is UnaryExpression unary ? (LambdaExpression)unary.Operand : (LambdaExpression)args[0];
            if(!(lambda.Body is MemberExpression)) throw new ArgumentException("Selector must be member expression.");
            var memberExpr = (MemberExpression) lambda.Body;
            if(!(memberExpr.Member is PropertyInfo)) throw new ArgumentException("Selector must be property.");
            var propInfo = (PropertyInfo) memberExpr.Member;
            var definingType = propInfo.DeclaringType;

            var mappings = context.Mappings[definingType];
            var fields = definingType.GetProperties().Where(a => mappings[a].Type != DbPropertyType.Key && mappings[a].Type != DbPropertyType.Timestamp)
                .ToDictionary(a => mappings[a].Name,
                    a =>
                    {
                        var fieldSelector = QueryModel.Select(mappings[a].GetFaunaFieldPath(), QueryModel.Var(lambdaArgName));
                        return a.Name == propInfo.Name
                            ? typeof(IEnumerable).IsAssignableFrom(propInfo.PropertyType)
                                ? QueryModel.Map(fieldSelector,
                                    QueryModel.Lambda($"arg{++_lambdaIndex}",
                                        QueryModel.If(QueryModel.Exists(QueryModel.Var($"arg{_lambdaIndex}")), QueryModel.Get(QueryModel.Var($"arg{_lambdaIndex}")), null)))
                                : QueryModel.If(QueryModel.Exists(fieldSelector), QueryModel.Get(fieldSelector), null) //perform safe include
                            : fieldSelector;
                    });

            var mappedObj = QueryModel.Obj(new Dictionary<string, object> {{"ref", QueryModel.Select("ref", QueryModel.Var(lambdaArgName))}, {"ts", QueryModel.Select("ts", QueryModel.Var(lambdaArgName))}, {"data", QueryModel.Obj(fields)}});

            return QueryModel.Map(rest, QueryModel.Lambda(lambdaArgName, mappedObj));
        }

        private static object HandlePaginate(IReadOnlyList<Expression> args, object rest)
        {
            var fromRef = (string)((ConstantExpression)args[0]).Value;
            var sortDirection = (ListSortDirection)((ConstantExpression)args[1]).Value;
            var size = (int)((ConstantExpression)args[2]).Value;
            var time = (DateTime)((ConstantExpression)args[3]).Value;
            var ts = time == default(DateTime) ? (DateTime?)null : time;

            return sortDirection == ListSortDirection.Ascending
                ? QueryModel.Paginate(rest, ts: ts == null ? null : QueryModel.Time(ts.Value.ToString("O")), size: size,
                    after: string.IsNullOrEmpty(fromRef) ? null : QueryModel.Ref(fromRef))
                : QueryModel.Paginate(rest, ts: ts == null ? null : QueryModel.Time(ts.Value.ToString("O")), size: size,
                    before: string.IsNullOrEmpty(fromRef) ? null : QueryModel.Ref(fromRef));
        }

        private object HandleSelect(IReadOnlyList<Expression> args, object rest)
        {
            var lambda = args[0] is UnaryExpression unary
                ? (LambdaExpression) unary.Operand
                : (LambdaExpression) args[0];
            var argName = $"arg{++_lambdaIndex}";

            return QueryModel.Map(rest, QueryModel.Lambda(argName, Accept(lambda.Body, argName)));
        }

        private object HandleWhere(IReadOnlyList<Expression> args, object rest)
        {
            var lambda = args[0] is LambdaExpression lexp ? lexp : (LambdaExpression)((UnaryExpression)args[0]).Operand;
            var body = (BinaryExpression) lambda.Body;
            return QueryModel.Filter(rest, QueryModel.Lambda($"arg{++_lambdaIndex}", Accept(body, $"arg{_lambdaIndex}")));
        }

        private object Visit(BinaryExpression binary, string varName)
        {
            var left = Accept(binary.Left, varName);
            var right = Accept(binary.Right, varName);
            if (binary.Method != null)
            {
                if (BuiltInBinaryMethods.ContainsKey((binary.Method.DeclaringType, binary.Method.Name)))
                    return BuiltInBinaryMethods[(binary.Method.DeclaringType, binary.Method.Name)](left, right);
            }
            switch (binary.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return QueryModel.Add(left, right);
                case ExpressionType.Divide:
                    return QueryModel.Divide(left, right);
                case ExpressionType.Modulo:
                    return QueryModel.Modulo(left, right);
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return QueryModel.Multiply(left, right);
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return QueryModel.Subtract(left, right);
                case ExpressionType.AndAlso:
                    return QueryModel.And(left, right);
                case ExpressionType.OrElse:
                    return QueryModel.Or(left, right);
                case ExpressionType.Equal:
                    return QueryModel.EqualsFn(left, right);
                case ExpressionType.NotEqual:
                    return QueryModel.Not(QueryModel.EqualsFn(left, right));
                case ExpressionType.GreaterThanOrEqual:
                    return QueryModel.GTE(left, right);
                case ExpressionType.GreaterThan:
                    return QueryModel.GT(left, right);
                case ExpressionType.LessThan:
                    return QueryModel.LT(left, right);
                case ExpressionType.LessThanOrEqual:
                    return QueryModel.LTE(left, right);
                case ExpressionType.Coalesce:
                    return QueryModel.If(QueryModel.EqualsFn(left, null), right, left);
                case ExpressionType.ExclusiveOr:
                case ExpressionType.LeftShift:
                case ExpressionType.RightShift:
                case ExpressionType.Or:
                case ExpressionType.And:
                case ExpressionType.ArrayIndex:
                    throw new UnsupportedMethodException(binary.NodeType.ToString(), UnsupportedError);
                default:
                    throw new UnsupportedMethodException(binary.Method?.Name ?? binary.NodeType.ToString());
            }
        }
        
        private object Visit(ConditionalExpression conditional, string varName) => QueryModel.If(Accept(conditional.Test, varName), Accept(conditional.IfTrue, varName), Accept(conditional.IfFalse, varName));
        private object Visit(ConstantExpression constant) => _context.ToFaunaObjOrPrimitive(constant.Value);
        private object Visit(DefaultExpression defaultExp) => _context.ToFaunaObjOrPrimitive(Expression.Lambda(defaultExp).Compile().DynamicInvoke());

        private object Visit(ListInitExpression listInit, string varName)
        {
            var result = new List<object>();
            foreach (var initializer in listInit.Initializers)
            {
                var args = initializer.Arguments;
                if(args.Count == 1) result.Add(Accept(args[0], varName));
                else throw new UnsupportedMethodException("Tuple collection initializer not supported.");
            }
            return result.ToArray();
        }

        private object Visit(MemberExpression member, string varName)
        {
            switch (member.Member)
                    {
                        case FieldInfo field:
                            switch (member.Expression)
                            {
                                case ConstantExpression constRoot:
                                    return _context.ToFaunaObjOrPrimitive(field.GetValue(constRoot.Value));
                                case ParameterExpression _:
                                    throw new InvalidOperationException("Can't use selector on field of model. Use a property instead.");
                                case MemberExpression memberRoot:
                                    return Accept(memberRoot, varName);
                                default:
                                    throw new ArgumentOutOfRangeException(); ;
                            }
                        case PropertyInfo property:
                            var mapping = _context.Mappings[property.DeclaringType][property];
                            switch (member.Expression)
                            {
                                case ConstantExpression constRoot:
                                    return _context.ToFaunaObjOrPrimitive(property.GetValue(constRoot.Value));
                                case ParameterExpression _:
                                    var isReference = mapping.Type == DbPropertyType.Reference;
                                    if(!isReference) return QueryModel.Select(mapping.GetFaunaFieldPath(), QueryModel.Var(varName));
                                    return typeof(IEnumerable).IsAssignableFrom(property.PropertyType)
                                        ? QueryModel.Map(
                                            QueryModel.Select(mapping.GetFaunaFieldPath(), QueryModel.Var(varName)),
                                            QueryModel.Lambda($"arg{++_lambdaIndex}",
                                                QueryModel.Map(QueryModel.Var($"arg{_lambdaIndex}"),
                                                    QueryModel.Lambda($"arg{++_lambdaIndex}", QueryModel.Get(QueryModel.Var($"arg{_lambdaIndex}")))
                                                )))
                                        : QueryModel.Map(
                                            QueryModel.Select(mapping.GetFaunaFieldPath(), QueryModel.Var(varName)),
                                            QueryModel.Lambda($"arg{++_lambdaIndex}", QueryModel.Get($"arg{_lambdaIndex}")));
                                case MemberExpression memberRoot:
                                    switch (memberRoot.Member)
                                    {
                                        case FieldInfo _:
                                            return Accept(memberRoot, varName);
                                        case PropertyInfo prop:
                                            return _context.IsReferenceType(prop.PropertyType)
                                                ? QueryModel.Map(
                                                    QueryModel.Select(mapping.GetFaunaFieldPath(),
                                                        Accept(memberRoot, varName)),
                                                    QueryModel.Lambda($"arg{++_lambdaIndex}", QueryModel.Get(QueryModel.Var($"arg{_lambdaIndex}"))))
                                                : QueryModel.Select(mapping.GetFaunaFieldPath(), Accept(memberRoot, varName));
                                        default:
                                            throw new ArgumentOutOfRangeException();
                                    }
                                default:
                                    return Accept(member.Expression, varName);
                            }
                        default:
                            throw new ArgumentOutOfRangeException(); ;
                    }
        }

        private object Visit(MemberInitExpression memberInit, string varName)
        {
            var baseObj = memberInit.NewExpression.Constructor.Invoke(new object[] { });
            var baseType = baseObj.GetType();
            var mappings = _context.Mappings[baseType];
            var members = baseType.GetProperties().Where(a =>
            {
                var propType = a.PropertyType;
                if (propType.Name.StartsWith("CompositeIndex")) return false;
                var propMapping = mappings[a];
                return propMapping.Type != DbPropertyType.Key && propMapping.Type != DbPropertyType.Timestamp;
            }).ToDictionary(a => a.GetFaunaFieldName().Replace("data.", ""), a => _context.ToFaunaObjOrPrimitive(a.GetValue(baseObj)));
            foreach (var binding in memberInit.Bindings.OfType<MemberAssignment>())
            {
                var propInfo = (PropertyInfo)binding.Member;
                var mapping = mappings[propInfo];
                var propType = propInfo.PropertyType;
                if (propType.Name.StartsWith("CompositeIndex")) continue;
                var propName = propInfo.GetFaunaFieldName();
                if (mapping.Type == DbPropertyType.Key || mapping.Type == DbPropertyType.Timestamp) continue;
                members[propName.Replace("data.", "")] = Accept(binding.Expression, varName);
            }

            return QueryModel.Obj(members);
        }

        private object Visit(MethodCallExpression methodCall, string varName)
        {
            var methodInfo = methodCall.Method;
            if ((methodCall.Object == null || methodCall.Object is ConstantExpression)
                && methodCall.Arguments.All(a => a is ConstantExpression || a is MemberExpression) 
                && methodCall.Arguments.OfType<MemberExpression>().All(a => a.Expression is ConstantExpression))
            {
                var fixedParams = FixConstantParameters(methodCall.Arguments);
                var callTarget = (methodCall.Object as ConstantExpression)?.Value;
                methodInfo.Invoke(callTarget, fixedParams);
            }
            if (!methodInfo.IsStatic)
                throw new UnsupportedMethodException("Can't call member method in FaunaDB query.");

            if (BuiltInFunctions.ContainsKey((methodInfo.DeclaringType, methodInfo.Name)))
                return BuiltInFunctions[(methodInfo.DeclaringType, methodInfo.Name)](methodCall.Arguments.Select(a => Accept(a, varName)).ToArray());

            var dbFunctionAttr = methodInfo.GetCustomAttribute<DbFunctionAttribute>();
            if(dbFunctionAttr == null) throw new UnsupportedMethodException(methodInfo.Name, "If this is a user defined function, add the DbFunction attribute.");
            var functionRef = QueryModel.Function(dbFunctionAttr.Name);
            return QueryModel.Call(functionRef, methodCall.Arguments.Select(a => Accept(a, varName)).ToArray());
        }

        private object Visit(NewArrayExpression newArray, string varName)
        {
            return newArray.NodeType == ExpressionType.NewArrayInit ? newArray.Expressions.Select(a => Accept(a, varName)) : Activator.CreateInstance(newArray.Type.MakeArrayType(), (int)((ConstantExpression)newArray.Expressions[0]).Value);
        }

        private object Visit(NewExpression newExpr)
        {
            if (!newExpr.Arguments.All(a => a is ConstantExpression || a is MemberExpression) || !newExpr.Arguments
                    .OfType<MemberExpression>().All(a => a.Expression is ConstantExpression))
                throw new UnsupportedMethodException(newExpr.Constructor.DeclaringType + ".ctor",
                    "Parameterised constructor with variable parameters not supported.");

            var parameters = FixConstantParameters(newExpr.Arguments);
            var obj = newExpr.Constructor.Invoke(parameters);
            return _context.ToFaunaObjOrPrimitive(obj);
        }
        
        private static object Visit(ParameterExpression p, string varName) => QueryModel.Var(varName);

        private static object Visit(TypeBinaryExpression typeBinary)
        {
            switch (typeBinary.Expression)
            {
                case ConstantExpression constType:
                    return typeBinary.TypeOperand.IsAssignableFrom((Type) constType.Value);
                case MemberExpression memberType when memberType.Expression is ConstantExpression cExp:
                    switch (memberType.Member)
                    {
                        case FieldInfo field:
                            return typeBinary.TypeOperand.IsAssignableFrom((Type)field.GetValue(cExp.Value));
                        case PropertyInfo prop:
                            return typeBinary.TypeOperand.IsAssignableFrom((Type)prop.GetValue(cExp.Value));
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private object Visit(UnaryExpression unary, string varName)
        {
            switch (unary.NodeType)
            {
                case ExpressionType.Not:
                    return QueryModel.Not(Accept(unary.Operand, varName));
                case ExpressionType.Quote:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    return Accept(unary.Operand, varName);
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    throw new UnsupportedMethodException(unary.NodeType.ToString(), CurrentlyUnsupportedError);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private object Accept(Expression expression, string varName)
        {
            switch (expression)
            {
                case BinaryExpression binary: return Visit(binary, varName);
                case ConditionalExpression conditional: return Visit(conditional, varName);
                case ConstantExpression constant: return Visit(constant);
                case DefaultExpression defaultExp: return Visit(defaultExp);
                case GotoExpression _: throw new UnsupportedMethodException("Goto", "Seriously?");
                case IndexExpression _: throw new UnsupportedMethodException("ArrayIndex", "List Indexing is currently unsupported.");
                case ListInitExpression listInit: return Visit(listInit, varName);
                case MemberExpression member: return Visit(member, varName);
                case MemberInitExpression memberInit: return Visit(memberInit, varName);
                case MethodCallExpression methodCall: return Visit(methodCall, varName);
                case NewArrayExpression newArray: return Visit(newArray, varName);
                case NewExpression newExpr: return Visit(newExpr);
                case ParameterExpression p: return Visit(p, varName);
                case SwitchExpression _: throw new UnsupportedMethodException("switch", "Switch expressions are not supported. Use if/else instead.");
                case TypeBinaryExpression typeBinary: return Visit(typeBinary);
                case UnaryExpression unary: return Visit(unary, varName);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static object[] FixConstantParameters(IEnumerable<Expression> args)
        {
            var fixedParams = new List<object>();
            foreach (var argument in args)
            {
                switch (argument)
                {
                    case ConstantExpression constantArg:
                        fixedParams.Add(constantArg.Value);
                        break;
                    case MemberExpression memberArg:
                        var obj = ((ConstantExpression) memberArg.Expression).Value;
                        switch (memberArg.Member)
                        {
                            case PropertyInfo propInfo:
                                fixedParams.Add(propInfo.GetValue(obj));
                                break;
                            case FieldInfo fieldInfo:
                                fixedParams.Add(fieldInfo.GetValue(obj));
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        break;
                }
            }

            return fixedParams.ToArray();
        }
    }
}