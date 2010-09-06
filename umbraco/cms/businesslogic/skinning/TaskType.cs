using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.interfaces.skinning;
using System.Xml;
using System.Reflection;

namespace umbraco.cms.businesslogic.skinning
{
    public abstract class TaskType : ProviderBase, ITaskType
    {
        public abstract TaskExecutionDetails Execute(string Value);

        public virtual TaskExecutionStatus RollBack(string OriginalValue)
        {
            return Execute(OriginalValue).TaskExecutionStatus;
        }

        public abstract string PreviewClientScript(string ControlClientId, string ClientSidePreviewEventType, string ClientSideGetValueScript);

        public String Value { get; set; }

        public virtual XmlNode ToXml(string OriginalValue, string NewValue)
        {
            XmlDocument d = new XmlDocument();

            XmlNode n = d.CreateElement("Task");

            XmlAttribute type = d.CreateAttribute("type");
            type.Value = this.GetType().Name;
            n.Attributes.Append(type);

            if (!this.GetType().FullName.Contains("umbraco.cms.businesslogic.skinning"))
            {
                XmlAttribute assembly = d.CreateAttribute("assembly");
                assembly.Value = this.GetType().Assembly.FullName; 
                n.Attributes.Append(assembly);
            }

            XmlAttribute executed = d.CreateAttribute("executed");
            executed.Value = DateTime.Now.ToString("s");
            n.Attributes.Append(executed);

            foreach(PropertyInfo p in this.GetType().GetProperties())
            {
                if(p.Name != "Name" && p.Name != "Description")
                {
                XmlNode pNode = d.CreateElement(p.Name);
                pNode.InnerText = p.GetValue(this,null) != null ? p.GetValue(this,null).ToString() : string.Empty;

                n.AppendChild(pNode);
                }
            }


            XmlNode origValNode = d.CreateElement("OriginalValue");
            origValNode.InnerText = OriginalValue;
            n.AppendChild(origValNode);

            XmlNode newValNode = d.CreateElement("NewValue");
            newValNode.InnerText = NewValue;
            n.AppendChild(newValNode);

            return n;
        }
    }
}
