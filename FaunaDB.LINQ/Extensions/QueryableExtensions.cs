using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using FaunaDB.Driver;
using FaunaDB.LINQ.Query;
using FaunaDB.LINQ.Types;

namespace FaunaDB.LINQ.Extensions
{
    public static class QueryableExtensions
    {
        private static readonly MethodInfo PaginateMethodInfo =
            typeof(QueryableExtensions).GetTypeInfo().GetDeclaredMethod(nameof(Paginate));

        /// <summary>
        /// Fetch a single page of results
        /// </summary>
        /// <param name="source">The source queryable object</param>
        /// <param name="fromRef">Ref to start from. Default: First Page</param>
        /// <param name="sortDirection">Direction to sort the pages. Default: Ascending</param>
        /// <param name="size">Size of the page. Default: 16</param>
        /// <param name="timeStamp">Timestamp to start from. Default: All</param>
        /// <typeparam name="T">Type of the model to be queried</typeparam>
        /// <returns>Paginated FaunaDB Query object</returns>
        public static IQueryable<T> Paginate<T>(this IQueryable<T> source, string fromRef = "",
            ListSortDirection sortDirection = ListSortDirection.Ascending, int size = 16, DateTime timeStamp = default(DateTime))
        {
            return source.Provider.CreateQuery<T>(Expression.Call(
                null,
                PaginateMethodInfo.MakeGenericMethod(typeof(T)), 
                source.Expression, 
                Expression.Constant(fromRef), 
                Expression.Constant(sortDirection),
                Expression.Constant(size),
                Expression.Constant(timeStamp)
                )
            );
        }

        private static readonly MethodInfo IncludeMethodInfo =
            typeof(QueryableExtensions).GetTypeInfo().GetDeclaredMethod(nameof(Include));

        /// <summary>
        /// Includes reference property in query result
        /// </summary>
        /// <param name="source">The source queryable object</param>
        /// <param name="selector">Property selector for the reference to be included</param>
        /// <typeparam name="T">Type of the query object</typeparam>
        /// <typeparam name="TSelected">Type of the selected property</typeparam>
        /// <returns>A FaunaDB LINQ Query object including the referenced property</returns>
        public static IIncludeQuery<T, TSelected> Include<T, TSelected>(this IQueryable<T> source, Expression<Func<T, TSelected>> selector)
        {
            return new FaunaQueryableData<T, TSelected>(source.Provider, Expression.Call(
                null,
                IncludeMethodInfo.MakeGenericMethod(typeof(T), typeof(TSelected)),
                source.Expression,
                selector
            ));
        }

        private static readonly MethodInfo AlsoIncludeMethodInfo =
            typeof(QueryableExtensions).GetTypeInfo().GetDeclaredMethod(nameof(AlsoInclude));

        /// <summary>
        /// Includes reference property of reference property in query result
        /// </summary>
        /// <param name="source">The source queryable object</param>
        /// <param name="selector">Property selector for the reference to be included</param>
        /// <typeparam name="TOrigin">Type of the query object</typeparam>
        /// <typeparam name="TCurrent">Type of the object to select a property on</typeparam>
        /// <typeparam name="TSelected">Type of the selected property</typeparam>
        /// <returns>A FaunaDB LINQ Query object including the referenced property</returns>
        public static IIncludeQuery<TOrigin, TSelected> AlsoInclude<TOrigin, TCurrent, TSelected>(this IIncludeQuery<TOrigin, TCurrent> source, Expression<Func<TCurrent, TSelected>> selector)
        {
            return new FaunaQueryableData<TOrigin, TSelected>(source.Provider, Expression.Call(
                null,
                AlsoIncludeMethodInfo.MakeGenericMethod(typeof(TOrigin), typeof(TCurrent), typeof(TSelected)),
                source.Expression,
                selector
            ));
        }

        private static readonly MethodInfo FromQueryMethodInfo =
            typeof(QueryableExtensions).GetTypeInfo().GetDeclaredMethod(nameof(FromQuery));

