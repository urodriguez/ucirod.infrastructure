using System;

namespace Shared.ApplicationV3.Exceptions
{
    public class InternalServerException : Exception
    {
        public InternalServerException(string message = "") : base(message)
        {
        }
    }
}