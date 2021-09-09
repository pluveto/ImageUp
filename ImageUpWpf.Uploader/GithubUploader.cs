using ImageUpWpf.Core;
using ImageUpWpf.Core.App;
using ImageUpWpf.Core.Plugin.Config;
using ImageUpWpf.Core.Plugin.Interface;
using ImageUpWpf.Core.Upload;
using ImageUpWpf.Uploader.Properties;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageUpWpf.Uploader
{
    public class GithubUploader : IPlugin, IUploader
    {
        private GithubConfig config = new GithubConfig();
        private Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly static SemaphoreSlim mutex = new SemaphoreSlim(1);

        public IuAppContext Context { get; set; }

        public PluginInfo PluginInfo { get; set; } =
            new PluginInfo()
            {
                MainClass = "GithubUploader",
                Name = "Github Uploader",
                Type = PluginType.Uploader,
                Icon = Resources.GithubIcon,
                Author = "Pluveto",
                Version = "0.0.2",
            };

        IPluginConfig IPlugin.Config { get => config; set => config = (GithubConfig)value; }

        public GithubUploader()
        {
            logger.Info("Plugin instanciated.");
        }


        static string StreamToBase64(Stream s)
        {
            using (var memoryStream = new MemoryStream())
            {
                s.Position = 0;
                s.CopyTo(memoryStream);
                var bytes = memoryStream.ToArray();
                return Convert.ToBase64String(bytes);
            }
        }

        public async Task<string> Upload(Stream sr, string name)
        {
            logger.Info("Prepare to upload " + name);

            HttpResponseMessage resp;
            using (var client = new HttpClient(handler: Context.Helper.GetProxyHandler()))
            using (sr)
            {
                client.Timeout = TimeSpan.FromMilliseconds(this.config.Timeout);
                HttpRequestMessage request = buildRequest(sr, name);

                try
                {
                    logger.Info("Waiting to upload " + name);
                    mutex.Wait();
                    // 由于 Github 批量提交会遇到冲突问题，所以加上互斥锁，使得上传顺序进行
                    logger.Info("Start uploading " + name);
                    resp = await client.SendAsync(request);
                    logger.Info("Finish uploading " + name);

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
                    mutex.Release();
                    logger.Info("Released mutex of " + name);
                }
                logger.Info($"Status Code: {resp.StatusCode}({(int)resp.StatusCode})");
                logger.Info("Response Body: " + await resp.Content.ReadAsStringAsync());
            }
            if (resp.StatusCode == System.Net.HttpStatusCode.Created
                || resp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var url = buildFinalUrl(name);
                //var resStr = await resp.Content.ReadAsStringAsync();
                //var resObj = JsonConvert.DeserializeObject<Model.CreateResp>(resStr);
                //var url = resObj.Content.DownloadUrl.AbsoluteUri;
                return url;
            }
            if (resp.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
            {
                // 可能是重复
                throw new UploadException("Unprocessable Entity, Maybe file duplicated.");
            }
            if (resp.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                throw new UploadException("Conflict, Maybe operating too quick.");
            }

            return null;
        }

        private HttpRequestMessage buildRequest(Stream sr, string name)
        {
            var api = $"https://api.github.com/repos/{this.config.Repository}/contents{GetPrefix()}{Uri.EscapeUriString(name)}";
            logger.Info("Request PUT on " + api);
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(api);
            request.Method = HttpMethod.Put;
            var json = buildRequestBody(name, sr);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            request.Headers.TryAddWithoutValidation("User-Agent", Resources.UserAgent);
            request.Headers.TryAddWithoutValidation("Accept", "application/vnd.github.v3+json");
            request.Headers.TryAddWithoutValidation("Authorization", $"token {this.config.AccessToken}");
            return request;
        }


        private string buildRequestBody(string name, Stream sr)
        {
            var msg = "feat: upload " + name + " via ImageUp";
            var base64Str = StreamToBase64(sr);
            var content = base64Str;
            var branch = this.config.Branch;
            var str = JsonConvert.SerializeObject(new
            {
                message = msg,
                content = content,
                branch = branch,
            });
            return str;
        }

        //private string StreamToString(Stream stream)
        //{
        //    var res = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
        //    return res;
        //}


        private string buildFinalUrl(string name)
        {
            var prefix = GetPrefix();
            var path = prefix + Uri.EscapeUriString(name);

            var cdn = this.config.CDN;
            string url;
            if (cdn == "" || cdn == "/" || !this.config.UseCDN)
            {
                url = "https://raw.githubusercontent.com/" + this.config.Repository + "/" + this.config.Branch + path;
                return url;
            }

            if (!cdn.EndsWith("/"))
            {
                cdn += "/";
            }
            url = cdn + path;
            return url;
        }

        private string GetPrefix()
        {
            var p = this.config.PathPrefix;
            if (!p.EndsWith("/"))
            {
                p += "/";
            }
            if (!p.StartsWith("/"))
            {
                p = "/" + p;
            }
            return p;
        }
    }
    public class GithubConfig : IPluginConfig
    {
        public string AccessToken { get; set; }
        public string Repository { get; set; }
        public string Owner { get { return this.Repository.Split("/")[0]; } }
        public string Name { get { return this.Repository.Split("/")[1]; } }

        public string Branch { get; set; }
        // Need to be end with "/"
        public string PathPrefix { get; set; }
        public bool UseCDN { get; set; }
        public string CDN { get; set; } = "";
        public double Timeout { get; set; } = 10000;

        public IDictionary<string, ConfigItemMeta> ConfigFormMeta => new Dictionary<string, ConfigItemMeta>() {
            {"AccessToken", new ConfigItemMeta{ Type = ConfigItemType.String,      DisplayName="Access Token" , } },
            {"Repository",  new ConfigItemMeta{ Type = ConfigItemType.String,      Placeholder="username/repo",  } },
            {"Branch",      new ConfigItemMeta{ Type = ConfigItemType.String,      Placeholder="Like main, master...", DefaultValue ="master", } },
            {"PathPrefix",  new ConfigItemMeta{ Type = ConfigItemType.String,      Placeholder="Must ends with \"/\"", DisplayName= "Path Prefix",  } },
            {"CDN",         new ConfigItemMeta{ Type = ConfigItemType.String,      DisplayName="CDN" , Description = "If set and CDN enabled, generated url = CDN/prefix/filename.jpg" } },
            {"UseCDN",      new ConfigItemMeta{ Type = ConfigItemType.BoolSwitch,  DisplayName="Use CDN" , DefaultValue = "false" } },
            {"Timeout",     new ConfigItemMeta{ Type = ConfigItemType.Integer,     DisplayName="Timeout in ms" , DefaultValue = "10000" } },
        };

    }

}
