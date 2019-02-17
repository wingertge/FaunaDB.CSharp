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
        Dictionary<Type, TypeConfiguration> Mappings { get; set; }
        Task<T> Query<T>(Expr query);
    }
}