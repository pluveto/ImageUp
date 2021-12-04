using ImageUpWpf.Core.Upload;
using Microsoft.Toolkit.Uwp.Notifications;
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
using Windows.Foundation.Collections;

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
            // Listen to notification activation
            ToastNotificationManagerCompat.OnActivated += OnToastActivated;
        }

        private void OnToastActivated(ToastNotificationActivatedEventArgsCompat toastArgs)
        {
            // Obtain the arguments from the notification
            ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

            // Obtain any user input (text boxes, menu selections) from the notification
            ValueSet userInput = toastArgs.UserInput;

            // Need to dispatch to UI thread if performing UI operations
            Application.Current.Dispatcher.Invoke(delegate
            {
                // TODO: Show the corresponding content
                MessageBox.Show("Toast activated. Args: " + toastArgs.Argument);
            });
        }

        private void uploadButton_Click(object sender, RoutedEventArgs e)
        {
            var path = createOpenFileDialog();
            if (default == path || path.Count == 0)
            {
                return;
            }
            handleUploadImages(path.ToArray());

        }

        private async void handleUploadImages(string[] path)
        {
            var context = ((App)Application.Current).AppContext;
            var task = new UploadTask
            {
                UploadMode = ChainUploadMode.Parallel,
                NamingTemplate = context.AppConfig.NamingTemplate
            }
            .AddChainUploaders(context.ChainUploaders)
            .AddFiles(path, AddFileCallback);
            
            var ret = await task.Run();            
            var clipboardCache = new StringBuilder();
            // 每个 Group 对应一个文件
            foreach (var (groupId, group) in ret)
            {
                var url = group.GetUrl();
                if(url == default)
                {
                    OnGroupFailed(group);
                    continue;
                }
                // TODO：支持更丰富的剪贴板格式选项
                // TODO: 对 UploadFileName 进行转义
                clipboardCache.AppendLine($"![{group.UploadFileName}]({url})");
            }
            Clipboard.SetText(clipboardCache.ToString());
        }

        private void OnGroupFailed(TaskGroup group)
        {
            var errorItem = group.Items.FirstOrDefault(x => x.Value.Message != default);
            string errorMessage = "未知错误";
            string uploader = "未知";
            if(errorItem.Key != null)
            {
                errorMessage = errorItem.Value.Message;
                uploader = errorItem.Value.Uploader.GetName();
            }
            if(group.Items.Count == 0)
            {
                errorMessage = "未创建上传任务,请检查插件是否正常运行!";
            }

            new ToastContentBuilder()
                .AddText("上传失败")
                .AddText($"文件 {group.UploadFileName} 未能上传。")
                .AddText($"错误消息：{errorMessage}(插件: {uploader})")
                .Show();
        }

        private void AddFileCallback(object sender, TaskGroupCreatingResult e)
        {
            
        }

        private IList<string> createOpenFileDialog()
        {
            var o = new OpenFileDialog();
            o.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.svg|" +
                "All files|*.*";
            var ret = o.ShowDialog();
            if (ret != true)
            {
                return null;
            }
            if (!o.FileNames.All(f => System.IO.File.Exists(f)))
            {
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
