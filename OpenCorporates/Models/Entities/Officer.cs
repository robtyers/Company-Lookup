using System;

namespace OpenCorporates.Models.Entities
{
    public class Officer
    {
        public DateTime? EndDate { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string OpencorporatesUrl { get; set; }
        public string Position { get; set; }
        public DateTime StartDate { get; set; }
        public string Uid { get; set; }
    }
}
