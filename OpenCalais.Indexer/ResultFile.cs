using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenCalais.Models;

namespace OpenCalais.Indexer
{
    public static class ResultFile
    {
        public static string GetFilename(string sourceFilename) => string.Concat
            (Path.GetFileNameWithoutExtension(sourceFilename), ".OpenCalais.result.", DateTime.Now.ToString("yyyyMMddThhmmss"), ".csv");

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
            line.Append(string.Concat(",", "\"Match\""));
            line.Append(string.Concat(",", "\"Name\""));
            line.Append(string.Concat(",", "\"Common Name\""));
            line.Append(string.Concat(",", "\"Category\""));
            line.Append(string.Concat(",", "\"Open Calais PermId Url\""));

            return line.ToString();
        }

        private static string FormatOutPutRow(OutputRow outputRow)
        {
            var line = new StringBuilder();

            line.Append(string.Concat("\"", outputRow.OriginalCompanyName, "\""));
            line.Append(string.Concat(",", "\"", outputRow.Match, "\""));
            line.Append(string.Concat(",", "\"", outputRow.Name, "\""));
            line.Append(string.Concat(",", "\"", outputRow.CommonName, "\""));
            line.Append(string.Concat(",", "\"", outputRow.Category, "\""));
            line.Append(string.Concat(",", "\"", outputRow.OpenCalaisPermId, "\""));
            
            return line.ToString();
        }
        
        public class OutputRow
        {
            public OutputRow(string companyName, NamedEntity entity)
            {
                OriginalCompanyName = companyName;
                Match = entity.Match;
                Name = entity.Name;
                CommonName = entity.CommonName;
                OpenCalaisPermId = entity.OpenCalaisPermId;
                Category = entity.Category;
            }

            public string Name { get; set; }
            public string CommonName { get; set; }
            public string Match { get; set; }
            public string OpenCalaisPermId { get; set; }
            public string OriginalCompanyName { get; }
            public string Category { get; set; }
        }
    }
}
