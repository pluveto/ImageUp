namespace ImageUpWpf.Uploader.Model
{
    using System;
    using Newtonsoft.Json;

    public partial class Author
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }
    }
}
