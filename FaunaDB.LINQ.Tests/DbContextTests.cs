using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using FaunaDB.Driver;
using Moq;
using Newtonsoft.Json;
using FaunaDB.LINQ.Extensions;
using Xunit;

namespace FaunaDB.LINQ.Tests
{
    public class DbContextTests
    {
        [Fact]
        public void TestQuery()
        {
            var testData = new Dictionary<string, object>
            {
                {"indexed1", "test1"},
                {"indexed2", "test2"}
            };
            var json = JsonConvert.SerializeObject(new {data = testData});
            MockAttributeContext(TestQuery_Run, json);
            MockManualContext(TestQuery_Run, json);
        }

        private static void TestQuery_Run(IDbContext context, ref Expr lastQuery)
        {
            var query = context.Query<ReferenceModel>(a => a.Indexed1 == "test1");
            var result = query.Provider.Execute<ReferenceModel>(query.Expression);
            var model = new ReferenceModel { Indexed1 = "test1", Indexed2 = "test2" }; // Indexed1: name attr "indexed1" | Indexed2: default name
            
            Assert.Equal(model, result);
        }

        private static void MockAttributeContext(IsolationUtils.TestAction action, string returns = null)
        {
            var clientMock = new Mock<IFaunaClient>();
            var context = IsolationUtils.CreateAttributeContext(clientMock.Object);
            Expr lastQuery = null;
            clientMock.Setup(a => a.Query(It.IsAny<Expr>())).Returns((Expr q) =>
            {
                lastQuery = q;
                return Task.FromResult(new RequestResult(
                    HttpMethod.Get,
                    "", 
                    new Dictionary<string, string>(),
                    "",
                    returns,
                    400, 
                    new Dictionary<string, IEnumerable<string>>(), 
                    DateTime.Now, 
                    DateTime.Now
                ));
            });
            
            action(context, ref lastQuery);
        }

        private static void MockManualContext(IsolationUtils.TestAction action, string returns = null)
        {
            var clientMock = new Mock<IFaunaClient>();
            var context = IsolationUtils.CreateMappingContext(clientMock.Object);
            Expr lastQuery = null;
            clientMock.Setup(a => a.Query(It.IsAny<Expr>())).Returns((Expr q) =>
            {
                lastQuery = q;
                return Task.FromResult(new RequestResult(
                    HttpMethod.Get,
                    "", 
                    new Dictionary<string, string>(),
                    "",
                    returns,
                    400, 
                    new Dictionary<string, IEnumerable<string>>(), 
                    DateTime.Now, 
                    DateTime.Now
                ));
            });
            
            action(context, ref lastQuery);
        }
    }
}