using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace ImageUpWpf.Core.App
{
    public class Helper
    {
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public IuAppContext Context { get; set; }

        public HttpMessageHandler GetProxyHandler()
        {
            if (default == Context || !Context.AppConfig.EnableProxy)
            {
                return new HttpClientHandler();
            }
            var proxyUrl = Context.AppConfig.Proxy;

            if (string.IsNullOrWhiteSpace(proxyUrl))
            {
                logger.Warn("Proxy url is empty, so it won't be used.");
                return new HttpClientHandler();
            }

            logger.Info("Using proxy " + proxyUrl);
            // First create a proxy object
            var proxy = new System.Net.WebProxy
            {
                Address = new Uri(proxyUrl),
                BypassProxyOnLocal = false
            };

            // Now create a client handler which uses that proxy
            return new HttpClientHandler
            {
                Proxy = proxy,
            };

        }
    }
}
