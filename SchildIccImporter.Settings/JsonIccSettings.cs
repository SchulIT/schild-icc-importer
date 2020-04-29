using Newtonsoft.Json;

namespace SchulIT.SchildIccImporter.Settings
{
    class JsonIccSettings : IIccSettings
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