        /// <summary>
        /// Includes a custom low level query in the LINQ query object
        /// </summary>
        /// <param name="source">The source queryable object</param>
        /// <param name="query">The function returning the custom query. The parameter represents the remaining LINQ query.</param>
        /// <typeparam name="TIn">Type of the objects to be queried on</typeparam>
        /// <typeparam name="TOut">Type of the custom query result</typeparam>
        /// <returns>A FaunaDB LINQ Query object including the custom query</returns>
        public static IQueryable<TOut> FromQuery<TIn, TOut>(this IQueryable<TIn> source, Func<object, Expr> query)
        {
            return source.Provider.CreateQuery<TOut>(Expression.Call(
                null,
                FromQueryMethodInfo.MakeGenericMethod(typeof(TIn), typeof(TOut)),
                source.Expression,
                Expression.Constant(query)
            ));
        }

        private static readonly MethodInfo AtMethodInfo =
            typeof(QueryableExtensions).GetTypeInfo().GetDeclaredMethod(nameof(At));

        /// <summary>
        /// Converts query to a temporal query at given snapshot time 
        /// </summary>
        /// <param name="source">The source queryable object</param>
        /// <param name="timeStamp">The snapshot time of the temporal query</param>
        /// <typeparam name="T">Type of the objects queried</typeparam>
        /// <returns>A FaunaDB LINQ Query object at the given time stamp</returns>
        public static IQueryable<T> At<T>(this IQueryable<T> source, DateTime timeStamp)
        {
            return source.Provider.CreateQuery<T>(Expression.Call(
                null,
                AtMethodInfo.MakeGenericMethod(typeof(T)),
                source.Expression,
                Expression.Constant(timeStamp.ToUniversalTime())
            ));
        }

        /// <summary>
        /// Returns the first database object in the query, or the default value if none exist
        /// </summary>
        /// <param name="source">The source queryable object</param>
        /// <typeparam name="T">Type of the objects queried</typeparam>
        /// <returns>The first object in the query, or the default value if none exist</returns>
        public static async Task<T> FirstOrDefaultAsync<T>(this IQueryable<T> source)
        {
            var result = await ExecuteAsync<T, IEnumerable<T>>(source.Paginate(size: 1).GetAll());
            return result.FirstOrDefault();
        }

        /// <summary>
        /// Asynchronously enumerates the query into a list.
        /// </summary>
        /// <param name="source">The source queryable object</param>
        /// <typeparam name="T">Type of the objects queried</typeparam>
        /// <returns>A list of database objects returned from the query</returns>
        public static Task<List<T>> ToListAsync<T>(this IQueryable<T> source)
        {
            return ExecuteAsync<T, List<T>>(source);
        }

        /// <summary>
        /// Asynchronously checks if any elements exist in the query
        /// </summary>
        /// <param name="source">The source queryable object</param>
        /// <typeparam name="T">Type of the objects queried</typeparam>
        /// <returns>True if any elements exist in the query, else false</returns>
        public static async Task<bool> AnyAsync<T>(this IQueryable<T> source)
        {
            var result = await ExecuteAsync<T, IEnumerable<T>>(source.Paginate(size: 1));
            return result.Any();
        }

        private static readonly MethodInfo GetAllMethodInfo =
            typeof(QueryableExtensions).GetTypeInfo().GetDeclaredMethod(nameof(GetAll));

        private static IQueryable<T> GetAll<T>(this IQueryable<T> source)
        {
            return source.Provider.CreateQuery<T>(Expression.Call(
                instance: null,
                method: GetAllMethodInfo.MakeGenericMethod(typeof(T)),
                arguments: source.Expression
            ));
        }

        private static Task<TTarget> ExecuteAsync<TSource, TTarget>(IQueryable<TSource> source)
        {
            return source.Provider is FaunaQueryProvider provider
                ? provider.ExecuteAsync<TTarget>(source.Expression)
                : Task.Run(() => source.Provider.Execute<TTarget>(source.Expression));
        }
    }
}