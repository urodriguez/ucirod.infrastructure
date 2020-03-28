using System;

namespace Core.Application.Exceptions
{
    public class EntryNotFoundException : Exception
    {
        public EntryNotFoundException(string message = "") : base(message)
        {
        }
    }
}