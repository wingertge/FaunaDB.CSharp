using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FaunaDB.Driver;
using FaunaDB.LINQ.Errors;
using FaunaDB.LINQ.Modeling;
using Newtonsoft.Json.Linq;

namespace FaunaDB.LINQ.Extensions
{
    public static class SerializationExtensions
    {
        public static object ToFaunaObj(this IDbContext context, object obj)
        {
            if (!context.Mappings.TryGetValue(obj.GetType(), out var mappings))
                throw new InvalidMappingException("Trying to use unregistered type.");

            var fields = new Dictionary<string, object>();
            foreach (var prop in obj.GetType().GetProperties())
            {
                var mapping = mappings[prop];

                if(new [] { DbPropertyType.Key, DbPropertyType.Timestamp, DbPropertyType.CompositeIndex }.Contains(mapping.Type)) continue;

                var propValue = prop.GetValue(obj);
                var propName = mapping.Name;

                if (propValue == null)
                {
                    fields[propName] = null;
                    continue;
                }

                var propType = prop.PropertyType;

                if (propType.GetTypeInfo().IsPrimitive || propType == typeof(string))
                    fields[propName] = propValue;
                else if (propType == typeof(DateTime))
                    fields[propName] = QueryModel.Time(((DateTime)propValue).ToString("O"));
                else
                {
                    if (typeof(IEnumerable).IsAssignableFrom(propType))
                    {
                        fields[propName] = mapping.Type == DbPropertyType.Reference
                            ? ((IEnumerable) propValue).Cast<object>().Select(a => QueryModel.Ref(SelectId(a, context))).ToArray()
                            : ((IEnumerable) propValue).Cast<object>().Select(context.ToFaunaObjOrPrimitive).ToArray();
                        continue;
                    }

                    fields[propName] = mapping.Type == DbPropertyType.Reference
                        ? QueryModel.Ref(SelectId(propValue, context))
                        : context.ToFaunaObj(propValue);
                }
            }

            return QueryModel.Obj(fields);
        }

        private static object SelectId(object obj, IDbContext context)
        {
            if (!context.Mappings.TryGetValue(obj.GetType(), out var mapping))
                throw new InvalidMappingException($"Unregistered type \"{obj.GetType()}\" mapped as reference.");

            return mapping.Key.GetValue(obj);
        }

        public static T Decode<T>(this IDbContext context, RequestResult request)
        {
            return (T) Decode(context, JObject.Parse(request.ResponseContent), typeof(T));
        }

        public static dynamic Decode(this IDbContext context, JToken value, Type type)
        {
            var obj = Activator.CreateInstance(type);
            if(!context.Mappings.ContainsKey(type))
                throw new InvalidMappingException("Trying to decode to unregistered mapping.");

            var mappings = context.Mappings[type];

            foreach (var prop in type.GetProperties())
            {
                var faunaPath = mappings[prop].GetStringFieldPath();
                var current = value;
                var valid = true;
                foreach (var segment in faunaPath)
                {
                    if (current is JObject jObj && jObj.TryGetValue(segment, out current)) continue;
                    valid = false;
                    break;
                }
                if(!valid) continue;

                var propType = prop.PropertyType.GetTypeInfo();

                if (propType.IsPrimitive || prop.PropertyType == typeof(string))
                    prop.SetValue(obj, current.ToObject(prop.PropertyType));
                else if (prop.PropertyType == typeof(DateTime))
                    prop.SetValue(obj, DateTime.Parse(current.ToObject<QueryModel.TimeStampV>().Ts.ToString()).ToUniversalTime());
                else
                {
                    if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType))
                    {
                        var mapping = context.Mappings[prop.PropertyType.GetElementType() ?? throw new InvalidMappingException("Trying to decode unregistered type.")];
                        if (mapping.Any(a => a.Value.Type == DbPropertyType.Key))
                        {
                            var enumerable = current.ToObject<IEnumerable<JObject>>();
                            var result = enumerable.Select(item => Decode(context, item, prop.PropertyType.GetElementType()))
                                .ToList();
                            prop.SetValue(obj, (dynamic) result);
                        }
                        else prop.SetValue(obj, current.ToObject(prop.PropertyType));
                    }
                    else prop.SetValue(obj, Decode(context, current, prop.PropertyType));
                }
            }

            return obj;
        }

        internal static object ToFaunaObjOrPrimitive(this IDbContext context, object obj)
        {
            if (obj == null) return null;
            var type = obj.GetType();
            if (type.GetTypeInfo().IsPrimitive || type == typeof(string))
                return obj;
            return type == typeof(DateTime) 
                ? QueryModel.Time(((DateTime)obj).ToString("O")) 
                : context.ToFaunaObj(obj);
        }
    }
}