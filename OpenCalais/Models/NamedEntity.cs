namespace OpenCalais.Models
{
    public class NamedEntity
    {
        public string Category { get; set; }
        public string Match { get; set; }
        public float ConfidenceLevel { get; set; }
        public string Name { get; set; }
        public string CommonName { get; set; }
        public string OpenCalaisPermId { get; set; }
    }
}
