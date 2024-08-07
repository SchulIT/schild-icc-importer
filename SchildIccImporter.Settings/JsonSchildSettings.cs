using Newtonsoft.Json;
using SchildIccImporter.Settings;
using System;

namespace SchulIT.SchildIccImporter.Settings
{
    public class JsonSchildSettings : ISchildSettings
    {
        [JsonProperty("only_visible")]
        public bool OnlyVisibleEntities { get; set; } = true;

        [JsonProperty("student_status")]
        public int[] StudentFilter { get; set; } = Array.Empty<int>();

        [JsonProperty("leave_date")]
        public DateTime? LeaveDate { get; set; } = null;

        [JsonProperty("connection_type")]
        public ConnectionType ConnectionType { get; set; } = ConnectionType.MSSQL;

        [JsonProperty("connection_string")]
        public string ConnectionString { get; set; } = string.Empty;
    }
}
