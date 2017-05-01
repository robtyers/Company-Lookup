using OpenCorporates.Models.Entities;

namespace OpenCorporates.Models
{
    public class StatementResult
    {
        public string ParentName { get; set; }
        public string ParentOpencorporatesUrl { get; set; }
        public string ParentType { get; set; }
        public string ChildName { get; set; }
        public string ChildOpenCorporatesUrl { get; set; }
        public string ChildType { get; set; }
        public string RelationshipType { get; set; }
        public RelationshipProperties RelationshipProperties { get; set; }
    }
}
