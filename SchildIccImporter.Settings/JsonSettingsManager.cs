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

        private readonly ILogger<JsonSettingsManager> logger;

        public JsonSettingsManager(ILogger<JsonSettingsManager> logger)
        {
            this.logger = logger;
        }

        protected static string GetSettingsDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                ApplicationVendor,
                ApplicationName
            );
        }

        protected static string GetSettingsJsonPath()
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
                logger.LogInformation("Einstellungsverzeichnis existiert nicht, lege es an.");

                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Fehler beim Erstellen des Einstellungsverzeichnisses. Bitte Schreibrechte überprüfen.");
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

            //await SaveSettingsAsync(settings);

            return settings;
        }

        public async Task SaveSettingsAsync(ISettings jsonSettings)
        {
            try
            {
                using (var writer = new StreamWriter(GetSettingsJsonPath()))
                {
                    var json = JsonConvert.SerializeObject(jsonSettings, Formatting.Indented);
                    await writer.WriteAsync(json);
                }

                logger.LogInformation("Einstellungen erfolgreich gespeichert.");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Einstellungen konnten nicht gespeichert werden. Fahre dennoch fort.");
            }
        }
    }
}
