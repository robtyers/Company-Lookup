using System.Net.Http;
using NUnit.Framework;
using OpenCorporates.Clients;

namespace OpenCorporates.Tests
{
    [TestFixture]
    public class UnitTests
    {
        private const string Key = ""; //"5X0oAlvzyKxt2hJdkOiH";

        [Test]
        public void SearchCallReturnsCompanyList()
        {
            var httpClient = new HttpClient();
            var client = new CompanySearchClient(httpClient, Key)
            {
                OrderByScore = true,
                ExcludeInactive = true,
                Normalise = true
            };

            var companyResponse = client.GetAsync("Alliance Medical Limited", "gb").Result;

            Assert.NotZero(companyResponse.Results.Companies.Count);
        }

        [Test]
        public void DetailCallReturnsCompanyDetails()
        {
            var httpClient = new HttpClient();
            var client = new CompanyDetailClient(httpClient, Key);

            var companyResponse = client.GetAsync("gb", "00102498").Result;

            Assert.NotNull(companyResponse.Results.Company);
        }

        [Test]
        public void NetworkCallReturnsCompanyNetwork()
        {
            var httpClient = new HttpClient();
            var client = new CompanyNetworkClient(httpClient, Key);

            // "https://opencorporates.com/companies/gb/08274009"  00102498
            var companyResponse = client.GetAsync("gb", "08274009", 80).Result;

            Assert.NotZero(companyResponse.Results.Count);
        }
    }
}
