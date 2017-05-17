using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenCorporates.Models.Entities;

namespace OpenCorporates.Indexer
{
    public static class ResultFile
    {
        public static string GetFilename(string sourceFilename) => string.Concat
            (Path.GetFileNameWithoutExtension(sourceFilename), ".OpenCorporates.result.", DateTime.Now.ToString("yyyyMMddThhmmss"), ".csv");

        public static void Save(string filename, IEnumerable<OutputRow> outputRows)
        {
            using (var outputFile = new StreamWriter(filename))
            {
                outputFile.WriteLine(GetColumnHeaders());

                foreach (var outputRow in outputRows)
                {
                    outputFile.WriteLine(FormatOutPutRow(outputRow));
                }
            }
        }

        private static string GetColumnHeaders()
        {
            var line = new StringBuilder();

            line.Append("\"Original Company Name\"");
            line.Append(string.Concat(",", "\"Resolved Company Name\""));
            line.Append(string.Concat(",", "\"Parent Company Name\""));
            line.Append(string.Concat(",", "\"Industry Code(s)\""));
            line.Append(string.Concat(",", "\"Current Status\""));
            line.Append(string.Concat(",", "\"Company Type\""));
            line.Append(string.Concat(",", "\"Jurisdiction Code\""));
            line.Append(string.Concat(",", "\"OpenCorporates Url\""));

            return line.ToString();
        }

        private static string FormatOutPutRow(OutputRow outputRow)
        {
            var line = new StringBuilder();

            line.Append(string.Concat("\"", outputRow.OriginalCompanyName, "\""));
            line.Append(string.Concat(",", "\"", outputRow.ResolvedCompanyName, "\""));
            line.Append(string.Concat(",", "\"", outputRow.ParentCompanyName, "\""));
            line.Append(string.Concat(",", "\"", outputRow.IndustryCodes, "\""));
            line.Append(string.Concat(",", "\"", outputRow.CurrentStatus, "\""));
            line.Append(string.Concat(",", "\"", outputRow.CompanyType, "\""));
            line.Append(string.Concat(",", "\"", outputRow.JurisdictionCode, "\""));
            line.Append(string.Concat(",", "\"", outputRow.OpenCorporatesUrl, "\""));
            
            return line.ToString();
        }
        
        public class OutputRow
        {
            public OutputRow(string companyName, Company company)
            {
                OriginalCompanyName = companyName;
                ResolvedCompanyName = company.Name;
                ParentCompanyName = string.Empty;
                CompanyNumber = company.CompanyNumber;
                CurrentStatus = company.CurrentStatus;
                OpenCorporatesUrl = company.OpencorporatesUrl;
                CompanyType = company.CompanyType;
                JurisdictionCode = company.JurisdictionCode;
            }

            public string ResolvedCompanyName { get; set; }
            public string ParentCompanyName { get; set; }
            public string CurrentStatus { get; set; }
            public string OpenCorporatesUrl { get; set; }
            public string CompanyNumber { get; set; }
            public string OriginalCompanyName { get; }
            public string CompanyType { get; }
            public string JurisdictionCode { get; }
            public string IndustryCodes { get; set; }
        }
    }
}
