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
    internal static class SerializationExtensions
    {
        internal static object ToFaunaObj(this IDbContext context, object obj)
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
                else if (IsTuple(propType))
                    fields[propName] = propType.GetProperties().Select(a => a.GetValue(obj)).ToArray();
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

        internal static T Decode<T>(this IDbContext context, RequestResult request)
        {
            return request == null ? default(T) : (T) Decode(context, JObject.Parse(request?.ResponseContent), typeof(T));
        }

        internal static dynamic Decode(this IDbContext context, JToken value, Type type)
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
                    if (typeof(IEnumerable).IsAssignableFrom(propType))
                    {
                        var elementType = propType.GetInterface(typeof(IEnumerable<>).Name).GetGenericArguments().Single();
                        TypeConfiguration mapping;
                        if (elementType.IsPrimitive || elementType == typeof(string) ||
                            prop.PropertyType == typeof(DateTime))
                            mapping = null;
                        else mapping = context.Mappings[elementType ?? throw new InvalidMappingException("Trying to decode unregistered type.")];
                        if (mapping != null && mapping.Any(a => a.Value.Type == DbPropertyType.Key))
                        {
                            var enumerable = current.ToObject<IEnumerable<JObject>>();
                            var result = enumerable.Select(item => Decode(context, item, elementType));
                            var castMethod = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(elementType);
                            var castResult = castMethod.Invoke(null, new object[] {result});

                            if (propType.IsArray)
                            {
                                var toArrayMethod = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(elementType);
                                var arrayResult = toArrayMethod.Invoke(null, new[] {castResult});
                                prop.SetValue(obj, arrayResult);
                            }
                            else
                            {
                                var constructor = propType.GetConstructor(new[] {typeof(IEnumerable<>).MakeGenericType(elementType)});
                                if(constructor == null)
                                    throw new InvalidMappingException("Sorry, custom IEnumerables are only supported if they have a constructor taking an IEnumerable.");
                                prop.SetValue(obj, constructor.Invoke(new[] {castResult}));
                            }
                        }
                        else if(elementType == typeof(DateTime))
                        {
                            var enumerable = current.ToObject<IEnumerable<QueryModel.TimeStampV>>();
                            var result = enumerable.Select(a => DateTime.Parse(a.Ts.ToString()).ToUniversalTime());
                            
                            if (propType.IsArray)
                            {
                                prop.SetValue(obj, result.ToArray());
                            }
                            else
                            {
                                var constructor = propType.GetConstructor(new[] {typeof(IEnumerable<>).MakeGenericType(elementType)});
                                if(constructor == null)
                                    throw new InvalidMappingException("Sorry, custom IEnumerables are only supported if they have a constructor taking an IEnumerable.");
                                prop.SetValue(obj, constructor.Invoke(new object[] {result}));
                            }
                        }
                        else
                        {
                            prop.SetValue(obj, current.ToObject(prop.PropertyType));
                        }
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
            if (typeof(IEnumerable).IsAssignableFrom(type))
                type = type.GetInterface(typeof(IEnumerable<>).Name).GetGenericArguments().Single();
            if (type.GetTypeInfo().IsPrimitive || type == typeof(string) || type == typeof(object))
                return obj;
            if (IsTuple(type))
                return type.GetProperties().Select(a => a.GetValue(obj)).ToArray();
            return type == typeof(DateTime) 
                ? QueryModel.Time(((DateTime)obj).ToString("O")) 
                : context.ToFaunaObj(obj);
        }

        private static bool IsTuple(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

                if (type.IsGenericType)
                {
                    var genType = type.GetGenericTypeDefinition();
                    if (genType == typeof(Tuple<>)
                        || genType == typeof(Tuple<,>)
                        || genType == typeof(Tuple<,,>)
                        || genType == typeof(Tuple<,,,>)
                        || genType == typeof(Tuple<,,,,>)
                        || genType == typeof(Tuple<,,,,,>)
                        || genType == typeof(Tuple<,,,,,,>)
                        || genType == typeof(Tuple<,,,,,,,>)
                        || genType == typeof(Tuple<,,,,,,,>))
                        return true;
                }

            return false;
        }
    }
}