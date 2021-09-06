using System;
using System.IO;
using System.Threading.Tasks;

namespace ImageUpWpf.Core
{
    public class TaskItem
    {
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public class StateTransitionEventArgs : EventArgs
        {
            public TaskItemStatus From { get; set; }
            public TaskItemStatus To { get; set; }
            public bool Cancelled { get; set; }

        }

        public bool HasError()
        {
            return Message != string.Empty;
        }

        public Stream Stream { get; set; }

        // Name for upload
        public string Name { get; set; }
        public string Message { get; set; }
        public string Url { get; set; }

        public IUploader Uploader { get; set; }

        public TaskItemStatus Status { get; set; } = TaskItemStatus.Ready;
        public string Id { get; set; }

        public delegate void StateTransitionEvent(StateTransitionEventArgs eventArgs);
        public event StateTransitionEvent AfterTransition;

        public TaskItem()
        {
            AfterTransition += TaskItem_AfterTransition;
        }
        private void Transition(TaskItemStatus nextStatus)
        {
            var old = Status;
            Status = nextStatus;
            var eventArgs = new StateTransitionEventArgs { Cancelled = false, From = old, To = Status };
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
                    Transition(TaskItemStatus.Uploading);
                    Url = await Uploader.Upload(s, Name);
                }
            }
            catch (UploadException e)
            {
                logger.Error("UploadException: " + e.Message);
                Transition(TaskItemStatus.Error);
                Status = TaskItemStatus.Error;
                Message = e.Message;
                return null;
            }
            catch (Exception e)
            {
                logger.Error("Exception: " + e.Message);
                Transition(TaskItemStatus.Error);
                Message = e.Message;
                return null;
            }
            Transition(TaskItemStatus.Completed);
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