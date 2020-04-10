using System;

namespace Logging.Domain
{
    public class Correlation
    {
        public Correlation(Guid id)
        {
            if (id == null || id == Guid.Empty) throw new ArgumentNullException("Correlation: id can not be null or empty");
            
            Id = id;
        }

        public Guid Id { get; }
    }
}