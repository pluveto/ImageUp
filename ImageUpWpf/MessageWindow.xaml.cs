using GalaSoft.MvvmLight.Command;
using ImageUpWpf.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImageUpWpf
{
    /// <summary>
    /// 消息窗口。请通过下面的静态方法 Show 使用。
    /// </summary>
    public partial class MessageWindow : Window
    {
        public MessageWindowViewModel ViewModel;
        public MessageWindow()
        {
            InitializeComponent();
            ViewModel = new MessageWindowViewModel();
            DataContext = ViewModel;
        }

        public new MessageBoxResult ShowDialog() {
            base.ShowDialog();
            return ViewModel.Result;
        }


        public static MessageBoxResult Show(string messageBoxText, string caption)
        {
            var w = new MessageWindow();
            w.ViewModel.Caption = caption;
            w.ViewModel.Text = messageBoxText;
            return w.ShowDialog();
        }
        public static MessageBoxResult Show(string messageBoxText)
        {
            var w = new MessageWindow();
            w.ViewModel.Caption = "Alert";
            w.ViewModel.Text = messageBoxText;
            return w.ShowDialog();
        }




    }

    public class MessageWindowViewModel : INotifyPropertyChanged
    {

        public string Caption { get; set; }
        public string Text { get; set; }

        public MessageBoxResult Result { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public RelayCommand<Window> CloseCommand { get; set; }

        public MessageWindowViewModel()
        {
            CloseCommand = new RelayCommand<Window>(CloseCommandAction);
        }
        private void CloseCommandAction(Window w)
        {
            Result = MessageBoxResult.OK;
            w.Close();
        }
    }
}
