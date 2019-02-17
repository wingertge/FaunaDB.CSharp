using System.Collections.Generic;
using System.Reflection;

namespace FaunaDB.LINQ.Modeling
{
    public class TypeConfigurationEntry
    {
        public virtual ConfigurationType Type { get; set; }
        public string Name { get; set; }
    }

    public enum ConfigurationType
    {
        CompositeIndex,
        Index,
        Key,
        Reference,
        NameOverride,
        Timestamp
    }
}