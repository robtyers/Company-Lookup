using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenCorporates.Models;

namespace OpenCorporates.Clients
{
    public class AccountStatusClient : BaseClient
    {
        // {0} = Jurisdiction Code {1} = Company Number
        private const string QueryString = "v0.4/account_status?api_token={0}";

        public AccountStatusClient(HttpClient client, string apiKey) : base(client, apiKey)
        {

        }

        public async Task<string> GetAsync()
        {
            var queryString = GetQueryString();
            var response = await Client.GetAsync(queryString);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private string GetQueryString()
        {
            var queryString = string.Format(QueryString, ApiKey);
            return queryString;
        }
    }
}
