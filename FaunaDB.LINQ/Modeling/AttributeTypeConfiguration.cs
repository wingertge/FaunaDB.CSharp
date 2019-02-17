using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FaunaDB.LINQ.Types;
using Newtonsoft.Json;

namespace FaunaDB.LINQ.Modeling
{
    public class AttributeTypeConfiguration : IAttributeTypeConfiguration
    {
        private readonly Type _type;

        public AttributeTypeConfiguration(Type type)
        {
            _type = type;
        }

        public Dictionary<PropertyInfo, TypeConfigurationEntry> Build()
        {
            var configuration = new Dictionary<PropertyInfo, TypeConfigurationEntry>();
            foreach (var prop in _type.GetProperties())
            {
                var nameAttribute = prop.GetCustomAttribute(typeof(JsonPropertyAttribute)) as JsonPropertyAttribute;
                var name = nameAttribute?.PropertyName;

                var attributes = prop.GetCustomAttributes().ToList();

                if (attributes.Any(a => a is KeyAttribute))
                {
                    configuration[prop] = new TypeConfigurationEntry
                    {
                        Name = "ref",
                        Type = ConfigurationType.Key
                    };
                    continue;
                }

                if (attributes.Any(a => a is ReferenceAttribute))
                {
                    configuration[prop] = new TypeConfigurationEntry
                    {
                        Name = name,
                        Type = ConfigurationType.Reference
                    };
                    continue;
                }

                if (attributes.Any(a => a is IndexedAttribute))
                {
                    var indexName = prop.GetCustomAttribute<IndexedAttribute>().Name;
                    if (prop.PropertyType.Name.StartsWith("CompositeIndex"))
                    {
                        configuration[prop] = new IndexTypeConfigurationEntry
                        {
                            Name = name,
                            Type = ConfigurationType.CompositeIndex,
                            IndexName = indexName
                        };
                        continue;
                    }

                    configuration[prop] = new IndexTypeConfigurationEntry
                    {
                        Name = name,
                        Type = ConfigurationType.Index,
                        IndexName = indexName
                    };
                    continue;
                }

                if (attributes.Any(a => a is TimestampAttribute))
                {
                    configuration[prop] = new TypeConfigurationEntry
                    {
                        Name = "ts",
                        Type = ConfigurationType.Timestamp
                    };
                }
            }

            return configuration;
        }
    }
}