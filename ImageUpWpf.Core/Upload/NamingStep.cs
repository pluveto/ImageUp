using System;
using System.Collections.Generic;
using System.IO;

namespace ImageUpWpf.Core.Upload
{
    public class NamingStep
    {
        /// <summary>
        /// Like: 
        /// </summary>
        public string NamingTemplate { get; set; } = string.Empty;
        public IDictionary<string, string> GetVars(string fullFileName)
        {
            var now = DateTime.Now;
            var ts = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
            var tsInMs = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds().ToString();

            return new Dictionary<string, string>
            {
                { "fullFileName" , fullFileName},
                { "fileName" , Path.GetFileName(fullFileName)},
                { "fileNameNoExt" , Path.GetFileNameWithoutExtension(fullFileName)},
                { "ext" , Path.GetExtension(fullFileName)},
                { "year" , now.Year.ToString()},
                { "month" , now.Month.ToString()},
                { "day" , now.Day.ToString()},
                { "guid" , Guid.NewGuid().ToString()},
                { "ts" , ts },
                { "timestamp" , ts },
                { "tsms" , tsInMs},
                { "timestampMs" , ts },

            };
        }
        public string Execute(string fullFileName)
        {
            if (string.Empty == NamingTemplate)
            {
                NamingTemplate = "{fileName}";
            }

            var vars = GetVars(fullFileName);
            return BatchReplace(NamingTemplate, vars);
        }

        private static string BatchReplace(string input, IDictionary<string, string> vars)
        {
            var s = input;
            foreach (var (f, t) in vars)
            {
                var from = "{" + f + "}";
                s = s.Replace(from, t);
            }
            return s;
        }
    }
}