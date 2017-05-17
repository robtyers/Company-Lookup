using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using log4net;
using log4net.Config;
using OpenCorporates.Clients;
using OpenDemocracy.Repository;

namespace OpenCorporates.Indexer
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger("Log4Net");

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Welcome to the OpenCorporates Lookup Assistant!");

            XmlConfigurator.Configure();

            var filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["Filename"]);
            Log.Info($"Loading {filename}");
            var repository = new CompanyRepository(filename);
            
            try
            {
                Log.Info("Initialising OpenCorporates client");
                var apiKey = ConfigurationManager.AppSettings["ApiKey"];
                
                Log.Info("Checking OpenCorporates account status");
                CheckAccountStatus(apiKey);

                Log.Info("Initialising search service");
                var searchService = InitialiseSearchService(apiKey);

                Log.Info("Starting search");
                var results = searchService.ProcessCompanies(repository);

                Log.Info("Saving results file");
                var outputFilename = ResultFile.GetFilename(filename);
                ResultFile.Save(outputFilename, results);

                Log.Info("Opening OpenCorporates results file");
                OpenResultsFile(outputFilename);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static SearchService InitialiseSearchService(string openCorporatesApiKey)
        {
            var searchClient = new CompanySearchClient(new HttpClient(), openCorporatesApiKey);
            var detailClient = new CompanyDetailClient(new HttpClient(), openCorporatesApiKey);
            var networkClient = new CompanyNetworkClient(new HttpClient(), openCorporatesApiKey);
            
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

        private static void CheckAccountStatus(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey)) return;

            var accountStatusClient = new AccountStatusClient(new HttpClient(), apiKey);
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
