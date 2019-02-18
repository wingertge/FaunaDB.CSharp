using System;
using FaunaDB.Driver;
using FaunaDB.LINQ.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;
using static FaunaDB.Driver.QueryModel;

namespace FaunaDB.LINQ.Tests
{
    public class ClientExtensionTests
    {
        public ClientExtensionTests()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        [Fact]
        public void CompositeWithArgsTest()
        {
            IsolationUtils.FakeAttributeClient(CompositeWithArgsTest_Run);
            IsolationUtils.FakeManualClient(CompositeWithArgsTest_Run);
        }

        private static void CompositeWithArgsTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var q = client.Query<ReferenceModel>(a => a.CompositeIndex, "test1", "test2");

            q.Provider.Execute<object>(q.Expression);
            var parsed = lastQuery;

            var manual = Map(Match(Index("composite_index"), "test1", "test2"), Lambda("arg0", Get(Var("arg0"))));

            Assert.Equal(JsonConvert.SerializeObject(parsed), JsonConvert.SerializeObject(manual));
        } 

        [Fact]
        public void CompositeWithTupleTest()
        {
            IsolationUtils.FakeAttributeClient(CompositeWithTupleTest_Run);
            IsolationUtils.FakeManualClient(CompositeWithTupleTest_Run);
        }

        private static void CompositeWithTupleTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var q1 = client.Query<ReferenceModel>(a => a.CompositeIndex == Tuple.Create("test1", "test2"));
            var q2 = client.Query<ReferenceModel>(a => Tuple.Create("test1", "test2") == a.CompositeIndex);

            var manual = Map(Match(Index("composite_index"), "test1", "test2"), Lambda("arg0", Get(Var("arg0"))));
            q1.Provider.Execute<object>(q1.Expression);

            Assert.Equal(JsonConvert.SerializeObject(lastQuery), JsonConvert.SerializeObject(manual));
            
            q2.Provider.Execute<object>(q1.Expression);

