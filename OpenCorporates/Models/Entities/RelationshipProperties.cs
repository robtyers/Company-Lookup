using System;

namespace OpenCorporates.Models.Entities
{
    public class RelationshipProperties
    {
        public DateTime LatestDate { get; set; }
        public int Confidence { get; set; }
        public string LatestDateType { get; set; }
        public string EarliestDateType { get; set; }
        public bool IsDeletion { get; set; }
        public DateTime EarliestDate { get; set; }
    }
}
