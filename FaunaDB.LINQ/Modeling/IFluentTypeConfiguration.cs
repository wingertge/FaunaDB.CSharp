using System;
using System.Linq.Expressions;

namespace FaunaDB.LINQ.Modeling
{
    public interface IFluentTypeConfiguration<T> : IFluentTypeConfiguration
    {
        IFluentTypeConfiguration<T> HasKey<TProperty>(Expression<Func<T, TProperty>> property);
        IFluentTypeConfiguration<T> HasIndex<TProperty>(Expression<Func<T, TProperty>> property, string indexName, string name = null);
        IFluentTypeConfiguration<T> HasCompositeIndex<TProperty>(Expression<Func<T, TProperty>> property, string indexName);
        IFluentTypeConfiguration<T> HasReference<TProperty>(Expression<Func<T, TProperty>> property, string name = null);
        IFluentTypeConfiguration<T> HasName<TProperty>(Expression<Func<T, TProperty>> property, string name);
        IFluentTypeConfiguration<T> HasTimestamp<TProperty>(Expression<Func<T, TProperty>> property);
    }

    public interface IFluentTypeConfiguration : ITypeConfiguration { }
}