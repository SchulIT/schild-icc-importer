using System;
using System.Collections.Generic;
using System.Text;

namespace SchildIccImporter.Gui.Message
{
    public class ResponseMessage
    {
        public ResponseMessageType Type { get; set; }

        public int ResponseCode { get; set; }

        public string ResponseBody { get; set; }
    }

    public enum ResponseMessageType
    {
        Success,
        Information,
        Error
    }
}
