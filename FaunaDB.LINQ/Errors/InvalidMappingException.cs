using System;

namespace FaunaDB.LINQ.Errors
{
    public class InvalidMappingException : Exception
    {
        public InvalidMappingException(string message) : base(message)
        {
        }
    }
}