using System;
using System.Collections.Generic;
using FaunaDB.Driver;
using FaunaDB.LINQ.Errors;
using FaunaDB.LINQ.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Xunit;
using static FaunaDB.Driver.QueryModel;

namespace FaunaDB.LINQ.Tests
{
    public class SerializationTests
    {
        public SerializationTests()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        [Fact]
        public void BasicReferenceTypeSerializationTest()
        {
            IsolationUtils.FakeAttributeClient(BasicReferenceTypeSerializationTest_Run);
            IsolationUtils.FakeManualClient(BasicReferenceTypeSerializationTest_Run);
        }

        private static void BasicReferenceTypeSerializationTest_Run(IDbContext context, ref Expr lastQuery)
        {
            var model = new ReferenceModel { Indexed1 = "test1", Indexed2 = "test2" }; // Indexed1: name attr "indexed1" | Indexed2: default name

            var manual = Obj("indexed1", "test1", "indexed2", "test2");

            var auto = context.ToFaunaObj(model);
            Assert.Equal(JsonConvert.SerializeObject(manual), JsonConvert.SerializeObject(auto));
        }
        
        [Fact]
        public void ReferenceTypeWithAllPrimitiveTypesTest()
        {
            IsolationUtils.FakeAttributeClient(ReferenceTypeWithAllPrimitiveTypesTest_Run);
            IsolationUtils.FakeManualClient(ReferenceTypeWithAllPrimitiveTypesTest_Run);
        }

        private void ReferenceTypeWithAllPrimitiveTypesTest_Run(IDbContext context, ref Expr lastQuery)
        {
            var model = new PrimitivesReferenceModel
            {
                BooleanVal = true,
                ByteVal = 10,
                CharVal = 'a',
                DateTimeVal = DateTime.MaxValue,
                DoubleVal = 123.4,
                FloatVal = 234.5f,
                LongVal = 123,
                IntVal = 234,
                SByteVal = 1,
                UIntVal = 2,
                UShortVal = 3,
                ULongVal = 4,
                ShortVal = 5,
                StringVal = "test1"
            };

            var dict = new Dictionary<string, object>
            {
                {"string_val", "test1"},
                {"int_val", 234},
                {"boolean_val", true},
                {"byte_val", 10},
                {"long_val", 123},
                {"float_val", 234.5f},
                {"double_val", 123.4},
                {"date_time_val", Time(DateTime.MaxValue.ToString("O"))},
                {"short_val", 5},
                {"u_short_val", 3},
                {"u_int_val", 2},
                {"u_long_val", 4}, // bizarre issue with conversion, ulong will implicit to double for some reason
                {"s_byte_val", 1},
                {"char_val", "a"},
            };

            var manual = Obj(dict);

            var auto = context.ToFaunaObj(model);

            Assert.Equal(JsonConvert.SerializeObject(manual), JsonConvert.SerializeObject(auto));
        }

        [Fact]
        public void ReferenceTypeWithValueTypesTest()
        {
            IsolationUtils.FakeAttributeClient(ReferenceTypeWithValueTypesTest_Run);
            IsolationUtils.FakeManualClient(ReferenceTypeWithValueTypesTest_Run);
        }

        private void ReferenceTypeWithValueTypesTest_Run(IDbContext context, ref Expr lastQuery)
        {
            var model = new ValueTypesReferenceModel
            {
                ValueModel = new ValueModel { Value1 = "test1", Value2 = "test2" },
                ValueModels1 = new List<ValueModel> { new ValueModel { Value1 = "test3", Value2 = "test4" }, new ValueModel { Value1 = "test5", Value2 = "test6" } },
                ValueModels2 = new[] { new ValueModel { Value1 = "test7", Value2 = "test8" }, new ValueModel { Value1 = "test9", Value2 = "test10" } }
            };

            var manual = Obj("value_model", Obj("value1", "test1", "value2", "test2"), "value_models1",
                new object[]{Obj("value1", "test3", "value2", "test4"), Obj("value1", "test5", "value2", "test6")},
                "value_models2", new object[]{Obj("value1", "test7", "value2", "test8"), Obj("value1", "test9", "value2", "test10")});

            Assert.Equal(JsonConvert.SerializeObject(manual), JsonConvert.SerializeObject(context.ToFaunaObj(model)));         
        }

