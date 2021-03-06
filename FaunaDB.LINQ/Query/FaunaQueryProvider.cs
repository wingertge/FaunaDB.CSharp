﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace FaunaDB.LINQ.Query
{
    internal class FaunaQueryProvider : IQueryProvider
    {
        private readonly IDbContext _context;
        private readonly object _selector;

        internal FaunaQueryProvider(IDbContext context, object selector)
        {
            _context = context;
            _selector = selector;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = expression.Type;
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(FaunaQueryableData<>).MakeGenericType(elementType), this, expression);
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new FaunaQueryableData<TElement>(this, expression);

        public object Execute(Expression expression) => throw new InvalidOperationException("Can't execute non generic Fauna query.");

        public TResult Execute<TResult>(Expression expression)
        {
            return _context.Query<TResult>(FaunaQueryParser.Parse(_selector, expression, _context)).Result;
        }

        public Task<TResult> ExecuteAsync<TResult>(Expression expression) => _context.Query<TResult>(FaunaQueryParser.Parse(_selector, expression, _context));
    }
}