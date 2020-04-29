using System.Threading.Tasks;

namespace SchulIT.SchildIccImporter.Settings
{
    public interface ISettingsManager
    {
        Task<ISettings> LoadSettingsAsync();
    }
}