            Assert.Equal(JsonConvert.SerializeObject(lastQuery), JsonConvert.SerializeObject(manual));
        }

        [Fact]
        public void SingleBooleanSelectorTest()
        {
            IsolationUtils.FakeAttributeClient(SingleBooleanSelectorTest_Run);
            IsolationUtils.FakeManualClient(SingleBooleanSelectorTest_Run);
        }

        private static void SingleBooleanSelectorTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var q = client.Query<ReferenceModel>(a => a.Indexed1 == "test2");
            q.Provider.Execute<object>(q.Expression);
            var parsed = lastQuery;
            var manual = Map(Match(Index("index_1"), Arr("test2")), Lambda("arg0", Get(Var("arg0"))));

            Assert.Equal(JsonConvert.SerializeObject(parsed), JsonConvert.SerializeObject(manual));
        }

        [Fact]
        public void MultiSelectorTest()
        {
            IsolationUtils.FakeAttributeClient(MultiSelectorTest_Run);
            IsolationUtils.FakeManualClient(MultiSelectorTest_Run);
        }

        private static void MultiSelectorTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var q1 = client.Query<ReferenceModel>(a => a.Indexed1 == "test1" && a.Indexed2 == "test2");
            var q2 = client.Query<ReferenceModel>(a => a.Indexed1 == "test1" || a.Indexed2 == "test2");

            var manual1 = Map(Intersection(Match(Index("index_1"), Arr("test1")), Match(Index("index_2"), Arr("test2"))), Lambda("arg0", Get(Var("arg0"))));
            var manual2 = Map(Union(Match(Index("index_1"), Arr("test1")), Match(Index("index_2"), Arr("test2"))), Lambda("arg0", Get(Var("arg0"))));

            q1.Provider.Execute<object>(q1.Expression);
            Assert.Equal(JsonConvert.SerializeObject(lastQuery), JsonConvert.SerializeObject(manual1));

            q2.Provider.Execute<object>(q2.Expression);
            Assert.Equal(JsonConvert.SerializeObject(lastQuery), JsonConvert.SerializeObject(manual2));
        }

        [Fact]
        public void RefQueryTest()
        {
            IsolationUtils.FakeAttributeClient(RefQueryTest_Run);
            IsolationUtils.FakeManualClient(RefQueryTest_Run);
        }

        private static void RefQueryTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var q = client.Query<ReferenceModel>("ref1");

            var manual = Get(Ref("ref1"));

            q.Provider.Execute<object>(q.Expression);
            Assert.Equal(JsonConvert.SerializeObject(lastQuery), JsonConvert.SerializeObject(manual));
        }

        [Fact]
        public void CreateTest()
        {
            IsolationUtils.FakeAttributeClient<ReferenceModel>(CreateTest_Run);
            IsolationUtils.FakeManualClient<ReferenceModel>(CreateTest_Run);
        }

        private static void CreateTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var model = new ReferenceModel {Indexed1 = "test1", Indexed2 = "test2"};
            var q = client.Create(model).Result;

            var manual = Create(Class("reference_model"), Obj("data", Obj("indexed1", "test1", "indexed2", "test2")));

            Assert.Equal(JsonConvert.SerializeObject(lastQuery), JsonConvert.SerializeObject(manual));
        }

        [Fact]
        public void UpdateTest()
        {
            IsolationUtils.FakeAttributeClient<ReferenceModel>(UpdateTest_Run);
            IsolationUtils.FakeManualClient<ReferenceModel>(UpdateTest_Run);
        }

        private static void UpdateTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var model = new ReferenceModel {Id = "testId", Indexed1 = "test1", Indexed2 = "test2"};
            var q = client.Update(model).Result;

            var manual = Update(Ref(model.Id), client.ToFaunaObj(model));

            Assert.Equal(JsonConvert.SerializeObject(lastQuery), JsonConvert.SerializeObject(manual));
        }

        [Fact]
        public void UpsertTest()
        {
            IsolationUtils.FakeAttributeClient<ReferenceModel>(UpsertTest_Run);
            IsolationUtils.FakeManualClient<ReferenceModel>(UpsertTest_Run);
        }

        private static void UpsertTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var model = new ReferenceModel { Id = "testId", Indexed1 = "test1", Indexed2 = "test2" };
            var q = client.Upsert(model);

            var manual = If(Exists(Ref(model.Id)), Update(Ref(model.Id), Obj("indexed1", "test1", "indexed2", "test2")),
                Create(Class("reference_model"), Obj("indexed1", "test1", "indexed2", "test2")));

            Assert.Equal(JsonConvert.SerializeObject(lastQuery), JsonConvert.SerializeObject(manual));
        }

        [Fact]
        public void SelectorFailureTest()
        {
            IsolationUtils.FakeAttributeClient(SelectorFailureTest_Run);
            IsolationUtils.FakeManualClient(SelectorFailureTest_Run);
        }

        private static void SelectorFailureTest_Run(IDbContext client, ref Expr lastQuery)
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var task = client.Upsert(new ReferenceModel(), a => bool.Parse(a.Indexed1));
                task.Wait();
                var result = task.Result;
            });
            
            Assert.Throws<ArgumentException>(() =>
            {
                var task = client.Upsert(new ReferenceModel(), a => bool.Parse(a.Indexed1), "");
                task.Wait();
                var result = task.Result;
            });
            
            Assert.Throws<ArgumentException>(() =>
            {
                var task = client.Upsert(new ReferenceModel(), a => a.Id, "");
                task.Wait();
                var result = task.Result;
            });

            Assert.Throws<ArgumentException>(() =>
            {
                var q = client.Query<ReferenceModel>(a => bool.Parse(a.Indexed1));
                q.Provider.Execute<ReferenceModel>(q.Expression);
            });
            
            Assert.Throws<ArgumentException>(() =>
            {
                var q = client.Query<ReferenceModel>(a => bool.Parse(a.Indexed1), "");
                q.Provider.Execute<ReferenceModel>(q.Expression);
            });
            
            Assert.Throws<ArgumentException>(() =>
            {
                var q = client.Query<ReferenceModel>(a => a.Id, "");
                q.Provider.Execute<ReferenceModel>(q.Expression);
            });
        }

        [Fact]
        public void DeleteTest()
        {
            IsolationUtils.FakeAttributeClient(DeleteTest_Run);
            IsolationUtils.FakeManualClient(DeleteTest_Run);
        }

        private static void DeleteTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var model = new ReferenceModel { Id = "testId", Indexed1 = "test1", Indexed2 = "test2" };
            var q = client.Delete(model);

            var manual = Delete(Ref(model.Id));

            Assert.Equal(JsonConvert.SerializeObject(lastQuery), JsonConvert.SerializeObject(manual));
        }

        [Fact]
        public void GetTest()
        {
            IsolationUtils.FakeAttributeClient<ReferenceModel>(GetTest_Run);
            IsolationUtils.FakeManualClient<ReferenceModel>(GetTest_Run);
        }

        private static void GetTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var q = client.Get<ReferenceModel>("test1");

            var manual = Get(Ref("test1"));

            Assert.Equal(JsonConvert.SerializeObject(lastQuery), JsonConvert.SerializeObject(manual));
        }

        [Fact]
        public void UpsertCompositeIndexWithArgsTest()
        {
            IsolationUtils.FakeAttributeClient<ReferenceModel>(UpsertCompositeIndexWithArgsTest_Run);
            IsolationUtils.FakeManualClient<ReferenceModel>(UpsertCompositeIndexWithArgsTest_Run);
        }

        private static void UpsertCompositeIndexWithArgsTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var model = new ReferenceModel { Indexed1 = "test1", Indexed2 = "test2" };
            var q = client.Upsert(model, a => a.CompositeIndex, "test1", "test2");

            var obj = Obj("indexed1", "test1", "indexed2", "test2");
            var matchExpr = Match(Index("composite_index"), "test1", "test2");
            var manual = If(Exists(matchExpr), Map(matchExpr, Lambda("arg0", Update(Var("arg0"), obj))), Create(Class("reference_model"), obj));

            Assert.Equal(JsonConvert.SerializeObject(lastQuery), JsonConvert.SerializeObject(manual));
        }

        [Fact]
        public void UpsertBooleanExpressionTest()
        {
            IsolationUtils.FakeAttributeClient<ReferenceModel>(UpsertBooleanExpressionTest_Run);
            IsolationUtils.FakeManualClient<ReferenceModel>(UpsertBooleanExpressionTest_Run);
        }

        private static void UpsertBooleanExpressionTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var model = new ReferenceModel { Indexed1 = "test1", Indexed2 = "test2" };
            var q = client.Upsert(model, a => a.Indexed1 == "test1");

            var obj = Obj("indexed1", "test1", "indexed2", "test2");
            var matchExpr = Match(Index("index_1"), Arr("test1"));
            var manual = If(Exists(matchExpr), Map(matchExpr, Lambda("arg0", Update(Var("arg0"), obj))),
                Create(Class("reference_model"), obj));

            Assert.Equal(JsonConvert.SerializeObject(lastQuery), JsonConvert.SerializeObject(manual));
        }
        
        private static object[] Arr(params object[] values) => values;
    }
}
