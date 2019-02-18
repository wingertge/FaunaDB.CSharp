using System;
using System.Linq;
using FaunaDB.Driver;
using FaunaDB.Driver.Errors;
using FaunaDB.LINQ.Extensions;
using FaunaDB.LINQ.Modeling;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;
using ListSortDirection = FaunaDB.LINQ.Types.ListSortDirection;
using static FaunaDB.Driver.QueryModel;

namespace FaunaDB.LINQ.Tests
{
    public class QueryTests
    {
        public QueryTests()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            typeof(QueryTests).GetProperties();
        }

        [Fact]
        public void SimplePaginateTest()
        {
            IsolationUtils.FakeAttributeClient(SimplePaginateTest_Run);
            IsolationUtils.FakeManualClient(SimplePaginateTest_Run);
        }

        private static void SimplePaginateTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var q = client.Query<ReferenceModel>(a => a.Indexed1 == "test1").Paginate(size: 5);
            var manual = Paginate(Map(Match(Index("index_1"), new object[]{"test1"}), Lambda("arg0", Get(Var("arg0")))), size: 5);

            q.Provider.Execute<object>(q.Expression);

            Assert.Equal(JsonConvert.SerializeObject(lastQuery), JsonConvert.SerializeObject(manual));
        }

        [Fact]
        public void FromRefPaginateTest()
        {
            IsolationUtils.FakeAttributeClient(FromRefPaginateTest_Run);
            IsolationUtils.FakeManualClient(FromRefPaginateTest_Run);
        }

        private static void FromRefPaginateTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var q = client.Query<ReferenceModel>(a => a.Indexed1 == "test1").Paginate(size: 5, fromRef: "testRef");
            var manual = Paginate(Map(Match(Index("index_1"), new object[]{"test1"}), Lambda("arg0", Get(Var("arg0")))), size: 5, after: Ref("testRef"));

            q.Provider.Execute<object>(q.Expression);

            Assert.Equal(JsonConvert.SerializeObject(lastQuery), JsonConvert.SerializeObject(manual));
        }

        [Fact]
        public void SortDirectionPaginateTest()
        {
            IsolationUtils.FakeAttributeClient(SortDirectionPaginateTest_Run);
            IsolationUtils.FakeManualClient(SortDirectionPaginateTest_Run);
        }

        private static void SortDirectionPaginateTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var q = client.Query<ReferenceModel>(a => a.Indexed1 == "test1").Paginate(size: 5, fromRef: "testRef",
                sortDirection: ListSortDirection.Descending);

            var manual = Paginate(Map(Match(Index("index_1"), new object[]{"test1"}), Lambda("arg0", Get(Var("arg0")))), size: 5, before: Ref("testRef"));

            q.Provider.Execute<object>(q.Expression);

            Assert.Equal(JsonConvert.SerializeObject(lastQuery), JsonConvert.SerializeObject(manual));
        }

        [Fact]
        public void DateTimePaginateTest()
        {
            IsolationUtils.FakeAttributeClient(DateTimePaginateTest_Run);
            IsolationUtils.FakeManualClient(DateTimePaginateTest_Run);
        }

        private static void DateTimePaginateTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var q = client.Query<ReferenceModel>(a => a.Indexed1 == "test1").Paginate(size: 5, timeStamp: new DateTime(2017, 1, 1));

            var manual = Paginate(Map(Match(Index("index_1"), new object[]{"test1"}), Lambda("arg0", Get(Var("arg0")))), size: 5, ts: Time(new DateTime(2017, 1, 1).ToString("O")));

            q.Provider.Execute<object>(q.Expression);

            Assert.Equal(JsonConvert.SerializeObject(lastQuery), JsonConvert.SerializeObject(manual));
        }

        [Fact]
        public void AllOptionsPaginateTest()
        {
            IsolationUtils.FakeAttributeClient(AllOptionsPaginateTest_Run);
            IsolationUtils.FakeManualClient(AllOptionsPaginateTest_Run);
        }

        private static void AllOptionsPaginateTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var q = client.Query<ReferenceModel>(a => a.Indexed1 == "test1").Paginate(size: 5, timeStamp: new DateTime(2017, 1, 1), fromRef: "testRef", sortDirection: ListSortDirection.Descending);

            var manual = Paginate(Map(Match(Index("index_1"), new object[]{"test1"}), Lambda("arg0", Get(Var("arg0")))), size: 5, ts: Time(new DateTime(2017, 1, 1).ToString("O")), before: Ref("testRef"));

            q.Provider.Execute<object>(q.Expression);

            Assert.Equal(JsonConvert.SerializeObject(lastQuery), JsonConvert.SerializeObject(manual));
        }

        [Fact]
        public void CatchAllWhereTest()
        {
            IsolationUtils.FakeAttributeClient(CatchAllWhereTest_Run);
            IsolationUtils.FakeManualClient(CatchAllWhereTest_Run);
        }

        private static void CatchAllWhereTest_Run(IDbContext client, ref Expr lastQuery)
        {
            //DO NOT USE CONST HERE, COMPILER OPTIMIZATION WILL BREAK THINGS
            int i1 = 1;
            int i2 = 2;
            var q = client.Query<ReferenceModel>(a => a.Indexed1 == "test1").Where(a =>
                (a.Indexed1 == "test1" && a.Indexed2 != "test2") ||
                (i2 > i1 && i1 < i2 && i1 <= i2 && i2 >= i1));

            var manual = Filter(Map(Match(Index("index_1"), new object[]{"test1"}), Lambda("arg0", Get(Var("arg0")))), Lambda("arg1", Or(
                And(
                    EqualsFn(
                        Select(new object[]{"data", "indexed1"}, Var("arg1")), "test1"
                    ),
                    Not(EqualsFn(Select(new object[]{"data", "indexed2"}, Var("arg1")), "test2"))
                ),
                And(
                    And(
                        And(
                            GT(2, 1),
                            LT(1, 2)
                        ),
                        LTE(1, 2)
                    ),
                    GTE(2, 1)
                )
            )));

            q.Provider.Execute<object>(q.Expression);

            var generated = JsonConvert.SerializeObject(lastQuery);

            Assert.Equal(JsonConvert.SerializeObject(manual), generated);
        }

        [Fact]
        public void SelectStringConcatTest()
        {
            IsolationUtils.FakeAttributeClient(SelectStringConcatTest_Run);
            IsolationUtils.FakeManualClient(SelectStringConcatTest_Run);
        }

        private static void SelectStringConcatTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var q1 = client.Query<ReferenceModel>(a => a.Indexed1 == "test1").Select(a => a.Indexed1 + "concat");
            var q2 = client.Query<ReferenceModel>(a => "test1" == a.Indexed1).Select(a => a.Indexed1 + "concat");

            var manual = Map(Map(Match(Index("index_1"), Arr("test1")), Lambda("arg0", Get(Var("arg0")))),
                Lambda("arg1", Concat(new object[]{Select(Arr("data", "indexed1"), Var("arg1")), "concat"})));

            q1.Provider.Execute<object>(q1.Expression);

            Assert.Equal(JsonConvert.SerializeObject(lastQuery), JsonConvert.SerializeObject(manual));

            q2.Provider.Execute<object>(q2.Expression);
            
            Assert.Equal(JsonConvert.SerializeObject(lastQuery), JsonConvert.SerializeObject(manual));
        }

        [Fact]
        public void MemberInitTest()
        {
            IsolationUtils.FakeAttributeClient(MemberInitTest_Run);
            IsolationUtils.FakeManualClient(MemberInitTest_Run);
        }

        private static void MemberInitTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var q = client.Query<ReferenceModel>(a => a.Indexed1 == "test1").Where(a => a == new ReferenceModel {Indexed1 = "test1"});
            var manual = Filter(Map(Match(Index("index_1"), Arr("test1")), Lambda("arg0", Get(Var("arg0")))), Lambda("arg1", EqualsFn(Var("arg1"), Obj("indexed1", "test1", "indexed2", null))));

            q.Provider.Execute<object>(q.Expression);

            Assert.Equal(JsonConvert.SerializeObject(lastQuery), JsonConvert.SerializeObject(manual));
        }

        [Fact]
        public void ChainedQueryTest()
        {
            IsolationUtils.FakeAttributeClient(ChainedQueryTest_Run);
            IsolationUtils.FakeManualClient(ChainedQueryTest_Run);
        }

        private static void ChainedQueryTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var q = client.Query<ReferenceModel>(a => a.Indexed1 == "test1").Where(a => a.Indexed1 == "test1").Select(a => a.Indexed1);

            var selectorManual = Map(Match(Index("index_1"), Arr("test1")), Lambda("arg0", Get(Var("arg0"))));
            var filterManual = Filter(selectorManual, Lambda("arg1", EqualsFn(Select(Arr("data", "indexed1"), Var("arg1")), "test1")));
            var selectManual = Map(filterManual, Lambda("arg2", Select(Arr("data", "indexed1"), Var("arg2"))));
            var manual = selectManual;

            q.Provider.Execute<object>(q.Expression);

            Assert.Equal(JsonConvert.SerializeObject(lastQuery), JsonConvert.SerializeObject(manual));
        }

        [Fact]
        public void SkipTakeQueryTest()
        {
            IsolationUtils.FakeAttributeClient(SkipTakeQueryTest_Run);
            IsolationUtils.FakeManualClient(SkipTakeQueryTest_Run);
        }

        private static void SkipTakeQueryTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var q = client.Query<ReferenceModel>(a => a.Indexed1 == "test1").Skip(3).Take(4);
            
            var selectorManual = Map(Match(Index("index_1"), Arr("test1")), Lambda("arg0", Get(Var("arg0"))));
            var skipManual = Drop(3, selectorManual);
            var takeManual = Take(4, skipManual);

            q.Provider.Execute<object>(q.Expression);
            
            Assert.Equal(JsonConvert.SerializeObject(lastQuery), JsonConvert.SerializeObject(takeManual));
        }

        [Fact]
        public void DistinctQueryTest()
        {
            IsolationUtils.FakeAttributeClient(DistinctQueryTest_Run);
            IsolationUtils.FakeManualClient(DistinctQueryTest_Run);
        }
        
        private static void DistinctQueryTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var q = client.Query<ReferenceModel>(a => a.Indexed1 == "test1").Distinct();
            
            var selectorManual = Map(Match(Index("index_1"), Arr("test1")), Lambda("arg0", Get(Var("arg0"))));
            var manual = Distinct(selectorManual);
            
            q.Provider.Execute<object>(q.Expression);
            
            Assert.Equal(JsonConvert.SerializeObject(lastQuery), JsonConvert.SerializeObject(manual));
        }

        [Fact]
        public void IncludeQueryTest()
        {
            IsolationUtils.FakeAttributeClient(IncludeQueryTest_Run);
            IsolationUtils.FakeManualClient(IncludeQueryTest_Run);
        }

        private static void IncludeQueryTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var q = client.Query<IncludeModel>(a => a.Indexed1 == "test1").Include(a => a.Reference).AlsoInclude(a => a.References);
            
            var selectorManual = Map(Match(Index("index_1"), Arr("test1")), Lambda("arg0", Get(Var("arg0"))));
            var includeManual = Map(selectorManual, Lambda("arg1", Obj("ref", Select("ref", Var("arg1")), "ts",
                Select("ts", Var("arg1")), "data", Obj(
                    "indexed1", Select(new object[] {"data", "indexed1"}, Var("arg1")),
                    "reference",
                    If(Exists(Select(new object[] {"data", "reference"}, Var("arg1"))),
                        Get(Select(new object[] {"data", "reference"}, Var("arg1"))), null),
                    "references", Select(new object[] {"data", "references"}, Var("arg1"))))));
            var includeAlsoManual = Map(includeManual, Lambda("arg2", Obj("ref", Select("ref", Var("arg2")), "ts",
                Select("ts", Var("arg2")), "data", Obj(
                    "indexed1", Select(new object[] {"data", "indexed1"}, Var("arg2")),
                    "reference", Select(new object[] {"data", "reference"}, Var("arg2")),
                    "references", Map(Select(new object[]{"data", "references"}, Var("arg2")), 
                        Lambda("arg3", If(Exists(Var("arg3")),
                            Get(Var("arg3")), null)))))));
            var manual = JsonConvert.SerializeObject(includeAlsoManual);
            
            q.Provider.Execute<object>(q.Expression);

            var automatic = JsonConvert.SerializeObject(lastQuery);
            Assert.Equal(manual, automatic);
        }

        [Fact]
        public void QueryFailureTest()
        {
            IsolationUtils.FakeAttributeClient(QueryFailureTest_Run);
            IsolationUtils.FakeManualClient(QueryFailureTest_Run);
        }

        private static void QueryFailureTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var i = "";

            Assert.Throws<ArgumentException>(() => client.Query<ReferenceModel>(a => a.Indexed1 != i));
            Assert.Throws<ArgumentException>(() => client.Query<ReferenceModel>(a => a.Id != DummyMethodCall()));
            Assert.Throws<ArgumentException>(() => client.Query<ReferenceModel>(a => a.Id != ""));
            Assert.Throws<UnsupportedMethodException>(() => client.Query<ReferenceModel>(a => a.Indexed1 != "" ^ a.Indexed2 == ""));
        }

        private static string DummyMethodCall() => "";

        private static object[] Arr(params object[] values) => values;

        [Fact]
        public static void CustomDbFunctionTest()
        {
            IsolationUtils.FakeAttributeClient(CustomDbFunctionTest_Run);
            IsolationUtils.FakeManualClient(CustomDbFunctionTest_Run);
        }
        
        private static void CustomDbFunctionTest_Run(IDbContext client, ref Expr lastQuery)
        {
            var q = client.Query<ReferenceModel>(a => a.Indexed1 == "test1").Select(a => CustomDbFunction(a.Indexed1));
            
            var selectorManual = Map(Match(Index("index_1"), Arr("test1")), Lambda("arg0", Get(Var("arg0"))));
            var selectManual = Map(selectorManual, Lambda("arg1", Call(Function("custom_function"), Select(Arr("data", "indexed1"), Var("arg1")))));
            var manual = selectManual;

            q.Provider.Execute<object>(q.Expression);
            
            Assert.Equal(JsonConvert.SerializeObject(manual), JsonConvert.SerializeObject(lastQuery));
        }

        [DbFunction("custom_function")]
        private static string CustomDbFunction(string arg) => throw new NotSupportedException();
    }
}