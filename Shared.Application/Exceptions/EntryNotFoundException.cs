using System;

namespace Shared.Application.Exceptions
{
    public class EntryNotFoundException : Exception
    {
        public EntryNotFoundException(string message = "") : base(message)
        {
        }
    }
}