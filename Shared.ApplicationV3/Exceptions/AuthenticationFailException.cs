using System;

namespace Shared.ApplicationV3.Exceptions
{
    public class AuthenticationFailException : Exception
    {
        public AuthenticationFailException(string message = "") : base(message)
        {
        }
    }
}