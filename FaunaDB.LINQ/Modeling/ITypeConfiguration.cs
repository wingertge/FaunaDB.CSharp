using System.Collections.Generic;
using System.Reflection;

namespace FaunaDB.LINQ.Modeling
{
    public interface ITypeConfiguration
    {
        Dictionary<PropertyInfo, TypeConfigurationEntry> Build();
    }
}