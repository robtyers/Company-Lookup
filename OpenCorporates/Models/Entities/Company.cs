using System;
using System.Collections.Generic;

namespace OpenCorporates.Models.Entities
{
    public class Company
    {
        public string BranchStatus { get; set; }
        public string CompanyNumber { get; set; }
        public string CompanyType { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CurrentStatus { get; set; }
        public DateTime? DissolutionDate { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? IncorporationDate { get; set; }
        public string JurisdictionCode { get; set; }
        public string Name { get; set; }
        public string OpencorporatesUrl { get; set; }
        public List<PreviousName> PreviousNames { get; set; }
        public string RegistryUrl { get; set; }
        public DateTime? RetrievedAt { get; set; }
        public Source Source { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string RegisteredAddressInFull { get; set; }
        public RegisteredAddress RegisteredAddress { get; set; }
        public string NativeCompanyNumber { get; set; }
        public string RestrictedForMarketing { get; set; }
        public List<CorporateGroupingListItem> CorporateGroupings { get; set; }
        public List<OfficerListItem> Officers { get; set; }
        public List<IndustryCodeListItem> IndustryCodes { get; set; }
    }
}
