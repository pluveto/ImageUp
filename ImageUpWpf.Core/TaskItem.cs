﻿using System;
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

        public delegate void StateTransitionEvent(StateTransitionEventArgs eventArgs);
        public event StateTransitionEvent AfterTransition;

        private void Transition(TaskItemStatus nextStatus)
        {
            var old = Status;
            Status = old;
            var eventArgs = new StateTransitionEventArgs { Cancelled = false, From = old, To = Status };
            AfterTransition?.Invoke(eventArgs);
        }

        public async Task<string> Run()
        {
            if (Status != TaskItemStatus.Ready)
            {
                logger.Warn($"Unable to run: current status={Status}. Task = {this.Name}");
                return null;
            }
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
            catch(Exception e)
            {
                logger.Error("UploadException: " + e.Message);
                Transition(TaskItemStatus.Error);
                Message = e.Message;
                return null;
            }
            Transition(TaskItemStatus.Completed);

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