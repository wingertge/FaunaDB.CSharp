using System;

namespace FaunaDB.LINQ.Modeling
{
    /// <summary>
    /// Attribute marking a property as the key of the object
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class KeyAttribute : Attribute
    {
        
    }
}