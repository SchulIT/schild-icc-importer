using SchildIccImporter.Settings;
using System;

namespace SchulIT.SchildIccImporter.Settings
{
    public interface ISchildSettings
    {
        bool OnlyVisibleEntities { get; }

        int[] StudentFilter { get; }

        DateTime? LeaveDate { get; }

        ConnectionType ConnectionType { get; }

        string ConnectionString { get; }
    }
}
