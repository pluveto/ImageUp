using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageUpWpf.Core
{
    public class TaskGroup
    {
        public Stream Stream { get; set; }
        /// <summary>
        /// key: GUID
        /// </summary>
        private Dictionary<string, TaskItem> Items { get; }
        /// <summary>
        /// 用于上传的短文件名
        /// </summary>
        public string UploadFileName { get; set; }
        public string Id { get; set; }
        public TaskGroup()
        {
            Items = new Dictionary<string, TaskItem>();
        }

        public void AddUploader(IUploader u)
        {
            var guid = Guid.NewGuid().ToString();
            Items[guid] = new TaskItem { Id = guid, Uploader = u, Stream = this.Stream, Name = UploadFileName };
        }

        public IList<string> GetUrls()
        {
            return Items.Where(x => x.Value.Url.Length > 0).Select(x => x.Value.Url).ToList();
        }

        public string GetUrl()
        {
            return Items.Where(x => !string.IsNullOrEmpty(x.Value.Url)).Select(x => x.Value.Url).FirstOrDefault();
        }

        public async Task<Dictionary<string, TaskItem>> Run()
        {
            var tasks = Items.Select(i => i.Value.Run()).ToArray();
            await Task.WhenAll(tasks);
            return Items;
        }
    }

    public class UploadTask
    {
        private Logger logger;

        /// <summary>
        /// key: GUID
        /// </summary>
        private Dictionary<string, TaskGroup> Groups { get; set; } = new Dictionary<string, TaskGroup>();
        public IList<IUploader> ChainUploaders { get; set; } = new List<IUploader>();
        public ChainUploadMode UploadMode { get; set; } = ChainUploadMode.Parallel;
        public string NamingTemplate { get; set; }

        public UploadTask()
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Trace("Task created");
        }
        public UploadTask AddChainUploader(IUploader uploader, bool insert = false)
        {
            ChainUploaders.Insert(insert ? 0 : ChainUploaders.Count, uploader);
            return this;
        }

        public UploadTask AddChainUploaders(IList<IUploader> uploaders, bool insert = false)
        {
            logger.Info($"Add {uploaders.Count} uploaders");
            foreach (var uploader in uploaders)
            {
                ChainUploaders.Insert(insert ? 0 : ChainUploaders.Count, uploader);
            }
            return this;
        }
        public UploadTask AddFile(string fileName, EventHandler<TaskGroupCreatingResult> result)
        {
            logger.Info("Add " + fileName);
            var info = new System.IO.FileInfo(fileName);

            if (!info.Exists)
            {
                result.Invoke(this, new TaskGroupCreatingResult { FileName = fileName, HasError = true, Message = "No such file", TaskGroup = null });
                return this;
            }
            // todo: configurable limit
            var sizeLimit = 5 * Utils.Size.MB;
            if (info.Length > sizeLimit)
            {
                result.Invoke(this, new TaskGroupCreatingResult { FileName = fileName, HasError = true, Message = "File too large: over " + Utils.Size.ReadableSize(sizeLimit), TaskGroup = null });
                return this;
            }
            var ns = new NamingStep { NamingTemplate = NamingTemplate };
            var guid = Guid.NewGuid().ToString();
            var g = new TaskGroup { Id = guid, Stream = info.OpenRead(), UploadFileName = ns.Execute(fileName) };
            foreach (var uploader in ChainUploaders)
            {
                g.AddUploader(uploader);
            }
            result.Invoke(this, new TaskGroupCreatingResult
            {
                FileName = fileName,
                HasError = false,
                Message = null,
                TaskGroup = g
            });
            this.Groups.Add(guid, g);
            return this;
        }
        public UploadTask AddFiles(string[] fileNames, EventHandler<TaskGroupCreatingResult> result)
        {
            logger.Info($"Add {fileNames.Length} files");
            foreach (var fileName in fileNames)
            {
                AddFile(fileName, result);
            }
            return this;
        }
        public async Task<Dictionary<string, TaskGroup>> Run()
        {
            logger.Info($"Start {this.Groups.Count} groups of tasks.");
            var tasks = this.Groups.Select(g => g.Value.Run()).ToArray();
            _ = await Task.WhenAll(tasks);
            logger.Info($"Done all groups of tasks.");
            return this.Groups;
        }
    }

    public enum ChainUploadMode
    {
        // 依次尝试各个 Uploader 直至成功
        Substitute,
        // 同时使用各个 Uploader
        Parallel
    }
}