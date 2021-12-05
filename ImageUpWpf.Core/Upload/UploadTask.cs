using ImageUpWpf.Core.Plugin.Interface;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageUpWpf.Core.Upload
{
    public delegate void TaskGroupUploadingEvent();
   
    /// <summary>
    /// 对于每个文件，将产生一个任务组 TaskGroup 。任务组中有一些 TaskItem，每个 TaskItem 都将被各个插件上传。
    /// </summary>
    public class UploadTask
    {
        private Logger logger;

        /// <summary>
        /// key: GUID
        /// </summary>
        public Dictionary<string, TaskGroup> Groups { get; private set; } = new Dictionary<string, TaskGroup>();
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
        public UploadTask AddFile(string fileName, EventHandler<TaskGroupCreatingResult> result, bool baseNameMode = true)
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
            string uploadName;
            if (baseNameMode)
            {
                uploadName = ns.Execute(Path.GetFileName(fileName));
            }
            else
            {
                uploadName = ns.Execute(fileName);
            }
            var g = new TaskGroup { Id = guid, Stream = info.OpenRead(), UploadFileName = uploadName };
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
        /// <summary>
        /// 返回任务组列表。key 为任务组的 GUID
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<string, TaskGroup>> Run()
        {
            logger.Info($"Start {this.Groups.Count} groups of tasks.");
            var tasks = this.Groups.Select(g => g.Value.Run()).ToArray();
            _ = await Task.WhenAll(tasks);
            logger.Info($"Done all groups of tasks.\n");
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