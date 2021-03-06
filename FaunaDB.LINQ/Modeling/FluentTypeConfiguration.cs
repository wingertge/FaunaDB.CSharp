﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FaunaDB.LINQ.Extensions;

namespace FaunaDB.LINQ.Modeling
{
    /// <summary>
    /// Base class for Fluent Type Mapping
    /// </summary>
    /// <typeparam name="T">Type of the database model to be mapped</typeparam>
    public class FluentTypeConfiguration<T> : IFluentTypeConfiguration<T>
    {
        private Dictionary<PropertyInfo, TypeConfigurationEntry> Configuration { get; } = new Dictionary<PropertyInfo, TypeConfigurationEntry>();

        public IFluentTypeConfiguration<T> HasKey<TProperty>(Expression<Func<T, TProperty>> property)
        {
            Configuration[property.GetPropertyInfo()] = new TypeConfigurationEntry
            {
                Type = ConfigurationType.Key,
                Name = "ref"
            };

            return this;
        }

        public IFluentTypeConfiguration<T> HasIndex<TProperty>(Expression<Func<T, TProperty>> property, string indexName, string name = null)
        {
            Configuration[property.GetPropertyInfo()] = new IndexTypeConfigurationEntry
            {
                Type = ConfigurationType.Index,
                Name = name,
                IndexName = indexName
            };

            return this;
        }

        public IFluentTypeConfiguration<T> HasCompositeIndex<TProperty>(Expression<Func<T, TProperty>> property, string indexName)
        {
            Configuration[property.GetPropertyInfo()] = new IndexTypeConfigurationEntry
            {
                Type = ConfigurationType.CompositeIndex,
                IndexName = indexName
            };

            return this;
        }

        public IFluentTypeConfiguration<T> HasReference<TProperty>(Expression<Func<T, TProperty>> property, string name = null)
        {
            Configuration[property.GetPropertyInfo()] = new TypeConfigurationEntry
            {
                Type = ConfigurationType.Reference,
                Name = name
            };

            return this;
        }

        public IFluentTypeConfiguration<T> HasName<TProperty>(Expression<Func<T, TProperty>> property, string name)
        {
            Configuration[property.GetPropertyInfo()] = new TypeConfigurationEntry
            {
                Type = ConfigurationType.NameOverride,
                Name = name
            };

            return this;
        }

        public IFluentTypeConfiguration<T> HasTimestamp<TProperty>(Expression<Func<T, TProperty>> property)
        {
            Configuration[property.GetPropertyInfo()] = new TypeConfigurationEntry
            {
                Type = ConfigurationType.Timestamp,
                Name = "ts"
            };

            return this;
        }

        public Dictionary<PropertyInfo, TypeConfigurationEntry> Build()
        {
            return Configuration;
        }
    }
}