using System;

namespace FaunaDB.LINQ.Modeling
{
    /// <summary>
    /// Attribute marking a property as a reference object
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ReferenceAttribute : Attribute
    {
        
    }
}