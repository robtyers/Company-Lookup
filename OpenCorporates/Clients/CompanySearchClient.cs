using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenCorporates.Models;

namespace OpenCorporates.Clients
{
    public interface ICompanySearchClient
    {
        Task<CompanyListResponse> GetAsync(string companyName);
    }

    public class CompanySearchClient : BaseClient, ICompanySearchClient
    {
        private const string QueryString = "v0.4/companies/search?format=json&amp;q=";

        public CompanySearchClient(HttpClient client, string apiKey) : base(client, apiKey)
        {
            
        }

        public async Task<CompanyListResponse> GetAsync(string companyName)
        {
            if (string.IsNullOrEmpty(companyName))
                throw new ArgumentException("Argument cannot be empty", nameof(companyName));

            var queryString = GetQueryString(companyName);
            var response = await Client.GetAsync(queryString);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CompanyListResponse>(result, SerializerSettings);
        }

        private string GetQueryString(string companyName)
        {
            string queryString = $"{QueryString}{companyName.Replace(" ", "+")}";

            if (!string.IsNullOrEmpty(ApiKey))
                queryString = $"{queryString}&amp;{ApiParam}{ApiKey}";

            return queryString;
        }
    }
}
