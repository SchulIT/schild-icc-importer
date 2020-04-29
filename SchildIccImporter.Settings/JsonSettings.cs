using Newtonsoft.Json;
using System.Collections.Generic;

namespace SchulIT.SchildIccImporter.Settings
{
    public class JsonSettings : ISettings
    {
        [JsonProperty("schild")]
        public ISchildSettings Schild { get; } = new JsonSchildSettings();

        [JsonProperty("icc")]
        public IIccSettings Icc { get; } = new JsonIccSettings();

        [JsonProperty("teacher_tag_mappings")]
        public Dictionary<string, string> TeacherTagMapping { get; } = new Dictionary<string, string>();

        
    }
}
