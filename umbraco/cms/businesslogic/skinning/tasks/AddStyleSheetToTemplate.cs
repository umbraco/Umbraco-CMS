using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.interfaces.skinning;
using HtmlAgilityPack;
using umbraco.IO;

namespace umbraco.cms.businesslogic.skinning.tasks
{
    public class AddStyleSheetToTemplate : TaskType
    {
        public string TargetFile { get; set; }
        public string StyleSheet { get; set; }
        public string Media { get; set; }

        public AddStyleSheetToTemplate()
        {
            this.Name = "Add StyleSheet To Template";
            this.Description = "Will add an additional stylesheet to a template";
        }

        public override TaskExecutionDetails Execute(string Value)
        {
            TaskExecutionDetails d = new TaskExecutionDetails();

            //open template

            HtmlDocument doc = new HtmlDocument();
            doc.Load(IO.IOHelper.MapPath(SystemDirectories.Masterpages) + "/" + TargetFile);


            HtmlNode head = doc.DocumentNode.SelectSingleNode("//head");

            if (head != null)
            {
                HtmlNode s = new HtmlNode(HtmlNodeType.Element, doc, 0);
                s.Name = "link";
                s.Attributes.Add("rel", "stylesheet");
                s.Attributes.Add("type", "text/css");
                s.Attributes.Add("href", StyleSheet);

                if(!string.IsNullOrEmpty(Media))
                    s.Attributes.Add("media", Media);
            }


            doc.Save(IO.IOHelper.MapPath(SystemDirectories.Masterpages) + "/" + TargetFile);

            d.TaskExecutionStatus = TaskExecutionStatus.Completed;
            d.NewValue = Value;
            //save

            return d;
        }

        public override string PreviewClientScript(string ControlClientId, string ClientSidePreviewEventType, string ClientSideGetValueScript)
        {
            //will be run on installation so currently no need for a client preview script
            return string.Empty;
        }
    }
}
