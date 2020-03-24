using System;

namespace Logging.Application.Dtos
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