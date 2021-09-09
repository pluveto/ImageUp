using System;
using System.Collections.Generic;
using System.Text;

namespace ImageUpWpf.Core.App
{
    public class AppConfig
    {
        public List<string> ChainUploaders { get; set; }
        public string NamingTemplate { get; set; }
        public bool EnableProxy { get; set; }
        public string Proxy { get; set; }
        public static AppConfig Default()
        {
            return new AppConfig
            {
                ChainUploaders = new List<string> { "GithubUploader" },
                NamingTemplate = "{fileName}",
                EnableProxy = false
            };
        }        
    }
}
