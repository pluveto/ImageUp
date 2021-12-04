using ImageUpWpf.Core.Plugin.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageUpWpf.Core.Upload
{
    /// <summary>
    /// 每个 TaskGroup 只负责一个 Stream 的上传。
    /// 而这个 Stream 的上传可以通过不同的上传插件实现。
    /// 为了区分，各个上传插件的任务被指派为不同的 TaskItem。
    /// </summary>
    public class TaskGroup
    {
        public Stream Stream { get; set; }
        /// <summary>
        /// key: GUID
        /// </summary>
        public Dictionary<string, TaskItem> Items { get; private set; }
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
        /// <summary>
        /// 获取所有可用的上传后 Url
        /// </summary>
        /// <returns></returns>
        public IList<string> GetUrls()
        {
            return Items.Where(x => x.Value.Url.Length > 0).Select(x => x.Value.Url).ToList();
        }
        /// <summary>
        /// 获取一个可用的上传后 Url
        /// </summary>
        /// <returns></returns>
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
}
