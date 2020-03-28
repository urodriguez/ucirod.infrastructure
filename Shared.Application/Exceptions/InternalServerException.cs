using System;

namespace Shared.Application.Exceptions
{
    public class InternalServerException : Exception
    {
        public InternalServerException(string message = "") : base(message)
        {
        }
    }
}