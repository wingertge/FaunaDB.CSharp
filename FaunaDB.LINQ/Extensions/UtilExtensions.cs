using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using FaunaDB.Driver;
using FaunaDB.LINQ.Modeling;
using FaunaDB.LINQ.Query;
using Newtonsoft.Json;

[assembly:InternalsVisibleTo("FaunaDB.Client.LINQ.Tests")]

namespace FaunaDB.LINQ.Extensions
{
    public static class UtilExtensions
    {

        internal static PropertyInfo GetPropertyInfo<TSource, TProperty>(this Expression<Func<TSource, TProperty>> propertyLambda)
        {
            if (!(propertyLambda.Body is MemberExpression member))
                throw new ArgumentException(
                    $"Expression '{propertyLambda}' refers to a method, not a property.");

            return GetPropertyInfo(member);
        }

        internal static PropertyInfo GetPropertyInfo(this MemberExpression memberExp)
        {
            if (!(memberExp.Member is PropertyInfo propInfo))
                throw new ArgumentException(
                    $"Member '{memberExp}' refers to a field, not a property.");

            return propInfo;
        }

        internal static string GetFaunaFieldName(this PropertyInfo propInfo)
        {
            return propInfo.Name.ToLowerUnderscored();
        }

        internal static string ToLowerUnderscored(this string s)
        {
            return string.Concat(s.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }

        internal static object[] GetFaunaFieldPath(this DbPropertyInfo propInfo)
        {
            return GetStringFieldPath(propInfo).Cast<object>().ToArray();
        }

        internal static string[] GetStringFieldPath(this DbPropertyInfo propInfo)
        {
            return propInfo.Type == DbPropertyType.Key || propInfo.Type == DbPropertyType.Timestamp
                ? new [] { propInfo.Name }
                : new [] { "data", propInfo.Name };
        }

        internal static object GetClassRef(this object obj)
        {
            return QueryModel.Class(obj.GetType().Name.ToLowerUnderscored());
        }

        internal static T GetConstantValue<T>(this Expression expression)
        {
            return (T) ((ConstantExpression) expression).Value;
        }

        internal static bool IsReferenceType(this IDbContext context, Type type)
        {
            if (type.IsPrimitive || type == typeof(string)) return false;
            var mapping = context.Mappings[type];
            return mapping.Any(a => a.Value.Type == DbPropertyType.Key);
        }
    }
}