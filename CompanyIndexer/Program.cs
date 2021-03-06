﻿using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using log4net;
using log4net.Config;
using OpenCalais.Clients;
using OpenCorporates.Clients;

namespace CompanyIndexer
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger("Log4Net");

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Welcome to the Company Lookup Assistant!");

            XmlConfigurator.Configure();

            var filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["Filename"]);
            Log.Info($"Loading {filename}");
            var repository = new CompanyRepository(filename);




            try
            {
                Log.Info("Initialising OpenCorporates client");
                var openCorporatesApiKey = ConfigurationManager.AppSettings["OpenCorporatesApiKey"];
                var openCalaisApiKey = ConfigurationManager.AppSettings["OpenCalaisApiKey"];

                Log.Info("Checking OpenCorporates account status");
                CheckOpenCorporatesAccountStatus(openCorporatesApiKey, new HttpClient());

                Log.Info("Initialising search service");
                var searchService = InitialiseSearchService(openCorporatesApiKey, openCalaisApiKey);

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

        private static SearchService InitialiseSearchService(string openCorporatesApiKey, string openCalaisApiKey)
        {
            var openCorporatesHttpClient = new HttpClient();
            var searchClient = new CompanySearchClient(openCorporatesHttpClient, openCorporatesApiKey);
            var detailClient = new CompanyDetailClient(openCorporatesHttpClient, openCorporatesApiKey);
            var networkClient = new CompanyNetworkClient(openCorporatesHttpClient, openCorporatesApiKey);

            var entitySearchClient = default(EntitySearchClient);
            if (bool.Parse(ConfigurationManager.AppSettings["OpenCalaisEnabled"]))
            {
                var openCalaisHttpClient = new HttpClient();
                entitySearchClient = new EntitySearchClient(openCalaisHttpClient, openCalaisApiKey);
            }
                
            return new SearchService(searchClient, networkClient, detailClient, entitySearchClient,  Log)
            {
                Delay = int.Parse(ConfigurationManager.AppSettings["DelayInMs"]),
                NetworkConfidence = int.Parse(ConfigurationManager.AppSettings["NetworkConfidence"]),
                SearchDepth = int.Parse(ConfigurationManager.AppSettings["SearchDepth"]),
                MaxErrorCount = int.Parse(ConfigurationManager.AppSettings["MaxErrorCount"]),
                JurisdictionCode = ConfigurationManager.AppSettings["JurisdictionCode"],
                ShowInactive = bool.Parse(ConfigurationManager.AppSettings["ShowInactive"])
            };
        }

        private static void CheckOpenCorporatesAccountStatus(string apiKey, HttpClient httpClient)
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
