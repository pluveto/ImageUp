using ImageUpWpf.Core;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;

namespace ImageUpWpf.Uploader
{
    public class GithubUploader : IPlugin, IUploader
    {
        public GithubConfig Config;

        public PluginInfo PluginInfo =>
            new PluginInfo()
            {
                MainClass = "GithubUploader",
                Name = "Github Uploader",
                Type = PluginType.Uploader,
                Icon = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAZdJREFUSEu1lYExBVEMRe+vABWgAlSAClABKkAFqAAdUMGnAlSADuiACpgzk+zkZ7L735rZzOzs7puXe5P7kryZJrbZxPhqITiUdCBp2x5ierfnRdLDUJBDBADfSNpYkuWnpAtJj9W+PoIrSZcj5bs1ogW3iuA/4A56J+k8MmSCPUnPtuFJEmQnks5s7cPeW/ZG/3vbc2xrR1GuTICe67bx2gj4XZX0nSSLazFrMDZ9byTgUOcBhOiIvsWyrKeW2UKZkqqnCei+JMqwxSjhtyq4mAFgu0FrnMZY9KdPdnCOBGi8YoivkjjwMRYJOuxI8BvQJiGIEZDN2pjwrcpcgS+fADGDfMi0P93ZYjQXY8Wtq8C+Mv2x86D86M7cAw5EL1B5OZCyTHGiSUiTEiUqL1u6mp5wIoDJmKrjO1onT64i/n1UAESZOUjsagfLkvr6Qv9Uw871LKdjCLUaikuHnfs7CQ1DNj7UohSZoMpy8EZDLmRg+FXOToDmnE85VlquTIjIgmyiMUq47cqbzDe2ELT0Qe+eyQn+AIklVhnz1DvpAAAAAElFTkSuQmCC",
                Author = "Pluveto",
                Version = "0.0.1",
            };

        IPluginConfig IPlugin.Config { get => Config; set => Config = (GithubConfig)value; }

        public GithubUploader()
        {
        }

        public async Task<IList<string>> Upload(IList<string> files)
        {
            var client = new GitHubClient(new ProductHeaderValue(this.GetType().FullName));
            var list = new List<string>();
            client.Credentials = new Credentials(this.Config.AccessToken);

            foreach (var file in files)
            {
                string mime = Utils.GetMIMEType(file);
                using (var archiveContents = File.OpenRead(file))
                {
                    var baseName = Path.GetFileName(file);
                    var base64Str = GetImageBase64String(file);
                    var req = new CreateFileRequest("feat: " + baseName, base64Str, Config.Branch, true);
                    var changeSet = await client.Repository.Content.CreateFile(Config.Owner, Config.Name, Config.PathPrefix + baseName, req);
                }
            }

            return list;

        }


        static string GetImageBase64String(string imgPath)
        {
            byte[] imageBytes = System.IO.File.ReadAllBytes(imgPath);
            return Convert.ToBase64String(imageBytes);
        }

    }
    public class GithubConfig : IPluginConfig
    {
        public string AccessToken { get; set; }
        public string Repository { get; set; }
        public string Owner { get { return this.Repository.Split("/")[0]; } }
        public string Name { get { return this.Repository.Split("/")[1]; } }

        public string Branch { get; internal set; }
        // Need to be end with "/"
        public string PathPrefix { get; internal set; }

        public IDictionary<string, ConfigItemMeta> ConfigFormMeta => new Dictionary<string, ConfigItemMeta>() {
            {"AccessToken", new ConfigItemMeta{ DisplayName="Access Token" ,Type = ConfigItemType.String } },
            {"Repository",  new ConfigItemMeta{ Placeholder="username/repo", Type = ConfigItemType.String } },
            {"Branch",      new ConfigItemMeta{ Placeholder="Like main, master...", DefaultValue ="master", Type = ConfigItemType.String } },
            {"PathPrefix",  new ConfigItemMeta{ Placeholder="Must ends with \"/\"", DisplayName= "Path Prefix", Type = ConfigItemType.String } },
        };

    }

}
