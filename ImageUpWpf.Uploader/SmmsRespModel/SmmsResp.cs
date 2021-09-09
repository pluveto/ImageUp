namespace ImageUpWpf.Uploader.SmmsRespModel
{
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class SmmsResp
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }

        [JsonProperty("RequestId")]
        public string RequestId { get; set; }
    }
}