        [Fact]
        public void ReferenceTypeWithReferenceTypesEncodeTest()
        {
            IsolationUtils.FakeAttributeClient(ReferenceTypeWithReferenceTypesEncodeTest_Run);
            IsolationUtils.FakeManualClient(ReferenceTypeWithReferenceTypesEncodeTest_Run);
        }

        private void ReferenceTypeWithReferenceTypesEncodeTest_Run(IDbContext context, ref Expr lastQuery)
        {
            var model = new ReferenceTypesReferenceModel
            {
                ReferenceModel = new ReferenceModel { Id = "test1", Indexed2 = "test2" },
                ReferenceModels1 = new List<ReferenceModel> { new ReferenceModel { Id = "test3", Indexed2 = "test4"}, new ReferenceModel { Id = "test5", Indexed2 = "test6" } },
                ReferenceModels2 = new [] { new ReferenceModel { Id = "test7", Indexed2 = "test8"}, new ReferenceModel { Id = "test9", Indexed2 = "test10" } }
            };

            var manual = Obj("reference_model", Ref("test1"), 
                "reference_models1", new object[] {Ref("test3"), Ref("test5")},
                "reference_models2", new object[] {Ref("test7"), Ref("test9")});

            Assert.Equal(JsonConvert.SerializeObject(manual), JsonConvert.SerializeObject(context.ToFaunaObj(model)));
        }

        [Fact]
        public void ReferenceTypeWithReferenceTypesDecodeTest()
        {
            var referenceModelDict1 = new Dictionary<string, object>
            {
                {"indexed2", "test2"}
            };            
            var referenceModelDict2 = new Dictionary<string, object>
            {
                {"indexed2", "test4"}
            };            
            var referenceModelDict3 = new Dictionary<string, object>
            {
                {"indexed2", "test6"}
            };            
            var referenceModelDict4 = new Dictionary<string, object>
            {
                {"indexed2", "test8"}
            };          
            var referenceModelDict5 = new Dictionary<string, object>
            {
                {"indexed2", "test10"}
            };
            var dict = new Dictionary<string, object>
            {
                {"reference_model", new {@ref = "test1", data = referenceModelDict1}},
                {
                    "reference_models1",
                    new List<object>
                    {
                        new {@ref = "test3", data = referenceModelDict2},
                        new {@ref = "test5", data = referenceModelDict3}
                    }
                },
                {
                    "reference_models2",
                    new[]
                    {
                        new {@ref = "test7", data = referenceModelDict4},
                        new {@ref = "test9", data = referenceModelDict5}
                    }
                }
            };

            var json = JsonConvert.SerializeObject(new {data = dict});
            
            IsolationUtils.FakeAttributeClient<ReferenceTypesReferenceModel>(ReferenceTypeWithReferenceTypesDecodeTest_Run, json);
            IsolationUtils.FakeManualClient<ReferenceTypesReferenceModel>(ReferenceTypeWithReferenceTypesDecodeTest_Run, json);
        }
        
        private static void ReferenceTypeWithReferenceTypesDecodeTest_Run(IDbContext context, ref Expr lastQuery)
        {
            var model = new ReferenceTypesReferenceModel
            {
                ReferenceModel = new ReferenceModel { Id = "test1", Indexed2 = "test2" },
                ReferenceModels1 = new List<ReferenceModel> { new ReferenceModel { Id = "test3", Indexed2 = "test4"}, new ReferenceModel { Id = "test5", Indexed2 = "test6" } },
                ReferenceModels2 = new [] { new ReferenceModel { Id = "test7", Indexed2 = "test8"}, new ReferenceModel { Id = "test9", Indexed2 = "test10" } }
            };
            var result = context.Get<ReferenceTypesReferenceModel>("test").Result;
            
            Assert.Equal(model, result);
        }

