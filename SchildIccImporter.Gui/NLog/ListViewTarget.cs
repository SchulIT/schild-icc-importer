using NLog;
using NLog.Targets;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace SchildIccImporter.Gui.Logger
{
    [Target("ListView")]
    public class ListViewTarget : TargetWithLayout
    {
        private object lockObject = new object();

        public ObservableCollection<LogEventInfo> Events { get; } = new ObservableCollection<LogEventInfo>();

        public ListViewTarget()
        {
            BindingOperations.EnableCollectionSynchronization(Events, lockObject);
        }

        protected override void Write(LogEventInfo logEvent)
        {
            lock (lockObject)
            {
                Events.Add(logEvent);
            }
        }
    }
}
