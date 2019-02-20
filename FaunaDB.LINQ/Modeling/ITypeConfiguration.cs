using System.Collections.Generic;
using System.Reflection;

namespace FaunaDB.LINQ.Modeling
{
    public interface ITypeConfiguration
    {
        /// <summary>
        /// Builds the type configuration. This is automatically called in the DbContext.
        /// </summary>
        /// <returns>The dictionary of type configuration entries</returns>
        Dictionary<PropertyInfo, TypeConfigurationEntry> Build();
    }
}