using System;

namespace FaunaDB.LINQ.Modeling
{
    /// <summary>
    /// Attribute to use for UDF stubs.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Method)]
    public class DbFunctionAttribute : Attribute
    {
        /// <summary>
        /// Function name on database. Defaults to function name.
        /// </summary>
        /// <value>The database function name.</value>
        public string Name { get; set; }

        public DbFunctionAttribute() { }
        public DbFunctionAttribute(string name)
        {
            Name = name;
        }
    }
}