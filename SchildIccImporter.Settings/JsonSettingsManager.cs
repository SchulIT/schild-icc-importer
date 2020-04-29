using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SchulIT.SchildIccImporter.Settings
{
    public class JsonSettingsManager : ISettingsManager
    {
        public const string JsonFileName = "settings.json";
        public const string ApplicationName = "SchildIccImporter";
        public const string ApplicationVendor = "SchulIT";

        private ILogger<JsonSettingsManager> logger;

        public JsonSettingsManager(ILogger<JsonSettingsManager> logger)
        {
            this.logger = logger;
        }

        protected string GetSettingsDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                ApplicationVendor,
                ApplicationName
            );
        }

        protected string GetSettingsJsonPath()
        {
            return Path.Combine(
                GetSettingsDirectory(),
                JsonFileName
            );
        }

        public async Task<ISettings> LoadSettingsAsync()
        {
            var directory = GetSettingsDirectory();
            var path = GetSettingsJsonPath();

            if (!Directory.Exists(directory))
            {
                logger.LogInformation("Settings directory does not exist, creating...");

                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to create settings directory.");
                    throw e;
                }
            }

            var settings = new JsonSettings();

            if (File.Exists(path))
            {
                using (var reader = new StreamReader(path))
                {
                    var json = await reader.ReadToEndAsync();
                    JsonConvert.PopulateObject(json, settings);
                }
            }

            await SaveSettingsAsync(settings);

            return settings;
        }

        public async Task SaveSettingsAsync(JsonSettings jsonSettings)
        {
            try
            {

                using (var writer = new StreamWriter(GetSettingsJsonPath()))
                {
                    var json = JsonConvert.SerializeObject(jsonSettings, Formatting.Indented);
                    await writer.WriteAsync(json);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Cannot save settings.json. Proceed anyways.");
            }
        }
    }
}
