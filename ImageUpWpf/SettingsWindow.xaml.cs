using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Frame = ModernWpf.Controls.Frame;

namespace ImageUpWpf
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindowViewModel ViewModel { get; private set; }

        public SettingsWindow()
        {
            InitializeComponent();
            this.DataContext = ViewModel = new SettingsWindowViewModel();
            RootFrame = rootFrame;
            NavigateToSelected();

        }

        private void SettingPageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NavigateToSelected();
        }

        void NavigateToSelected()
        {
            if (SettingPageListView.SelectedValue is Type type)
            {
                _ = (RootFrame?.Navigate(type));
                Debug.WriteLine("Go to " + type.ToString());
            }
        }
        public static Frame RootFrame
        {
            get => _rootFrame.Value;
            private set => _rootFrame.Value = value;
        }

        private static readonly ThreadLocal<Frame> _rootFrame = new ThreadLocal<Frame>();

        private void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                RootFrame.RemoveBackEntry();
            }
        }

        private void RootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            SettingPageListView.SelectedValue = RootFrame.CurrentSourcePageType;
        }
    }

    public class SettingsWindowViewModel : INotifyPropertyChanged
    {
        public SettingsWindowViewModel() { }

        public event PropertyChangedEventHandler PropertyChanged;
    }



    public class SettingPagesData : List<SettingPageDataItem>
    {
        public SettingPagesData()
        {
            AddPage(typeof(PluginsSettingPage), "Plugins");
        }

        private void AddPage(Type pageType, string displayName = null)
        {
            Add(new SettingPageDataItem(pageType, displayName));
        }
    }

    public class SettingPageDataItem
    {
        public SettingPageDataItem(Type pageType, string title = null)
        {
            PageType = pageType;
            Title = title ?? pageType.Name.Replace("Page", null);
        }

        public string Title { get; }

        public Type PageType { get; }

        public override string ToString()
        {
            return Title;
        }
    }
}
