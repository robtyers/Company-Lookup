using System.Threading.Tasks;
using OpenCorporates.Models;

namespace OpenCorporates.Clients
{
    public interface ICompanySearchClient
    {
        Task<CompanyListResponse> GetAsync(string companyName, string jurisdictionCode = "");
        bool OrderByScore { get; set; }
    }
}
