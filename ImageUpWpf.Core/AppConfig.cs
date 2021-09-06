using System;
using System.Collections.Generic;
using System.Text;

namespace ImageUpWpf.Core
{
    public class AppConfig
    {
        public List<string> ChainUploaders { get; set; }
        public string NamingTemplate { get; set; }

        public static AppConfig Default()
        {
            return new AppConfig { 
                ChainUploaders = new List<string> { "GithubUploader" },
                NamingTemplate = "{fileName}",
            };
        }
    }
}
