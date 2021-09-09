using ImageUpWpf.Core.Plugin.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ImageUpWpf.Core.Plugin
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
            logger.Debug("Loading plugins");
            var pluginAsm = SearchPluginDlls();
            foreach (var a in pluginAsm)
            {
                var pluginClassTypes = GetImplementsOf(a, typeof(IPlugin));
                logger.Debug($"Found {pluginClassTypes.Length} implements of IPlugin");
                foreach (var pluginClassType in pluginClassTypes)
                {
                    var plugin = (IPlugin)Activator.CreateInstance(pluginClassType);
                    logger.Debug($"Create plugin instance({pluginClassType.Name})");
                    // var pluginConfigType = GetFirstImplOf(a, typeof(IPluginConfig));
                    var pluginConfigType = GetPluginConfigType(plugin);//  GetClassConfigType(pluginClassType);
                    plugin.PluginInfo.MainClass = pluginClassType.Name;
                    plugin.Config = LoadConfig(plugin.PluginInfo.MainClass, pluginConfigType);
                    Plugins.Add(plugin);
                }
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

        private Type GetPluginConfigType(IPlugin plugin)
        {
            var info = GetPropInfo<IPlugin>(plugin => plugin.Config);
            return info.GetGetMethod().Invoke(plugin, null).GetType();
        }
        public PropertyInfo GetPropInfo<T>(Expression<Func<T, object>> lambda)
        {
            var member = lambda.Body as MemberExpression;
            var prop = member.Member as PropertyInfo;
            return prop;
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

        private IPluginConfig LoadConfig(string pluginMainClass, Type pluginConfigType)
        {
            if (default == pluginConfigType)
            {
                logger.Warn($"Plugin config not loaded for {pluginMainClass}");
                return null;
            }
            logger.Warn($"Plugin config type {pluginConfigType.Name} loading for " + pluginMainClass);

            // or create config for it
            var x = typeof(Utils.Config).GetMethod(nameof(Utils.Config.GetConfig), BindingFlags.Public | BindingFlags.Static);
            // GetConfig<pluginConfigType>(pluginDir, pluginName);
            IPluginConfig config = (IPluginConfig)x.MakeGenericMethod(pluginConfigType).Invoke(null, new object[] { PluginDir, pluginMainClass });
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
            logger.Debug($"Searching Dlls in {PluginDir}");
            foreach (var dll in dlls)
            {
                logger.Debug($"Found {dll}");
                try
                {
                    var a = Assembly.LoadFile(dll);
                    logger.Debug($"Assembly loaed");

                    var impl = GetImplementsOf(a, typeof(IPlugin));
                    if (default != impl && 0 < impl.Length)
                    {
                        pluginDlls.Add(a);
                        logger.Debug($"Add assembly {a.FullName}");
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    logger.Error(e);
                    continue;
                }
            }
            return pluginDlls;
        }
        private static Type[] GetImplementsOf(Assembly a, Type interfaceType)
        {
            var types = a.GetExportedTypes();
            return types.Where(t => t.GetInterfaces().Contains(interfaceType)).ToArray();
        }
    }
}
