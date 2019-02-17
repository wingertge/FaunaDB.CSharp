using System.Collections.Generic;
using Newtonsoft.Json;

namespace FaunaDB.Driver.Errors
{
    public class QueryError
    {
        public IReadOnlyList<string> Position { get; }
        public string Code { get; }
        public string Description { get; }
        public IReadOnlyList<ValidationFailure> Failures { get; }

        [JsonConstructor]
        public QueryError(IReadOnlyList<string> position, string code, string description, IReadOnlyList<ValidationFailure> failures)
        {
            Position = position ?? new List<string>();
            Code = code;
            Description = description;
            Failures = failures ?? new List<ValidationFailure>();
        }
    }
}