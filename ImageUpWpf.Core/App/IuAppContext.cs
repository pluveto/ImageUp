using ImageUpWpf.Core.Plugin;
using ImageUpWpf.Core.Plugin.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageUpWpf.Core.App
{
    public delegate void PluginLoadErrEvent(PluginLoadErrorType errorType, string message);
    /// <summary>
    /// 程序的状态上下文
    /// </summary>
    public class IuAppContext
    {
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// App 配置
        /// </summary>
        public AppConfig AppConfig { get; set; }
        /// <summary>
        /// 带有状态，因此需要实例化的 Helper 函数
        /// </summary>
        public Helper Helper => new Helper { Context = this };
        /// <summary>
        /// 图片上传插件
        /// </summary>
        public List<IUploader> ChainUploaders { get; private set; }
        /// <summary>
        /// 插件管理程序
        /// </summary>
        public PluginManager PluginManager { get; private set; }
        /// <summary>
        /// 插件错误处理程序
        /// </summary>
        public event PluginLoadErrEvent OnPluginLoadError;
        /// <summary>
        /// 当程序初始化时调用
        /// </summary>
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
        /// <summary>
        /// 将本程序注入到各插件的 IuAppContext 属性中
        /// </summary>
        /// <param name="pm"></param>
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
