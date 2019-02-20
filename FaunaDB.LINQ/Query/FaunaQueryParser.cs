using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FaunaDB.Driver;
using FaunaDB.Driver.Errors;
using FaunaDB.LINQ.Extensions;
using FaunaDB.LINQ.Modeling;
using FaunaDB.LINQ.Types;
using static FaunaDB.Driver.QueryModel;

namespace FaunaDB.LINQ.Query
{
    public class FaunaQueryParser
    {
        private int _lambdaIndex;
        private readonly IDbContext _context;
        private const string UnsupportedError = "Unsupported (likely won't change).";

        private static readonly Dictionary<(Type, string), Func<object, object, object>> BuiltInBinaryMethods = new Dictionary<(Type, string), Func<object, object, object>>
        {
            { (typeof(string), "Concat"), (left, right) => Concat(left, right) }
        };

        private static readonly Dictionary<(Type, string), Func<object[], object>> BuiltInFunctions = new Dictionary<(Type, string), Func<object[], object>>
        {
            { (typeof(string), "Concat"), Concat },
            { (typeof(Tuple), "Create"), a => a }
        };

        internal static Expr Parse(object selector, Expression expr, IDbContext context)
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
                    current = Drop(args[0].GetConstantValue<int>(), rest);
                    break;
                case "Take":
                    current = Take(args[0].GetConstantValue<int>(), rest);
                    break;
                case "Distinct":
                    current = Distinct(rest);
                    break;
                case "Include":
                case "AlsoInclude":
                    current = HandleInclude(args, rest, _context);
                    break;
                case "FromQuery":
                    current = ((Func<object, Expr>)((ConstantExpression) args[0]).Value).Invoke(rest);
                    break;
                case "At":
                    current = At(_context.ToFaunaObjOrPrimitive((DateTime) ((ConstantExpression) args[0]).Value), rest);
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
                        var fieldSelector = Select(mappings[a].GetFaunaFieldPath(), Var(lambdaArgName));
                        return a.Name == propInfo.Name
                            ? typeof(IEnumerable).IsAssignableFrom(propInfo.PropertyType)
                                ? Map(fieldSelector,
                                    Lambda($"arg{++_lambdaIndex}",
                                        If(Exists(Var($"arg{_lambdaIndex}")), Get(Var($"arg{_lambdaIndex}")), null)))
                                : If(Exists(fieldSelector), Get(fieldSelector), null) //perform safe include
                            : fieldSelector;
                    });

            var mappedObj = Obj(new Dictionary<string, object> {{"ref", Select("ref", Var(lambdaArgName))}, {"ts", Select("ts", Var(lambdaArgName))}, {"data", Obj(fields)}});

            return Map(rest, Lambda(lambdaArgName, mappedObj));
        }

        private static object HandlePaginate(IReadOnlyList<Expression> args, object rest)
        {
            var fromRef = (string)((ConstantExpression)args[0]).Value;
            var sortDirection = (ListSortDirection)((ConstantExpression)args[1]).Value;
            var size = (int)((ConstantExpression)args[2]).Value;
            var time = (DateTime)((ConstantExpression)args[3]).Value;
            var ts = time == default(DateTime) ? (DateTime?)null : time;

            return sortDirection == ListSortDirection.Ascending
                ? Paginate(rest, ts: ts == null ? null : Time(ts.Value.ToString("O")), size: size,
                    after: string.IsNullOrEmpty(fromRef) ? null : Ref(fromRef))
                : Paginate(rest, ts: ts == null ? null : Time(ts.Value.ToString("O")), size: size,
                    before: string.IsNullOrEmpty(fromRef) ? null : Ref(fromRef));
        }

        private object HandleSelect(IReadOnlyList<Expression> args, object rest)
        {
            var lambda = args[0] is UnaryExpression unary
                ? (LambdaExpression) unary.Operand
                : (LambdaExpression) args[0];
            var argName = $"arg{++_lambdaIndex}";

            return Map(rest, Lambda(argName, Accept(lambda.Body, argName)));
        }

        private object HandleWhere(IReadOnlyList<Expression> args, object rest)
        {
            var lambda = args[0] is LambdaExpression lexp ? lexp : (LambdaExpression)((UnaryExpression)args[0]).Operand;
            var body = (BinaryExpression) lambda.Body;
            return Filter(rest, Lambda($"arg{++_lambdaIndex}", Accept(body, $"arg{_lambdaIndex}")));
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
                    return Add(left, right);
                case ExpressionType.Divide:
                    return Divide(left, right);
                case ExpressionType.Modulo:
                    return Modulo(left, right);
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return Multiply(left, right);
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return Subtract(left, right);
                case ExpressionType.AndAlso:
                    return And(left, right);
                case ExpressionType.OrElse:
                    return Or(left, right);
                case ExpressionType.Equal:
                    return EqualsFn(left, right);
                case ExpressionType.NotEqual:
                    return Not(EqualsFn(left, right));
                case ExpressionType.GreaterThanOrEqual:
                    return GTE(left, right);
                case ExpressionType.GreaterThan:
                    return GT(left, right);
                case ExpressionType.LessThan:
                    return LT(left, right);
                case ExpressionType.LessThanOrEqual:
                    return LTE(left, right);
                case ExpressionType.Coalesce:
                    return If(EqualsFn(left, null), right, left);
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
        
        private object Visit(ConditionalExpression conditional, string varName) => If(Accept(conditional.Test, varName), Accept(conditional.IfTrue, varName), Accept(conditional.IfFalse, varName));
        private object Visit(ConstantExpression constant) => _context.ToFaunaObjOrPrimitive(constant.Value);
        private object Visit(DefaultExpression defaultExp) => _context.ToFaunaObjOrPrimitive(Expression.Lambda(defaultExp).Compile().DynamicInvoke());

        private object Visit(ListInitExpression listInit, string varName)
        {
            if (!IsDatabaseSide(listInit))
                return _context.ToFaunaObjOrPrimitive(Expression.Lambda(listInit).Compile().DynamicInvoke());
            var result = Expression.Lambda(listInit.NewExpression).Compile().DynamicInvoke();
            foreach (var initializer in listInit.Initializers)
            {
                var args = initializer.Arguments;
                initializer.AddMethod.Invoke(result, args.Select(a => Accept(a, varName)).ToArray());
            }
            return result;
        }

        private object Visit(MemberExpression member, string varName)
        {
            if (!IsDatabaseSide(member))
                return _context.ToFaunaObjOrPrimitive(GetLocalVariableValue(member));
            var memberInfo = member.Member;
            if (!(memberInfo is PropertyInfo prop))
                throw new ArgumentException("Can't use fields as selector.");
            var rest = Accept(member.Expression, varName);
            var mapping = _context.Mappings[member.Member.DeclaringType][prop];
            if (!IsParentReference(member))
                return Select(mapping.Name, rest);
            if (mapping.Type != DbPropertyType.Reference)
                return Select(mapping.GetFaunaFieldPath(), rest);
            if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType))
            {
                return Map(Select(mapping.GetFaunaFieldPath(), rest),
                    Lambda($"arg{++_lambdaIndex}", Get(Var($"arg{_lambdaIndex}"))));
            }

            return Get(Select(mapping.GetFaunaFieldPath(), rest));
        }

        private bool IsParentReference(MemberExpression member)
        {
            if (member.Expression is ParameterExpression)
                return true;
            var parent = member.Expression as MemberExpression;
            var parentPropInfo = parent.Member as PropertyInfo ??
                                 throw new ArgumentException("Can't use fields as selectors");

            return _context.Mappings[parent.Member.DeclaringType][parentPropInfo].Type == DbPropertyType.Reference;
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

            return Obj(members);
        }

        private object Visit(MethodCallExpression methodCall, string varName)
        {
            var methodInfo = methodCall.Method;
            if (!IsDatabaseSide(methodCall))
            {
                var result = Expression.Lambda(methodCall).Compile().DynamicInvoke();
                return _context.ToFaunaObjOrPrimitive(result);
            }
            if (!methodInfo.IsStatic)
                throw new UnsupportedMethodException("Can't call member method in FaunaDB query.");

            if (BuiltInFunctions.ContainsKey((methodInfo.DeclaringType, methodInfo.Name)))
                return BuiltInFunctions[(methodInfo.DeclaringType, methodInfo.Name)](methodCall.Arguments.Select(a => Accept(a, varName)).ToArray());

            var dbFunctionAttr = methodInfo.GetCustomAttribute<DbFunctionAttribute>();
            if(dbFunctionAttr == null) throw new UnsupportedMethodException(methodInfo.Name, "If this is a user defined function, add the DbFunction attribute.");
            var functionRef = Function(dbFunctionAttr.Name);
            return Call(functionRef, methodCall.Arguments.Select(a => Accept(a, varName)).ToArray());
        }

        private static bool IsDatabaseSide(Expression expr)
        {
            while (true)
            {
                switch (expr)
                {
                    case MemberExpression member:
                        expr = member.Expression;
                        continue;
                    case ParameterExpression _:
                        return true;
                    case NewExpression newExpression:
                        return newExpression.Arguments.Any(IsDatabaseSide);
                    case MethodCallExpression method:
                        return method.Arguments.Any(IsDatabaseSide) || IsDatabaseSide(method.Object);
                    case NewArrayExpression arr:
                        return arr.NodeType == ExpressionType.NewArrayInit && arr.Expressions.Any(IsDatabaseSide);
                    case ListInitExpression listInit:
                        return listInit.Initializers.Any(a => a.Arguments.Any(IsDatabaseSide));
                    default:
                        return false;
                }
            }
        }

        private object GetLocalVariableValue(Expression expr) => _context.ToFaunaObjOrPrimitive(Expression.Lambda<Func<object>>(Expression.Convert(expr, typeof(object))).Compile().Invoke());

        private object Visit(NewArrayExpression newArray, string varName)
        {
            return newArray.NodeType == ExpressionType.NewArrayInit ? newArray.Expressions.Select(a => Accept(a, varName)) : Activator.CreateInstance(newArray.Type.MakeArrayType(), (int)((ConstantExpression)newArray.Expressions[0]).Value);
        }

        private object Visit(NewExpression newExpr) => throw new UnsupportedMethodException("Can't call constructor with database side parameters");
        
        private static object Visit(ParameterExpression p, string varName) => Var(varName);

        private static object Visit(TypeBinaryExpression typeBinary)
        {
            if(IsDatabaseSide(typeBinary.Expression))
                throw new UnsupportedMethodException("Type checking operator unsupported on Database side data.");
            return Expression.Lambda<Func<bool>>(typeBinary).Compile().Invoke();
        }

        private object Visit(UnaryExpression unary, string varName)
        {
            switch (unary.NodeType)
            {
                case ExpressionType.Not:
                    return Not(Accept(unary.Operand, varName));
                case ExpressionType.Quote:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    return Accept(unary.Operand, varName);
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    return Subtract(0, Accept(unary.Operand, varName));
                default:
                    throw new UnsupportedMethodException($"Unsupported operation: {unary.NodeType}");
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
                case ListInitExpression listInit: return Visit(listInit, varName);
                case MemberExpression member: return Visit(member, varName);
                case MemberInitExpression memberInit: return Visit(memberInit, varName);
                case MethodCallExpression methodCall: return Visit(methodCall, varName);
                case NewArrayExpression newArray: return Visit(newArray, varName);
                case NewExpression newExpr: return Visit(newExpr);
                case ParameterExpression p: return Visit(p, varName);
                case TypeBinaryExpression typeBinary: return Visit(typeBinary);
                case UnaryExpression unary: return Visit(unary, varName);
                default:
                    throw new UnsupportedMethodException(expression.NodeType.ToString());
            }
        }
    }
}