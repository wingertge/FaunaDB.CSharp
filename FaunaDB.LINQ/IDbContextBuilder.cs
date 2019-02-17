using FaunaDB.LINQ.Modeling;

namespace FaunaDB.LINQ
{
    public interface IDbContextBuilder
    {
        void RegisterReferenceModel<T>();
        void RegisterMapping<TMapping>() where TMapping : class, IFluentTypeConfiguration, new();
        IDbContext Build();
    }
}