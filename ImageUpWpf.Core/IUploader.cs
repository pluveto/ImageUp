using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ImageUpWpf.Core
{
    public interface IUploader
    {
        Task<string> Upload(Stream sr, string name);

    }

    public interface IPlugin
    {
        PluginInfo PluginInfo { get; }
        IPluginConfig Config{ get; set; }
    }

    public class PluginInfo
    {
        public string Icon { get; set; }
        public string MainClass { get; set; }
        public string Summary { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string Repository { get; set; }
        public PluginType Type { get; set; }
        public string Name { get; set; }
    }

    public class PluginType
    {
        private string type = "";
        public static PluginType Uploader = new PluginType("Uploader");

        public PluginType(string type)
        {
            this.type = type;
        }
        public new string ToString() {
            return this.type;
        }

    }
}
