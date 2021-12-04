using ImageUpWpf.Core.Plugin.Interface;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ImageUpWpf.Core.Upload
{
    /// <summary>
    /// 描述单个上传任务的状态。
    /// 每个 TaskItem 可以被一个插件所上传。
    /// 每个 TaskItem 上传的对象可以是文件或者其它，取决于使用者。对它而言只负责上传流
    /// </summary>
    public class TaskItem
    {
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public class StateTransitionEventArgs : EventArgs
        {
            public TaskItem TaskItem { get; set; }
            public TaskItemStatus From { get; set; }
            public TaskItemStatus To { get; set; }
            public bool Cancelled { get; set; }

        }

        public bool HasError()
        {
            return Message != string.Empty;
        }
        /// <summary>
        /// 流在对象创建时被给出
        /// </summary>
        public Stream Stream { get; set; }

        /// <summary>
        /// 通常是上传的文件名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 若发生错误，此字符串将是错误消息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 上传成功后的 Url
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 负责上传本 TaskItem 的插件
        /// </summary>
        public IUploader Uploader { get; set; }

        public TaskItemStatus Status { get; set; } = TaskItemStatus.Ready;
        public string Id { get; set; }

        public delegate void StateTransitionEvent(StateTransitionEventArgs eventArgs);
        public event StateTransitionEvent AfterTransition;

        public TaskItem()
        {
            AfterTransition += TaskItem_AfterTransition;
        }
        /// <summary>
        /// 报告状态转移情况，以便各个监听器能够得知
        /// </summary>
        /// <param name="nextStatus"></param>
        private void ReportTransition(TaskItemStatus nextStatus)
        {
            var old = Status;
            Status = nextStatus;
            var eventArgs = new StateTransitionEventArgs { Cancelled = false, From = old, To = Status, TaskItem = this };
            AfterTransition?.Invoke(eventArgs);
        }

        private void TaskItem_AfterTransition(StateTransitionEventArgs a)
        {
            logger.Info($"Task status trans: {a.From}->{a.To}, of " + Id);
        }

        public async Task<string> Run()
        {
            if (Status != TaskItemStatus.Ready)
            {
                logger.Warn($"Unable to run: current status={Status}. Task = {this.Name}");
                return null;
            }
            logger.Info($"Start task id={Id} name={Name}");
            try
            {
                using (var s = new MemoryStream())
                {
                    Stream.CopyTo(s);
                    ReportTransition(TaskItemStatus.Uploading);
                    s.Position = 0;
                    // 本函数的核心代码就是完成上传
                    Url = await Uploader.Upload(s, Name);
                }
            }
            catch (UploadException e)
            {
                logger.Error("UploadException: " + e.Message);
                ReportTransition(TaskItemStatus.Error);
                Status = TaskItemStatus.Error;
                Message = e.Message;
                return null;
            }
            catch (Exception e)
            {
                logger.Error("Exception: " + e.Message);
                ReportTransition(TaskItemStatus.Error);
                Message = e.Message;
                return null;
            }
            ReportTransition(TaskItemStatus.Completed);
            logger.Info($"Done task id={Id} name={Name}");
            return Url;
        }
    }
    public enum TaskItemStatus
    {
        Ready,
        Waiting,
        Uploading,
        Completed,
        Error
    }
}