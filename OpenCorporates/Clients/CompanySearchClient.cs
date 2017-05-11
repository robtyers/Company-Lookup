using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenCorporates.Models;

namespace OpenCorporates.Clients
{
    public class CompanySearchClient : BaseClient, ICompanySearchClient
    {
        private const string QueryString = "/companies/search?fields=normalised_name&amp;format=json&amp;q=";
        private const string JurisdictionCodeParam = "jurisdiction_code=";
        private const string OrderResultsParam = "order=score";
        private const string InactiveParam = "inactive=false";
        private const string NormaliseParam = "normalise_company_name=true";
        
        public bool OrderByScore { get; set; }
        public bool ExcludeInactive { get; set; }
        public bool Normalise { get; set; }
        
        public CompanySearchClient(HttpClient client, string apiKey) : base(client, apiKey)
        {
            OrderByScore = true;
            ExcludeInactive = false;
            Normalise = true;
        }

        public async Task<CompanyListResponse> GetAsync(string companyName, string jurisdictionCode = "")
        {
            if (string.IsNullOrEmpty(companyName))
                throw new ArgumentException("Argument cannot be empty", nameof(companyName));

            var queryString = GetQueryString(companyName, jurisdictionCode);
            var result = string.Empty;
            var cnt = 0;

            do
            {
                try
                {
                    var response = await Client.GetAsync(queryString);
                    response.EnsureSuccessStatusCode();
                    result = await response.Content.ReadAsStringAsync();
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Thread.Sleep(3000);
                    cnt++;
                }
            } while (cnt < 3);
            
            return JsonConvert.DeserializeObject<CompanyListResponse>(result, SerializerSettings);
        }

        private string GetQueryString(string companyName, string jurisdictionCode)
        {
            string queryString = $"{QueryString}{companyName.Replace(" ", "+")}";

            if (ExcludeInactive)
                queryString = $"{queryString}&amp;{InactiveParam}";

            if (OrderByScore)
                queryString = $"{queryString}&amp;{OrderResultsParam}";
            
            if (Normalise)
                queryString = $"{queryString}&amp;{NormaliseParam}";
            
            if (!string.IsNullOrEmpty(jurisdictionCode))
                queryString = $"{queryString}&amp;{JurisdictionCodeParam}{jurisdictionCode}";
            
            if (!string.IsNullOrEmpty(ApiKey))
                queryString = $"{queryString}&amp;{ApiParam}{ApiKey}";

            return queryString;
        }
    }
}
