using System;
using System.Collections.Generic;
using System.Linq;
using FaunaDB.Driver;
using FaunaDB.LINQ.Modeling;

namespace FaunaDB.LINQ
{
    public class DbContextBuilder : IDbContextBuilder
    {
        private readonly Dictionary<Type, ITypeConfiguration> _configurations = new Dictionary<Type, ITypeConfiguration>();
        private readonly IFaunaClient _client;

        public DbContextBuilder(IFaunaClient client)
        {
            _client = client;
        }

        public void RegisterReferenceModel<T>()
        {
            _configurations[typeof(T)] = new AttributeTypeConfiguration(typeof(T));
        }

        public void RegisterMapping<TMapping, TModel>() where TMapping : class, IFluentTypeConfiguration<TModel>, new()
        {
            var configInstance = Activator.CreateInstance(typeof(TMapping)) as TMapping;
            _configurations[typeof(TModel)] = configInstance;
        }

        public IDbContext Build()
        {
            var overrides = _configurations.Select(a => (a.Key, a.Value.Build())).ToDictionary(a => a.Item1, a => a.Item2);

            return new DbContext(_client, new DbModelBuilder().Build(overrides));
        }
    }
}