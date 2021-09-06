namespace ImageUpWpf.Uploader.Model
{
    using System;
    using Newtonsoft.Json;

    public partial class Commit
    {
        [JsonProperty("sha")]
        public string Sha { get; set; }

        [JsonProperty("node_id")]
        public string NodeId { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("html_url")]
        public Uri HtmlUrl { get; set; }

        [JsonProperty("author")]
        public Author Author { get; set; }

        [JsonProperty("committer")]
        public Author Committer { get; set; }

        [JsonProperty("tree")]
        public Tree Tree { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("parents")]
        public Parent[] Parents { get; set; }

        [JsonProperty("verification")]
        public Verification Verification { get; set; }
    }
}
