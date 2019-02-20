using System;
using System.Linq.Expressions;

namespace FaunaDB.LINQ.Modeling
{
    public interface IFluentTypeConfiguration<T> : IFluentTypeConfiguration
    {
        /// <summary>
        /// Marks the selected property as a ref
        /// </summary>
        /// <param name="property">The property to be marked as the model ref</param>
        /// <typeparam name="TProperty">Type of the property to be marked as the ref</typeparam>
        /// <returns>The same type configurator instance</returns>
        IFluentTypeConfiguration<T> HasKey<TProperty>(Expression<Func<T, TProperty>> property);
        
        /// <summary>
        /// Marks the selected property as an index with the passed name
        /// </summary>
        /// <param name="property">The property to be marked as an index</param>
        /// <param name="indexName">Name of the database index</param>
        /// <param name="name">Name of the database property. Default: based on property name</param>
        /// <typeparam name="TProperty">Type of the property to be marked as an index</typeparam>
        /// <returns>The same type configurator instance</returns>
        IFluentTypeConfiguration<T> HasIndex<TProperty>(Expression<Func<T, TProperty>> property, string indexName, string name = null);
        
        /// <summary>
        /// Marks the selected property as a composite index with the given name
        /// </summary>
        /// <param name="property">The property to be marked as a composite index</param>
        /// <param name="indexName">Name of the database index</param>
        /// <typeparam name="TProperty">Type of the property to be marked as a composite index</typeparam>
        /// <returns>The same type configurator instance</returns>
        IFluentTypeConfiguration<T> HasCompositeIndex<TProperty>(Expression<Func<T, TProperty>> property, string indexName);
        
        /// <summary>
        /// Marks the selected property as a reference object
        /// </summary>
        /// <param name="property">The property to be marked as a reference object</param>
        /// <param name="name">Optional name of the database property</param>
        /// <typeparam name="TProperty">Type of the property to be marked as a reference object</typeparam>
        /// <returns>The same type configurator instance</returns>
        IFluentTypeConfiguration<T> HasReference<TProperty>(Expression<Func<T, TProperty>> property, string name = null);
        
        /// <summary>
        /// Overrides the default name on the selected property
        /// </summary>
        /// <param name="property">The property to have its name overriden</param>
        /// <param name="name">The name to be used in the database</param>
        /// <typeparam name="TProperty">Type of the property to have its name overriden</typeparam>
        /// <returns>The same type configurator instance</returns>
        IFluentTypeConfiguration<T> HasName<TProperty>(Expression<Func<T, TProperty>> property, string name);
        
        /// <summary>
        /// Mark the selected property as the object timestamp
        /// </summary>
        /// <param name="property">The property to be marked as a timestamp</param>
        /// <typeparam name="TProperty">Type of the property to be marked as a timestamp</typeparam>
        /// <returns>The same type configurator instance</returns>
        IFluentTypeConfiguration<T> HasTimestamp<TProperty>(Expression<Func<T, TProperty>> property);
    }

    public interface IFluentTypeConfiguration : ITypeConfiguration { }
}