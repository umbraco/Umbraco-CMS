using System.IO;
using System;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Collections;
using System.Web.UI;
using ClientDependency.Core;
using Umbraco.Core.IO;
using umbraco.presentation;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Web.UI;

namespace umbraco.controls
{
    public class SaveClickEventArgs : EventArgs
    {
        public string Message { get; set; }
        public SpeechBubbleIcon IconType { get; set; }

        public SaveClickEventArgs(string message)
        {
            Message = message;
            IconType = SpeechBubbleIcon.Success;
        }
    }
}
