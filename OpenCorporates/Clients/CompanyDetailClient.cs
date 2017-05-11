using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenCorporates.Models;

namespace OpenCorporates.Clients
{
    public class CompanyDetailClient : BaseClient, ICompanyDetailClient
    {
        // {0} = Jurisdiction Code {1} = Company Number
        private const string QueryString = "/companies/{0}/{1}";

        public CompanyDetailClient(HttpClient client, string apiKey) : base(client, apiKey)
        {

        }

        public async Task<CompanyResponse> GetAsync(string jurisdiction, string companyNumber)
        {
            if (string.IsNullOrEmpty(jurisdiction))
                throw new ArgumentException("Argument cannot be empty", nameof(jurisdiction));

            if (string.IsNullOrEmpty(companyNumber))
                throw new ArgumentException("Argument cannot be empty", nameof(companyNumber));

            var queryString = GetQueryString(jurisdiction, companyNumber);
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
            
            return JsonConvert.DeserializeObject<CompanyResponse>(result, SerializerSettings);
        }

        private string GetQueryString(string jurisdiction, string companyNumber)
        {
            var queryString = string.Format(QueryString, jurisdiction, companyNumber);

            if (!string.IsNullOrEmpty(ApiKey))
                queryString = $"{queryString}&amp;{ApiParam}{ApiKey}";

            return queryString;
        }
    }
}
