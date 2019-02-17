using System;
using System.Collections.Generic;
using System.Reflection;
using FaunaDB.LINQ.Extensions;

namespace FaunaDB.LINQ.Modeling
{
    public class DbModelBuilder
    {
        private readonly Queue<Type> _registrationQueue = new Queue<Type>();
        private readonly List<Type> _registeredTypes = new List<Type>();

        public Dictionary<Type, TypeConfiguration> Build(Dictionary<Type, Dictionary<PropertyInfo, TypeConfigurationEntry>> overrides)
        {
            var result = new Dictionary<Type, TypeConfiguration>();
            foreach (var type in overrides.Keys)
                _registrationQueue.Enqueue(type);

            while (_registrationQueue.Count > 0)
            {
                var type = _registrationQueue.Dequeue();
                var typeOverrides = new Dictionary<PropertyInfo, TypeConfigurationEntry>();
                if (overrides.ContainsKey(type)) typeOverrides = overrides[type];

                result[type] = BuildType(type, typeOverrides);
                _registeredTypes.Add(type);
            }

            return result;
        }

        private TypeConfiguration BuildType(Type type, IReadOnlyDictionary<PropertyInfo, TypeConfigurationEntry> overrides)
        {
            var configuration = new TypeConfiguration();

            foreach (var property in type.GetProperties())
            {
                overrides.TryGetValue(property, out var @override);

                var name = @override?.Name ?? property.GetFaunaFieldName();

                switch (@override?.Type)
                {
                    case ConfigurationType.CompositeIndex:
                        configuration[property] = new IndexPropertyInfo {Name = name, Type = DbPropertyType.CompositeIndex, IndexName = (@override as IndexTypeConfigurationEntry)?.IndexName };
                        break;
                    case ConfigurationType.Key:
                        configuration[property] = new DbPropertyInfo {Name = name, Type = DbPropertyType.Key};
                        break;
                    case ConfigurationType.Timestamp:
                        configuration[property] = new DbPropertyInfo {Name = name, Type = DbPropertyType.Timestamp};
                        break;
                    case ConfigurationType.Reference:
                        configuration[property] = new DbPropertyInfo { Name = name, Type = DbPropertyType.Reference };
                        break;
                    case ConfigurationType.Index:
                        configuration[property] = new IndexPropertyInfo { Name = name, Type = DbPropertyType.PrimitiveIndex, IndexName = (@override as IndexTypeConfigurationEntry)?.IndexName};
                        break;
                    default:
                        if (property.PropertyType.GetTypeInfo().IsPrimitive || property.PropertyType == typeof(string))
                            configuration[property] = new DbPropertyInfo {Name = name, Type = DbPropertyType.Primitive};
                        else
                        {
                            var propertyType = property.PropertyType;
                            if (!_registeredTypes.Contains(propertyType) && !_registrationQueue.Contains(propertyType) &&
                                propertyType != type)
                                _registrationQueue.Enqueue(property.PropertyType);

                            configuration[property] = new DbPropertyInfo {Name = name, Type = DbPropertyType.ValueType};
                        }

                        break;
                }

                if (@override?.Type == ConfigurationType.Key) configuration.Key = property;
            }

            return configuration;
        }
    }

    public class TypeConfiguration : Dictionary<PropertyInfo, DbPropertyInfo>
    {
        public PropertyInfo Key { get; set; }
    }
}