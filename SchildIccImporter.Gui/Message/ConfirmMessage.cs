using System;
using System.Collections.Generic;
using System.Text;

namespace SchildIccImporter.Gui.Message
{
    public class ConfirmMessage
    {
        public string Title { get; set; }

        public string Content { get; set; }

        public Action ConfirmAction { get; set; }

        public Action CancelAction { get; set; }
    }
}
