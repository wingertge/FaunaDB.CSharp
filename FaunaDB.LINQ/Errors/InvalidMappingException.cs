using System;

namespace FaunaDB.LINQ.Errors
{
    /// <inheritdoc />
    /// <summary>
    /// Exception thrown when database object mapping is invalid or missing
    /// </summary>
    public class InvalidMappingException : Exception
    {
        internal InvalidMappingException(string message) : base(message)
        {
        }
    }
}