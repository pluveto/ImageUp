using ImageUpWpf.Core;
using ImageUpWpf.Core.Plugin.Config;
using ImageUpWpf.Core.Plugin.Interface;
using ModernWpf.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageUpWpf
{
    /// <summary>
    /// PluginsSettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class PluginsSettingPage : Page
    {
        public PluginsSettingPage()
        {
            InitializeComponent();
            this.DataContext = ViewModel = new PluginsSettingPageVM();
        }

        public PluginsSettingPageVM ViewModel { get; }
    }

    public class PluginsSettingPageVM
    {

        public ObservableCollection<PluginSettingTabItem> TabData { get; }

        public PluginsSettingPageVM()
        {
            this.TabData = new ObservableCollection<PluginSettingTabItem>();
            var plugins = (Application.Current as App).AppContext.PluginManager.Plugins;
            foreach (var plugin in plugins)
            {
                this.TabData.Add(new PluginSettingTabItem(plugin));
            }
            Debug.WriteLine($"{this.TabData.Count} plugins loaded");
        }

    }

    public class PluginSettingTabItem : TabItem, INotifyPropertyChanged
    {
        public PluginSettingTabItem(IPlugin plugin)
        {
            Plugin = plugin;
            this.Header = buildHeaderUI();            
            SettingUI = buildSettingUI();
            this.Content = SettingUI;            
        }

        private string buildHeaderUI()
        {
            var img = new Image() { Source = base64ToImage(Plugin.PluginInfo.Icon), Width = 16, Height = 16, Margin = new Thickness(0, 0, 2, 0) };
            TabItemHelper.SetIcon(this, img);
            var headerText = Plugin.PluginInfo.Name;
            return headerText;

        }

        private ImageSource base64ToImage(string b64icon)
        {
            var imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.StreamSource = new MemoryStream(Convert.FromBase64String(b64icon));
            imageSource.EndInit();
            return imageSource;
        }

        // 创建设置界面
        private FrameworkElement buildSettingUI()
        {
            var panel = new StackPanel { Margin = new Thickness(10) };
            foreach (var (k, v) in this.Plugin.Config.ConfigFormMeta)
            {
                if (v.Type == ConfigItemType.String)
                {
                    var field = new TextBox();
                    ControlHelper.SetHeader(field, k);
                    ControlHelper.SetPlaceholderText(field, v.Placeholder);
                    var config = this.Plugin.Config;
                    var value = config.GetType().GetProperty(k).GetValue(config);
                    if (value != default)
                    {
                        field.Text = value.ToString();
                    }
                    else if (v.DefaultValue != default)
                    {
                        field.Text = v.DefaultValue;
                    }
                    panel.Children.Add(field);
                }

            }
            return panel;
        }

        public IPlugin Plugin { get; set; }
        public FrameworkElement SettingUI { get; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class SettingItem : INotifyPropertyChanged
    {
        public string Key { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
