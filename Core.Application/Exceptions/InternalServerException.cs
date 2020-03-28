using System;

namespace Core.Application.Exceptions
{
    public class InternalServerException : Exception
    {
        public InternalServerException(string message = "") : base(message)
        {
        }
    }
}