using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using log4net;
using log4net.Config;
using OpenCorporates.Clients;

namespace CompanyIndexer
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger("Log4Net");

        static void Main(string[] args)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Welcome to the Company Lookup Assistant!");

                XmlConfigurator.Configure();

                var filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["Filename"]);
                Log.Info($"Loading {filename}");
                var repository = new CompanyRepository(filename);

                Log.Info("Initialising OpenCorporates client");
                var httpClient = new HttpClient();
                var apiKey = ConfigurationManager.AppSettings["ApiKey"];

                Log.Info("Checking account status");
                CheckAccountStatus(apiKey, httpClient);

                Log.Info("Initialising search service");
                var searchService = InitialiseSearchService(httpClient, apiKey);

                Log.Info("Starting search");
                var results = searchService.ProcessCompanies(repository);

                Log.Info("Saving results file");
                var outputFilename = ResultFile.GetFilename(filename);
                ResultFile.Save(outputFilename, results);

                Log.Info("Opening results file");
                OpenResultsFile(outputFilename);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static SearchService InitialiseSearchService(HttpClient httpClient, string apiKey)
        {
            var searchClient = new CompanySearchClient(httpClient, apiKey);
            var detailClient = new CompanyDetailClient(httpClient, apiKey);
            var networkClient = new CompanyNetworkClient(httpClient, apiKey);

            return new SearchService(searchClient, networkClient, detailClient, Log)
            {
                Delay = int.Parse(ConfigurationManager.AppSettings["DelayInMs"]),
                NetworkConfidence = int.Parse(ConfigurationManager.AppSettings["NetworkConfidence"]),
                SearchDepth = int.Parse(ConfigurationManager.AppSettings["SearchDepth"]),
                MaxErrorCount = int.Parse(ConfigurationManager.AppSettings["MaxErrorCount"]),
                JurisdictionCode = ConfigurationManager.AppSettings["JurisdictionCode"],
                ShowInactive = bool.Parse(ConfigurationManager.AppSettings["ShowInactive"])
            };
        }

        private static void CheckAccountStatus(string apiKey, HttpClient httpClient)
        {
            if (string.IsNullOrEmpty(apiKey)) return;

            var accountStatusClient = new AccountStatusClient(httpClient, apiKey);
            Log.Info(accountStatusClient.GetAsync().Result);

            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
        }

        private static void OpenResultsFile(string outputFilename)
        {
            var proc = new Process
            {
                StartInfo =
                {
                    FileName = outputFilename,
                    UseShellExecute = true
                }
            };
            proc.Start();
        }
    }
}
