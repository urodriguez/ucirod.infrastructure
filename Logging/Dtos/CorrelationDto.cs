using System;

namespace Logging.Dtos
{
    public class CorrelationDto
    {
        public CorrelationDto()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
    }
}