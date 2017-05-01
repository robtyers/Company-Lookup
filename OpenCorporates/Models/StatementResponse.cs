using System.Collections.Generic;

namespace OpenCorporates.Models
{
    public class StatementResponse : BaseResponse
    {
        public List<StatementResult> Results { get; set; }
    }
}
