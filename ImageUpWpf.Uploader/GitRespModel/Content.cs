namespace ImageUpWpf.Uploader.Model
{
    using System;
    using Newtonsoft.Json;

    public partial class Content
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("sha")]
        public string Sha { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("html_url")]
        public Uri HtmlUrl { get; set; }

        [JsonProperty("git_url")]
        public Uri GitUrl { get; set; }

        [JsonProperty("download_url")]
        public Uri DownloadUrl { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("_links")]
        public Links Links { get; set; }
    }
}
