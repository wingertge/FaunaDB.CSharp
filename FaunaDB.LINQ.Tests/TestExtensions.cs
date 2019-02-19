using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FaunaDB.LINQ.Tests
{
    public static class TestExtensions
    {
        private static readonly MethodInfo InvalidFunctionMethodInfo =
            typeof(TestExtensions).GetTypeInfo().GetDeclaredMethod(nameof(InvalidFunction));

        public static IQueryable<T> InvalidFunction<T>(this IQueryable<T> source)
        {
            return source.Provider.CreateQuery<T>(Expression.Call(
                null,
                InvalidFunctionMethodInfo.MakeGenericMethod(typeof(T)),
                source.Expression
            ));
        }
    }
}