namespace ImageUpWpf.Uploader.Model
{
    using Newtonsoft.Json;

    public partial class CreateResp
    {
        [JsonProperty("content")]
        public Content Content { get; set; }

        [JsonProperty("commit")]
        public Commit Commit { get; set; }
    }
}
