using System.Collections.Generic;
using OpenCorporates.Models.Entities;

namespace OpenCorporates.Models
{
    public class CompanyListResult : BaseResult
    {
        public List<CompanyListItem> Companies { get; set; }
    }
}
