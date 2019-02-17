using System;

namespace FaunaDB.Driver.Errors
{
    public class UnsupportedMethodException : Exception
    {
        public UnsupportedMethodException(string methodName, string message = null) : base($"Unsupported method {methodName}: {message}") { }
    }
}