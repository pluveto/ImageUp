using ImageUpWpf.Core.Plugin;
using ImageUpWpf.Core.Plugin.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageUpWpf.Core.App
{
    public delegate void PluginLoadErrEvent(PluginLoadErrorType errorType, string message);
    public class IuAppContext
    {
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public AppConfig AppConfig { get; set; }
        public Helper Helper => new Helper { Context = this };
        public List<IUploader> ChainUploaders { get; private set; }
        public PluginManager PluginManager { get; private set; }
        public event PluginLoadErrEvent OnPluginLoadError;
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
                    var info = ((IPlugin)first).PluginInfo;
                    NLog.LogManager.GetCurrentClassLogger().Warn($"Using default uploader: {info.Name}({info.MainClass})");
                }
                else
                {
                    var msg = $"No default uploader";
                    NLog.LogManager.GetCurrentClassLogger().Warn(msg);
                    OnPluginLoadError?.Invoke(PluginLoadErrorType.NoDefaultUploader, msg);
                }
            }
            foreach (var uploaderName in chainUploaders)
            {
                var uploader = PluginManager.GetUploader(uploaderName);
                if (default == uploader)
                {
                    var msg = $"Uploader plugin not found: {uploaderName}";
                    NLog.LogManager.GetCurrentClassLogger().Warn(msg);
                    OnPluginLoadError?.Invoke(PluginLoadErrorType.NotFound, msg);
                    continue;
                }
                ChainUploaders.Add(uploader);
            }
        }
    }
}
