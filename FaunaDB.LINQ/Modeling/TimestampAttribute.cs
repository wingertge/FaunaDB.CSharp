using System;

namespace FaunaDB.LINQ.Modeling
{
    /// <summary>
    /// Marks a property as the object's time stamp
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class TimestampAttribute : Attribute
    {
        
    }
}