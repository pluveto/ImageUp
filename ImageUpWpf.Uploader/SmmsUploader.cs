using ImageUpWpf.Core;
using ImageUpWpf.Core.App;
using ImageUpWpf.Core.Plugin.Config;
using ImageUpWpf.Core.Plugin.Interface;
using ImageUpWpf.Uploader.Properties;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ImageUpWpf.Uploader
{
    public class SmmsUploader : IPlugin, IUploader

    {
        private SmmsConfig config = new SmmsConfig();
        private Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public IuAppContext Context { get; set; }

        public PluginInfo PluginInfo { get; set; } = new PluginInfo()
        {
            Name = "SM.MS Uploader",
            Type = PluginType.Uploader,
            Icon = Resources.SmmsLogo,
            Author = "Pluveto",
            Version = "0.0.1",
        };

        IPluginConfig IPlugin.Config { get => config; set => config = (SmmsConfig)value; }

        public async Task<string> Upload(Stream sr, string name)
        {
            var fileName = Path.GetFileName(name);
            checkConfig();
            HttpResponseMessage resp;
            using (var client = new HttpClient(handler: Context.Helper.GetProxyHandler()))
            using (sr)
            {
                client.Timeout = TimeSpan.FromMilliseconds(this.config.Timeout);
                HttpRequestMessage request = buildRequest(sr, fileName);
                try
                {
                    logger.Info("Start uploading " + fileName);
                    resp = await client.SendAsync(request);
                    logger.Info("Finish uploading " + fileName);
                }
                catch (HttpRequestException e)
                {
                    logger.Error(e.Message);
                    throw e;
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                }
                var code = (int)resp.StatusCode;
                logger.Info($"Status Code: {resp.StatusCode}({code})");
                var respStr = await resp.Content.ReadAsStringAsync();
                logger.Info("Response Body: " + respStr);
                if (300 > code && code >= 200)
                {
                    var ret = JsonConvert.DeserializeObject<SmmsRespModel.SmmsResp>(respStr);
                    if (ret.Success)
                    {
                        return ret.Data.Url.AbsoluteUri;
                    }
                    else
                    {
                        throw new Core.Upload.UploadException(ret.Message);
                    }
                }
                else
                {
                    throw new Core.Upload.UploadException(respStr);
                }
            }
            return null;
        }

        private void checkConfig()
        {
            if (string.IsNullOrWhiteSpace(this.config.Token))
            {
                throw new Exception("Invalid token, empty.");
            }
        }

        private HttpRequestMessage buildRequest(Stream sr, string fileName)
        {
            var api = this.config.BaseUrl + "/upload";
            logger.Info("Request POST on " + api);
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(api);
            request.Method = HttpMethod.Post;
            request.Content = buildRequestBody(sr, fileName);
            request.Headers.TryAddWithoutValidation("User-Agent", Resources.UserAgent);
            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            request.Headers.TryAddWithoutValidation("Authorization", $"Basic {this.config.Token}");
            return request;
        }

        public static byte[] streamToByteArray(Stream input)
        {
            MemoryStream ms = new MemoryStream();
            input.CopyTo(ms);
            return ms.ToArray();
        }
        private HttpContent buildRequestBody(Stream sr, string fileName)
        {

            var multipartFormDataContent = new MultipartFormDataContent();
            HttpContent fileContent = new StreamContent(sr);
            multipartFormDataContent.Add(fileContent, "smfile", fileName);
            return multipartFormDataContent;
        }
    }

    public class SmmsConfig : IPluginConfig
    {
        internal string BaseUrl { get; set; } = "https://sm.ms/api/v2";
        public string Token { get; set; }
        public double Timeout { get; set; } = 60*1000;

        public IDictionary<string, ConfigItemMeta> ConfigFormMeta => new Dictionary<string, ConfigItemMeta>() {
            {"Token",       new ConfigItemMeta{ Type = ConfigItemType.String,      DisplayName="Token" , Description = "Get it from https://sm.ms/home/"} },
            {"Timeout",     new ConfigItemMeta{ Type = ConfigItemType.Integer,     DisplayName="Timeout in ms" , DefaultValue = "10000" } },
        };

    }
}
