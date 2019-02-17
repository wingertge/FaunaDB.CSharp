using System.Linq;

namespace FaunaDB.LINQ.Query
{
    public interface IIncludeQuery<out TQuery, TSelected> : IQueryable<TQuery>
    {
        
    }
}