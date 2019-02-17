using System.Collections.Generic;
using Newtonsoft.Json;

namespace FaunaDB.Driver.Errors
{
    public class ValidationFailure
    {
        public IReadOnlyList<string> Field { get; }
        public string Code { get; }
        public string Description { get; }

        [JsonConstructor]
        public ValidationFailure(IReadOnlyList<string> field, string code, string description)
        {
            Field = field ?? new List<string>();
            Code = code;
            Description = description;
        }
    }
}