using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace umbraco.interfaces.skinning
{
    public interface ITaskType
    {
        String Name { get; set; }
        String Value { get; set; }

        TaskExecutionDetails Execute(string Value);

        TaskExecutionStatus RollBack(string OriginalValue);

        string PreviewClientScript(string ControlClientId, string ClientSidePreviewEventType, string ClientSideGetValueScript);

        XmlNode ToXml(string OriginalValue, string NewValue);
    }
}
