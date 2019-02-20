using FaunaDB.LINQ.Modeling;

namespace FaunaDB.LINQ
{
    public interface IDbContextBuilder
    {
        /// <summary>
        /// Registers a model as a reference type and generates mappings from attributes
        /// </summary>
        /// <typeparam name="T">Type of the model to be registered</typeparam>
        void RegisterReferenceModel<T>();
        
        /// <summary>
        /// Registers a fluent model mapping
        /// </summary>
        /// <typeparam name="TMapping">Type of the fluent configuration mapping</typeparam>
        void RegisterMapping<TMapping>() where TMapping : class, IFluentTypeConfiguration, new();
        
        /// <summary>
        /// Builds a DB Context with registered models
        /// </summary>
        /// <returns></returns>
        IDbContext Build();
    }
}