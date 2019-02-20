using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FaunaDB.Driver;
using FaunaDB.LINQ.Modeling;
using FaunaDB.LINQ.Query;

namespace FaunaDB.LINQ
{
    public interface IDbContext
    {
        /// <summary>
        /// Do not use directly
        /// </summary>
        Dictionary<Type, TypeConfiguration> Mappings { get; set; }
        
        /// <summary>
        /// Query the database with passed expression. Don't use this unless you have to.
        /// </summary>
        /// <param name="query">The query to be run on the database</param>
        /// <typeparam name="T">The type of the model to be queried</typeparam>
        /// <returns>Query result</returns>
        Task<T> Query<T>(Expr query);
    }
}