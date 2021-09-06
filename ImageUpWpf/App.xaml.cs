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
        public IuAppContext AppContext { get; }
        public App()
        {
            AppContext = new IuAppContext();
            AppContext.Init();
        }
    }

}
