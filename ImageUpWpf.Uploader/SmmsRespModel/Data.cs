namespace ImageUpWpf.Uploader.SmmsRespModel
{
    using System;
    using Newtonsoft.Json;

    public partial class Data
    {
        [JsonProperty("file_id")]
        public long FileId { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("storename")]
        public string Storename { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("delete")]
        public Uri Delete { get; set; }

        [JsonProperty("page")]
        public Uri Page { get; set; }
    }
}
