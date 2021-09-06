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
        private NLog.Logger logger;

        public PluginManager()
        {
            this.logger = NLog.LogManager.GetCurrentClassLogger();
            this.PluginDir = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "plugins");
        }
        public string PluginDir { get; set; }

        public List<IPlugin> Plugins { get; private set; }
        /// <summary>
        /// LoadPlugins 加载插件。将会从本地扫描读取 DLL，然后创建插件的实例和配置文件的实例。
        /// 此函数应在程序启动时调用。
        /// </summary>
        public void LoadPlugins()
        {
            this.Plugins = new List<IPlugin>();
            this.Uploaders = new List<IUploader>();
            var pluginAsm = SearchPluginDlls();
            foreach (var a in pluginAsm)
            {
                var pluginClassType = GetFirstImplOf(a, typeof(IPlugin));
                var plugin = (IPlugin)Activator.CreateInstance(pluginClassType);

                var pluginConfigType = GetFirstImplOf(a, typeof(IPluginConfig));
                plugin.Config = LoadConfig(plugin.PluginInfo.MainClass, pluginConfigType);

                Plugins.Add(plugin);
            }

            foreach (var plugin in Plugins)
            {
                if (plugin.GetType().GetInterfaces().Contains(typeof(IUploader)))
                {
                    logger.Trace("Found uploader: " + plugin.PluginInfo.Name);
                    Uploaders.Add((IUploader)plugin);
                }
            }
        }

        public List<PluginInfo> PluginInfos => Plugins.Select(x => x.PluginInfo).ToList();
        /// <summary>
        /// Uploaders are availables all Uploaders. Filled after LoadPlugins()
        /// </summary>
        public List<IUploader> Uploaders { get; internal set; }

        public IUploader GetUploader(string mainClass)
        {
            return this.Uploaders.SingleOrDefault(u => u.GetType().Name == mainClass);
        }

        private IPluginConfig LoadConfig(string mainClass, Type pluginConfigType)
        {
            if(default == pluginConfigType)
            {
                return null;
            }
            // or create config for it
            var x = typeof(Utils.Config).GetMethod("GetConfig", BindingFlags.Public | BindingFlags.Static);
            // GetConfig<pluginConfigType>(pluginDir, pluginName);
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
