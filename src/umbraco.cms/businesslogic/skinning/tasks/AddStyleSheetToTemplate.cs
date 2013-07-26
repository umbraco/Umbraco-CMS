using System;
using umbraco.interfaces.skinning;
using HtmlAgilityPack;
using umbraco.IO;

namespace umbraco.cms.businesslogic.skinning.tasks
{
    public class AddStyleSheetToTemplate : TaskType
    {
        public string TargetFile { get; set; }
        public string TargetSelector { get; set; }
        public string Media { get; set; }

        public AddStyleSheetToTemplate()
        {
            this.Name = "Add StyleSheet To Template";
            this.Description = "Will add an additional stylesheet to a template";
        }

        public override TaskExecutionDetails Execute(string Value)
        {
            TaskExecutionDetails d = new TaskExecutionDetails();


            HtmlDocument doc = new HtmlDocument();
            doc.Load(IO.IOHelper.MapPath(SystemDirectories.Masterpages) + "/" + TargetFile);

            //if (doc.DocumentNode.SelectSingleNode(string.Format("//link [@href = '{0}']", Value)) == null)
            //{

                HtmlNode target = doc.DocumentNode.SelectSingleNode(string.IsNullOrEmpty(TargetSelector) ? "//head" : TargetSelector.ToLower());

                if (target != null)
                {

                    HtmlNode s = doc.CreateElement("link");
                    //s.Name = "link";
                    s.Attributes.Append("rel", "stylesheet");
                    s.Attributes.Append("type", "text/css");


                    s.Attributes.Append("href", Value);


                    if (!string.IsNullOrEmpty(Media))
                        s.Attributes.Append("media", Media);

                    target.AppendChild(s);

                    doc.Save(IO.IOHelper.MapPath(SystemDirectories.Masterpages) + "/" + TargetFile);

                    d.TaskExecutionStatus = TaskExecutionStatus.Completed;
                    d.NewValue = Value;
                }
                else
                    d.TaskExecutionStatus = TaskExecutionStatus.Cancelled;   
            //}
            //else
            //    d.TaskExecutionStatus = TaskExecutionStatus.Cancelled;

            return d;
        }

        public override TaskExecutionStatus RollBack(string OriginalValue)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(IO.IOHelper.MapPath(SystemDirectories.Masterpages) + "/" + TargetFile);

            HtmlNode s = doc.DocumentNode.SelectSingleNode(string.Format("//link [@href = '{0}']", Value));

            if (s != null)
            {
                s.RemoveAll();

                doc.Save(IO.IOHelper.MapPath(SystemDirectories.Masterpages) + "/" + TargetFile);

                return TaskExecutionStatus.Completed;
            }
            else
                return TaskExecutionStatus.Cancelled;

            
        }

        public override string PreviewClientScript(string ControlClientId, string ClientSidePreviewEventType, string ClientSideGetValueScript)
        {
            return string.Format(
                   @"var link{4};
                    jQuery('#{0}').bind('{2}', function() {{ 
                        jQuery(link{4}).remove();
                        link{4} = jQuery('<link>');
                        link{4}.attr({{
                                type: 'text/css',
                                rel: 'stylesheet',
                                {3}
                                href:{1}
                        }});
                        jQuery('head').append(link{4}); 
                    }});


                    //cancel support
                    jQuery('#cancelSkinCustomization').click(function () {{ 
                        jQuery(link{4}).remove();       
                    }});
                    ",
                   ControlClientId,
                   ClientSideGetValueScript,
                   ClientSidePreviewEventType,
                   string.IsNullOrEmpty(Media) ? "" : string.Format("media :'{0}',",Media),
                   Guid.NewGuid().ToString().Replace("-", ""));
        }
    }
}
