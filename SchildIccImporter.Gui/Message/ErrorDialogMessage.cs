using System;

namespace SchildIccImporter.Gui.Message
{
    public class ErrorDialogMessage : DialogMessage
    {
        public Exception Exception { get; set; }
    }
}
