namespace ImageUpWpf.Uploader.Model
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class Links
    {
        [JsonProperty("self")]
        public Uri Self { get; set; }

        [JsonProperty("git")]
        public Uri Git { get; set; }

        [JsonProperty("html")]
        public Uri Html { get; set; }
    }
}
