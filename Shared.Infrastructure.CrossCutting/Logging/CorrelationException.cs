using System;

namespace Shared.Infrastructure.CrossCutting.Logging
{
    public class CorrelationException : Exception
    {
        public CorrelationException(Exception inner) : base("An error has ocurred trying to generate a Correlation", inner)
        {
        }
    }
}