        [Fact]
        public void SimpleDecodeTest()
        {
            const string json = "{\"data\":{\"indexed1\":\"test1\",\"indexed2\":\"test2\"}}";

            IsolationUtils.FakeAttributeClient<ReferenceModel>(SimpleDecodeTest_Run, json);
            IsolationUtils.FakeManualClient<ReferenceModel>(SimpleDecodeTest_Run, json);
        }

        private void SimpleDecodeTest_Run(IDbContext context, ref Expr lastQuery)
        {

            var model = new ReferenceModel { Indexed1 = "test1", Indexed2 = "test2" };

            var result = context.Get<ReferenceModel>("testRef").Result;

            Assert.Equal(model, result);
        }


        [Fact]
        public void ReferenceTypeWithAllPrimitivesDecodeTest()
        {
            var dict = new Dictionary<string, object>
            {
                {"string_val", "test1"},
                {"int_val", 234},
                {"boolean_val", true},
                {"byte_val", 10},
                {"long_val", 123},
                {"float_val", 234.5f},
                {"double_val", 123.4},
                {"date_time_val", new TimeStampV(new DateTime(2010, 1, 1))},
                {"short_val", 5},
                {"u_short_val", 3},
                {"u_int_val", 2},
                {"u_long_val", 4}, // bizarre issue with conversion, ulong will implicit to double for some reason
                {"s_byte_val", 1},
                {"char_val", "a"},
            };

            var json = JsonConvert.SerializeObject(new {data = dict});

            IsolationUtils.FakeAttributeClient<PrimitivesReferenceModel>(ReferenceTypeWithAllPrimitivesDecodeTest_Run, json);
            IsolationUtils.FakeManualClient<PrimitivesReferenceModel>(ReferenceTypeWithAllPrimitivesDecodeTest_Run, json);
        }

        private void ReferenceTypeWithAllPrimitivesDecodeTest_Run(IDbContext context, ref Expr lastQuery)
        {
            var model = new PrimitivesReferenceModel
            {
                BooleanVal = true,
                ByteVal = 10,
                CharVal = 'a',
                DateTimeVal = new DateTime(2010, 1, 1).ToUniversalTime(),
                DoubleVal = 123.4,
                FloatVal = 234.5f,
                LongVal = 123,
                IntVal = 234,
                SByteVal = 1,
                UIntVal = 2,
                UShortVal = 3,
                ULongVal = 4,
                ShortVal = 5,
                StringVal = "test1"
            };

            var result = context.Get<PrimitivesReferenceModel>("test1").Result;

            Assert.Equal(model, result);
        }

        [Fact]
        public void NamedPropertyModelEncodeTest()
        {
            IsolationUtils.FakeAttributeClient(NamedPropertyModelEncodeTest_Run);
            IsolationUtils.FakeManualClient(NamedPropertyModelEncodeTest_Run);
        }
        
        private void NamedPropertyModelEncodeTest_Run(IDbContext context, ref Expr lastQuery)
        {
            var model = new NamedPropertyModel
            {
                TestString = "test123"
            };
            
            var dict = new Dictionary<string, object>
            {
                {"not_a_test_string", "test123"}
            };

            var manual = Obj(dict);
            var automatic = context.ToFaunaObj(model);
            
            Assert.Equal(JsonConvert.SerializeObject(manual), JsonConvert.SerializeObject(automatic));
        }

        [Fact]
        public void NamedPropertyModelDecodeTest()
        {
            var dict = new Dictionary<string, object>
            {
                {"not_a_test_string", "test123"}
            };
            
            var json = JsonConvert.SerializeObject(new {data = dict});
            
            IsolationUtils.FakeAttributeClient<NamedPropertyModel>(NamedPropertyModelDecodeTest_Run, json);
            IsolationUtils.FakeManualClient<NamedPropertyModel>(NamedPropertyModelDecodeTest_Run, json);
        }
        
