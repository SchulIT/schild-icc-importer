using System.Collections.Generic;

namespace SchulIT.SchildIccImporter.Settings
{
    public interface ISettings
    {
        ISchildSettings Schild { get; }

        IIccSettings Icc { get; }

        Dictionary<string, string> TeacherTagMapping { get; }
    }
}
