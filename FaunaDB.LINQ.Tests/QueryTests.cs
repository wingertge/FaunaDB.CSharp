using System;
using System.Linq;
using FaunaDB.Driver;
using FaunaDB.LINQ.Extensions;
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
            var q = client.Query<ReferenceModel>(a => a.Indexed1 == "test1").Select(a => a.Indexed1 + "concat");

            var manual = Map(Map(Match(Index("index_1"), Arr("test1")), Lambda("arg0", Get(Var("arg0")))),
                Lambda("arg1", Concat(new object[]{Select(Arr("data", "indexed1"), Var("arg1")), "concat"})));

        q.Provider.Execute<object>(q.Expression);

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

        private static object[] Arr(params object[] values) => values;
    }
}