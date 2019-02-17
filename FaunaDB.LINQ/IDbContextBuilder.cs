using FaunaDB.LINQ.Modeling;

namespace FaunaDB.LINQ
{
    public interface IDbContextBuilder
    {
        void RegisterReferenceModel<T>();
        void RegisterMapping<TMapping, TModel>() where TMapping : class, IFluentTypeConfiguration<TModel>, new();
        IDbContext Build();
    }
}