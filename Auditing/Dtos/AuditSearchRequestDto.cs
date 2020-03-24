using System;
using Infrastructure.CrossCutting.Authentication;

namespace Auditing.Dtos
{
    public class AuditSearchRequestDto
    {
        public AuditSearchRequestDto()
        {
            Page = 0;
            PageSize = 10;
            SortBy = "creationDate";
            SortOrder = "desc";
        }

        public Account Account { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
        public DateTime? SinceDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}