using System.Threading.Tasks;
using FaunaDB.Driver;
using FaunaDB.LINQ.Extensions;
using Moq;
using Newtonsoft.Json.Linq;

namespace FaunaDB.LINQ.Tests
{
    public static class IsolationUtils
    {
        internal delegate void TestAction(IDbContext context, ref Expr lastExpr);

        internal static void FakeAttributeClient(TestAction test, string json = "{}")
        {
            var clientMock = new Mock<IFaunaClient>();
            var baseContext = CreateAttributeContext(clientMock.Object);
            var mock = new Mock<DbContext>(clientMock.Object, baseContext.Mappings) {CallBase = true};

            Expr lastQuery = null;
            mock.Setup(a => a.Query<object>(It.IsAny<Expr>())).Returns((Expr q) =>
            {
                lastQuery = q;
                return Task.FromResult(mock.Object.Decode(json, typeof(object)));
            });

            test(mock.Object, ref lastQuery);
        }

        internal static void FakeManualClient(TestAction test, string json = "{}")
        {
            var clientMock = new Mock<IFaunaClient>();
            var baseContext = CreateMappingContext(clientMock.Object);
            var mock = new Mock<DbContext>(clientMock.Object, baseContext.Mappings) { CallBase = true };

            Expr lastQuery = null;
            mock.Setup(a => a.Query<object>(It.IsAny<Expr>())).Returns((Expr q) =>
            {
                lastQuery = q;
                return Task.FromResult(mock.Object.Decode(json, typeof(object)));
            });

            test(mock.Object, ref lastQuery);
        }

        internal static void FakeAttributeClient<T>(TestAction test, string json = "{}")
        {
            var clientMock = new Mock<IFaunaClient>();
            var baseContext = CreateAttributeContext(clientMock.Object);
            var mock = new Mock<DbContext>(clientMock.Object, baseContext.Mappings) { CallBase = true };

            Expr lastQuery = null;
            mock.Setup(a => a.Query<T>(It.IsAny<Expr>())).Returns((Expr q) =>
            {
                lastQuery = q;
                return Task.FromResult(mock.Object.Decode(JObject.Parse(json), typeof(T)));
            });

            test(mock.Object, ref lastQuery);
        }

        internal static void FakeManualClient<T>(TestAction test, string json = "{}")
        {
            var clientMock = new Mock<IFaunaClient>();
            var baseContext = CreateMappingContext(clientMock.Object);
            var mock = new Mock<DbContext>(clientMock.Object, baseContext.Mappings) { CallBase = true };

            Expr lastQuery = null;
            mock.Setup(a => a.Query<T>(It.IsAny<Expr>())).Returns((Expr q) =>
            {
                lastQuery = q;
                return Task.FromResult(mock.Object.Decode(JObject.Parse(json), typeof(T)));
            });

            test(mock.Object, ref lastQuery);
        }

        internal static IDbContext CreateAttributeContext(IFaunaClient mock)
        {
            var builder = DbContext.StartBuilding(mock);
            builder.RegisterReferenceModel<ReferenceModel>();
            builder.RegisterReferenceModel<PrimitivesReferenceModel>();
            builder.RegisterReferenceModel<ReferenceTypesReferenceModel>();
            builder.RegisterReferenceModel<ValueTypesReferenceModel>();
            builder.RegisterReferenceModel<NamedPropertyModel>();
            builder.RegisterReferenceModel<ModelWithUnmappedReference>();
            builder.RegisterReferenceModel<SupportedCustomCollectionModel>();
            builder.RegisterReferenceModel<SupportedCustomCollectionModel2>();
            builder.RegisterReferenceModel<UnsupportedCustomCollectionModel>();
            builder.RegisterReferenceModel<UnsupportedCustomCollectionModel2>();
            builder.RegisterReferenceModel<UnsupportedCustomCollectionModel3>();
            return builder.Build();
        }

        internal static IDbContext CreateMappingContext(IFaunaClient mock)
        {
            var builder = DbContext.StartBuilding(mock);
            builder.RegisterMapping<ReferenceModelMapping>();
            builder.RegisterMapping<PrimitivesReferenceModelMapping>();
            builder.RegisterMapping<ReferenceTypesReferenceModelMapping>();
            builder.RegisterMapping<ValueTypesReferenceModelMapping>();
            builder.RegisterMapping<NamedPropertyModelMapping>();
            builder.RegisterMapping<ModelWithUnmappedReferenceMapping>();
            builder.RegisterMapping<SupportedCustomCollectionModelMapping>();
            builder.RegisterMapping<SupportedCustomCollectionModelMapping2>();
            builder.RegisterMapping<UnsupportedCustomCollectionModelMapping>();
            builder.RegisterMapping<UnsupportedCustomCollectionModelMapping2>();
            builder.RegisterMapping<UnsupportedCustomCollectionModelMapping3>();
            return builder.Build();
        }
    }
}