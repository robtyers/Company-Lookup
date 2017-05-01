using System;

namespace OpenCorporates.Models.Entities
{
    public class Source
    {
        public string Publisher { get; set; }
        public DateTime? RetrievedAt { get; set; }
        public string Terms { get; set; }
        public string Url { get; set; }
    }
}
