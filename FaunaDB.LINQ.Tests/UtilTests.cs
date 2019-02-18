using System;
using System.Linq.Expressions;
using FaunaDB.LINQ.Extensions;
using Xunit;

namespace FaunaDB.LINQ.Tests
{
    public class UtilTests
    {
        private class TestClass
        {
            public string TestField;
            public string TestMethod() => "";
        }
        
        [Fact]
        public void UtilFailureTest()
        {
            Assert.Throws<ArgumentException>(() => UtilExtensions.GetPropertyInfo((TestClass a) => a.TestMethod()));
            var testClass = new TestClass();
            Assert.Throws<ArgumentException>(() => Expression.Field(Expression.Constant(testClass), "TestField").GetPropertyInfo());
        }
    }
}