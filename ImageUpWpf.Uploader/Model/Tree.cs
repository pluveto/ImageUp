namespace ImageUpWpf.Uploader.Model
{
    using System;
    using Newtonsoft.Json;

    public partial class Tree
    {
        [JsonProperty("sha")]
        public string Sha { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }
    }
}
