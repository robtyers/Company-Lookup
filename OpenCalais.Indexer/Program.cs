using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using log4net;
using log4net.Config;
using OpenCalais.Clients;
using OpenDemocracy.Repository;

namespace OpenCalais.Indexer
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger("Log4Net");

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Welcome to the OpenCalais Lookup Assistant!");

            XmlConfigurator.Configure();

            var filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["Filename"]);
            Log.Info($"Loading {filename}");
            var repository = new CompanyRepository(filename);

            try
            {
                Log.Info("Initialising OpenCalais client");
                var apiKey = ConfigurationManager.AppSettings["ApiKey"];
                if(string.IsNullOrEmpty(apiKey))
                    throw new ArgumentException("You must specify an API key");

                Log.Info("Initialising search service");
                var searchService = InitialiseSearchService(apiKey);
                
                Log.Info("Starting search");
                var results = searchService.ProcessCompanies(repository);

                Log.Info("Saving results file");
                var outputFilename = ResultFile.GetFilename(filename);
                ResultFile.Save(outputFilename, results);

                Log.Info("Opening OpenCalais results file");
                OpenResultsFile(outputFilename);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static SearchService InitialiseSearchService(string apiKey)
        {
            var httpClient = new HttpClient();
            var searchClient = new EntitySearchClient(httpClient, apiKey);
            
            return new SearchService(searchClient, Log)
            {
                Delay = int.Parse(ConfigurationManager.AppSettings["DelayInMs"])
            };
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
