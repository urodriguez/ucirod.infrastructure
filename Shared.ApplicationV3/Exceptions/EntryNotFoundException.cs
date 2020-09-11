using System;

namespace Shared.ApplicationV3.Exceptions
{
    public class EntryNotFoundException : Exception
    {
        public EntryNotFoundException(string message = "") : base(message)
        {
        }
    }
}