using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MessageWindow.Show("Hello, world!");
        }

        private void uploadButton_Click(object sender, RoutedEventArgs e)
        {
            var path = createOpenFileDialog();
            if(default != path)
            {
                MessageWindow.Show(path.FirstOrDefault());
            }            

        }

        private IList<string> createOpenFileDialog()
        {
            var o = new OpenFileDialog();
            o.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.svg|" +
                "All files|*.*";
            var ret = o.ShowDialog();
            if(ret != true)
            {
                return null;
            }
            if(!o.FileNames.All(f => System.IO.File.Exists(f))){
                return null;
            }
            return o.FileNames;
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow().ShowDialog();
        }
    }
}
