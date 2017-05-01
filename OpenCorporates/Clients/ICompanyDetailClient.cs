using System.Threading.Tasks;
using OpenCorporates.Models;

namespace OpenCorporates.Clients
{
    public interface ICompanyDetailClient
    {
        Task<CompanyResponse> GetAsync(string jurisdiction, string companyNumber);
    }
}