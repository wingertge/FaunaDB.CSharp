using System.Collections.Generic;

namespace FaunaDB.Driver.Errors
{
    public struct QueryErrorResponse
    {
        public int StatusCode { get; private set; }
        public IReadOnlyList<QueryError> Errors { get; private set; }

        public QueryErrorResponse(int statusCode, IReadOnlyList<QueryError> errors)
        {
            StatusCode = statusCode;
            Errors = errors;
        }
    }
}