using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageUpWpf.Core
{
    public class IuAppContext
    {
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public AppConfig AppConfig { get; set; }
        public List<IUploader> ChainUploaders { get; private set; }
        public PluginManager PluginManager { get; private set; }

        public void Init()
        {
            NLog.LogManager.GetCurrentClassLogger().Trace("Init");
            LoadPluginManager();
            LoadConfig();
            LoadChainUploaders(AppConfig.ChainUploaders);

        }

        private void LoadPluginManager()
        {
            PluginManager = new PluginManager();
            PluginManager.LoadPlugins();
            Inject(PluginManager);
        }

        public void LoadConfig()
        {
            // todo: default config
            var c = Utils.Config.GetConfig<AppConfig>(AppDomain.CurrentDomain.BaseDirectory, "app_config");
            AppConfig = default == c ? AppConfig.Default() : c;
        }

        public void Inject(PluginManager pm)
        {
            foreach (var plugin in pm.Plugins)
            {
                var props = plugin.GetType().GetProperties(); 
                var ctx = props.SingleOrDefault(x => x.PropertyType == typeof(IuAppContext));
                if (default != ctx)
                {
                    ctx.SetValue(plugin, this);
                    logger.Info($"Inject IuAppContext for {plugin}.{ctx.Name}");
                }
            }
        }

        public void LoadChainUploaders(List<string> chainUploaders)
        {
            ChainUploaders = new List<IUploader>();
            if (chainUploaders.Count == 0)
            {
                var first = PluginManager.Uploaders.FirstOrDefault();
                if (default != first)
                {
                    ChainUploaders.Add(first);
                }
            }
            foreach (var uploaderName in chainUploaders)
            {
                var uploader = PluginManager.GetUploader(uploaderName);
                if (default == uploader)
                {
                    NLog.LogManager.GetCurrentClassLogger().Warn($"Uploader plugin not found: {uploader}");
                    continue;
                }
                ChainUploaders.Add(uploader);
            }
        }
    }
}
