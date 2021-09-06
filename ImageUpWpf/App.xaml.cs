using ImageUpWpf.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ImageUpWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public AppContext AppContext { get; }
        public App()
        {
            AppContext = new AppContext();
        }
    }

    // 此处相当于一个全局的状态保存位置
    public class AppContext
    {
        public PluginManager PluginManager { get; }
        public IUploader DefaultUploader { get; set; }
        public AppContext()
        {
            PluginManager = new PluginManager();
            PluginManager.LoadPlugins();
            if(PluginManager.Uploaders.Count > 0)
            {
                DefaultUploader = PluginManager.Uploaders.First();
            }
        }

    }
}
