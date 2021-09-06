using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ImageUpWpf.Core
{
    public class PluginManager
    {
        public PluginManager()
        {
            this.PluginDir = Path.Join(Environment.CurrentDirectory, "plugins");
        }
        public string PluginDir { get; set; }

        public List<IPlugin> Plugins { get; private set; }
        public void LoadPlugins()
        {
            this.Plugins = new List<IPlugin>();
            var pluginAsm = SearchPluginDlls();
            foreach (var a in pluginAsm)
            {
                var pluginClassType = GetFirstImplOf(a, typeof(IPlugin));
                var plugin = (IPlugin)Activator.CreateInstance(pluginClassType);

                var pluginConfigType = GetFirstImplOf(a, typeof(IPluginConfig));
                plugin.Config = LoadConfig(plugin.PluginInfo.MainClass, pluginConfigType);

                Plugins.Add(plugin);
            }
        }

        public List<PluginInfo> PluginInfos => Plugins.Select(x => x.PluginInfo).ToList();
        public IUploader GetUploader(string pluginName)
        {
            var pluginAsm = SearchPluginDlls();

            IUploader uploader = null;
            try
            {
                foreach (var a in pluginAsm)
                {
                    //if (!dll.Contains(pluginName))
                    //{
                    //    continue;
                    //}
                    Type pluginClassType = GetFirstImplOfWithNameContains(a, typeof(IUploader), pluginName);
                    if (null == pluginClassType)
                    {
                        continue;
                    }
                    Type pluginConfigType = GetFirstImplOfWithNameContains(a, typeof(IPluginConfig), pluginName);

                    // If no config dependency
                    if (default == pluginConfigType)
                    {
                        uploader = (IUploader)Activator.CreateInstance(pluginClassType);
                    }
                    else
                    {
                        // IPluginConfig config = LoadConfig(pluginName, pluginConfigType);
                        // uploader = (IUploader)Activator.CreateInstance(pluginClassType, config);
                    }
                    return uploader;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
            return uploader;


        }

        private IPluginConfig LoadConfig(string mainClass, Type pluginConfigType)
        {
            if(default == pluginConfigType)
            {
                return null;
            }
            // or create config for it
            var x = typeof(PluginManager).GetMethod("GetPluginConfig", BindingFlags.Public | BindingFlags.Static);
            // GetPluginConfig<pluginConfigType>(pluginDir, pluginName);
            IPluginConfig config = (IPluginConfig)x.MakeGenericMethod(pluginConfigType).Invoke(null, new object[] { PluginDir, mainClass });
            if (config == default)
            {
                config = (IPluginConfig)Activator.CreateInstance(pluginConfigType);
            }

            return config;
        }

        private IList<Assembly> SearchPluginDlls()
        {
            Utils.CreateDirIfNotExists(PluginDir);
            var dlls = Directory.GetFiles(PluginDir, "*.dll");
            var pluginDlls = new List<Assembly>();
            foreach (var dll in dlls)
            {
                try
                {
                    var a = Assembly.LoadFile(dll);
                    var impl = GetFirstImplOf(a, typeof(IPlugin));
                    if (impl == default)
                    {
                        continue;
                    }
                    pluginDlls.Add(a);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    continue;
                }
            }
            return pluginDlls;
        }
#pragma warning disable IDE0051 // 此函数已经通过反射调用，调用者 LoadConfig
        public static T GetPluginConfig<T>(string dir, string pluginName)
#pragma warning restore IDE0051
        {
            var (cfgPath, ext) = TryGetFileWithExtIn(dir, pluginName, new string[] { "yaml", "yml", "json" });
            if (cfgPath == default)
            {
                return default(T);
            }
            var sr = File.OpenText(cfgPath);
            switch (ext)
            {
                case "yaml":
                case "yml":
                    var deserializer = new DeserializerBuilder()
                                       .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                       .Build();
                    return deserializer.Deserialize<T>(sr);
                case "json":
                    return JsonConvert.DeserializeObject<T>(sr.ReadToEnd());
                default:
                    break;
            }
            return default(T);

        }

        private static (string path, string ext) TryGetFileWithExtIn(string dir, string pluginName, string[] exts)
        {
            var p = Path.Join(dir, pluginName.ToLower());
            foreach (var ext in exts)
            {
                var path = p + "." + ext;
                if (File.Exists(path))
                {
                    return (path, ext);
                }
            }
            return (default, default);
        }

        private static Type GetFirstImplOf(Assembly a, Type interfaceType)
        {
            return a.GetExportedTypes().Where(t => t.GetInterfaces().Contains(interfaceType)).FirstOrDefault();
        }
        private static Type GetFirstImplOfWithNameContains(Assembly a, Type interfaceType, string name)
        {
            return a.GetExportedTypes().Where(t => t.GetInterfaces().Contains(interfaceType) && t.Name.Contains(name)).FirstOrDefault();
        }
    }
}
