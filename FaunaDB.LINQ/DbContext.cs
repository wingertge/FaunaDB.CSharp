using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FaunaDB.Driver;
using FaunaDB.LINQ.Extensions;
using FaunaDB.LINQ.Modeling;

[assembly: InternalsVisibleTo("FaunaDB.LINQ.Tests")]
namespace FaunaDB.LINQ
{
    /// <summary>
    /// The database context
    /// </summary>
    public class DbContext : IDbContext
    {
        public Dictionary<Type, TypeConfiguration> Mappings { get; set; }

        private readonly IFaunaClient _client;

        /// <summary>
        /// For Unit Testing only - DO NOT USE
        /// </summary>
        /// <param name="client"></param>
        /// <param name="mappings"></param>
        public DbContext(IFaunaClient client, Dictionary<Type, TypeConfiguration> mappings)
        {
            _client = client;
            Mappings = mappings;
        }

        public virtual async Task<T> Query<T>(Expr query)
        {
            var result = await _client.Query(query);
            return this.Decode<T>(result);
        }

        /// <summary>
        /// Start building the database mappings
        /// </summary>
        /// <param name="client">T</param>
        /// <returns></returns>
        public static IDbContextBuilder StartBuilding(IFaunaClient client)
        {
            return new DbContextBuilder(client);
        }
    }
}