        private static void NamedPropertyModelDecodeTest_Run(IDbContext context, ref Expr lastQuery)
        {
            var model = new NamedPropertyModel
            {
                TestString = "test123"
            };

            var result = context.Get<NamedPropertyModel>("asdf").Result;
            
            Assert.Equal(model, result);
        }

        [Fact]
        public void CustomCollectionDecodeTest()
        {
            const string json = "{\"data\": {\"custom_collection\": [\"test1\",\"test2\"]}}";
            
            IsolationUtils.FakeAttributeClient<SupportedCustomCollectionModel>(CustomCollectionDecodeTest_Run, json);
            IsolationUtils.FakeManualClient<SupportedCustomCollectionModel>(CustomCollectionDecodeTest_Run, json);
        }

        private static void CustomCollectionDecodeTest_Run(IDbContext context, ref Expr lastQuery)
        {
            var model = new SupportedCustomCollectionModel {CustomCollection = new SupportedCustomCollection<string>(new []{"test1", "test2"})};

            var result = context.Get<SupportedCustomCollectionModel>("asd").Result;
            
            Assert.Equal(model, result);
        }
        
        [Fact]
        public void CustomCollectionDecodeTest2()
        {
            var datesJson = JsonConvert.SerializeObject(new[] {new TimeStampV(DateTime.MinValue), new TimeStampV(DateTime.MaxValue)});
            var json = "{\"data\": {\"custom_collection\": " + datesJson + ", \"dates_array\": " + datesJson + "}}";
            
            IsolationUtils.FakeAttributeClient<SupportedCustomCollectionModel2>(CustomCollectionDecodeTest2_Run, json);
            IsolationUtils.FakeManualClient<SupportedCustomCollectionModel2>(CustomCollectionDecodeTest2_Run, json);
        }

        private static void CustomCollectionDecodeTest2_Run(IDbContext context, ref Expr lastQuery)
        {
            var model = new SupportedCustomCollectionModel2 {CustomCollection = new SupportedCustomCollection<DateTime>(new []{DateTime.MinValue.ToUniversalTime(), DateTime.MaxValue.ToUniversalTime()}), DatesArray = new [] {DateTime.MinValue, DateTime.MaxValue}};

            var result = context.Get<SupportedCustomCollectionModel2>("asd").Result;
            
            Assert.Equal(model, result);
        }

        [Fact]
        public void SerializationFailureTest()
        {
            IsolationUtils.FakeAttributeClient(SerializationFailureTest_Run);
            IsolationUtils.FakeManualClient(SerializationFailureTest_Run);
        }

        private static void SerializationFailureTest_Run(IDbContext context, ref Expr lastQuery)
        {
            var model1 = new UnmappedModel { TestString = "test" };
            Assert.Throws<InvalidMappingException>(() => context.ToFaunaObj(model1));
            Assert.Throws<InvalidMappingException>(() => context.Decode(JObject.Parse(JsonConvert.SerializeObject(model1)), model1.GetType()));

            var model2 = new ReferenceModel {Indexed1 = null, Indexed2 = "test"};
            var manual2 = Obj("indexed1", null, "indexed2", "test");
            Assert.Equal(JsonConvert.SerializeObject(manual2), JsonConvert.SerializeObject(context.ToFaunaObj(model2)));
            
            var model3 = new ModelWithUnmappedReference { Model = new UnmappedModel() };
            Assert.Throws<InvalidMappingException>(() => context.ToFaunaObj(model3));

            Assert.Throws<JsonSerializationException>(() => context.Decode(JObject.Parse("{\"data\": {custom_collection: [null, null, null]}}"), typeof(UnsupportedCustomCollectionModel)));
            Assert.Throws<InvalidMappingException>(() => context.Decode(JObject.Parse("{\"data\": {custom_collection: [null, null, null]}}"), typeof(UnsupportedCustomCollectionModel2)));
            Assert.Throws<InvalidMappingException>(() => context.Decode(JObject.Parse("{\"data\": {custom_collection: [null, null, null]}}"), typeof(UnsupportedCustomCollectionModel3)));
        }
    }
}