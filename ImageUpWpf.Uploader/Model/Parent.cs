namespace ImageUpWpf.Uploader.Model
{
    using System;
    using Newtonsoft.Json;

    public partial class Parent
    {
        [JsonProperty("sha")]
        public string Sha { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("html_url")]
        public Uri HtmlUrl { get; set; }
    }
}
