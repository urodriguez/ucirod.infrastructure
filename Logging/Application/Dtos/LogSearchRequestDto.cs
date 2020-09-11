using System;
using Logging.Domain;
using Shared.Infrastructure.CrossCuttingV3.Authentication;

namespace Logging.Application.Dtos
{
    public class LogSearchRequestDto
    {
        public LogSearchRequestDto()
        {
            Page = 0;
            PageSize = 20;
            SortBy = "creationDate";
            SortOrder = "desc";
        }

        public Credential Credential { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
        public string SearchWord { get; set; }
        public LogType? LogType { get; set; }
        public string Environment { get; set; }
        public DateTime? SinceDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}