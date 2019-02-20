using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FaunaDB.LINQ.Query
{
    public class FaunaQueryableData<TData> : IQueryable<TData>
    {
        public IEnumerator<TData> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<TData>>(Expression)?.GetEnumerator() ?? new List<TData>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Provider.Execute<IEnumerable>(Expression).GetEnumerator();
        }

        public Expression Expression { get; }
        public Type ElementType => typeof(TData);
        public IQueryProvider Provider { get; }

        public FaunaQueryableData(IDbContext context, object selector)
        {
            Provider = new FaunaQueryProvider(context, selector);
            Expression = Expression.Constant(this);
        }

        public FaunaQueryableData(IQueryProvider provider, Expression expression)
        {
            Provider = provider;
            Expression = expression;
        }
    }

    public class FaunaQueryableData<TData, TCurrent> : FaunaQueryableData<TData>, IIncludeQuery<TData, TCurrent> {
        public FaunaQueryableData(IQueryProvider provider, Expression expression) : base(provider, expression) { }
    }
}