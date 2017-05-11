using System.Threading.Tasks;
using OpenCorporates.Models;

namespace OpenCorporates.Clients
{
    public interface ICompanyNetworkClient
    {
        Task<StatementResponse> GetAsync(string jurisdiction, string companyNumber, int confidence = 80);
    }
}
