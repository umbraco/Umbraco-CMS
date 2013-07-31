using System;
using umbraco.interfaces.skinning;
using umbraco.IO;
using HtmlAgilityPack;

namespace umbraco.cms.businesslogic.skinning.tasks
{
    public class ModifyTemplate : TaskType
    {
        public string TargetFile { get; set; }
        public string TargetID { get; set; }
        public string TargetAttribute { get; set; }

        public ModifyTemplate()
        {
            this.Name = " Modify template";
            this.Description = "Will modify a template";
        }

        public override TaskExecutionDetails Execute(string Value)
        {
            TaskExecutionDetails d = new TaskExecutionDetails();

            //open template

            HtmlDocument doc = new HtmlDocument();
            doc.Load(IO.IOHelper.MapPath(SystemDirectories.Masterpages) + "/" +TargetFile);

            if (doc.DocumentNode.SelectNodes(string.Format("//*[@id = '{0}']", TargetID)) != null)
            {
                foreach (HtmlNode target in doc.DocumentNode.SelectNodes(string.Format("//*[@id = '{0}']", TargetID)))
                {
                    if (string.IsNullOrEmpty(TargetAttribute))
                    {
                        d.OriginalValue = target.InnerHtml;
                        target.InnerHtml = Value;
                    }
                    else
                    {
                        if (target.Attributes[TargetAttribute] == null)
                        {
                            target.Attributes.Append(TargetAttribute, Value);
                        }
                        else
                        {
                            d.OriginalValue = target.Attributes[TargetAttribute].Value;
                            target.Attributes[TargetAttribute].Value = Value;
                        }
                    }
                }
            }
            doc.Save(IO.IOHelper.MapPath(SystemDirectories.Masterpages) + "/" + TargetFile);

            d.TaskExecutionStatus = TaskExecutionStatus.Completed;
            d.NewValue = Value;
            //save

            return d;
        }


        public override string PreviewClientScript(string ControlClientId,string ClientSidePreviewEventType, string ClientSideGetValueScript)
        {

            if (string.IsNullOrEmpty(TargetAttribute))
            {
                return string.Format(
                    @"jQuery('#{0}').bind('{3}', function() {{ 
                        var val = '{5}'; 
                        jQuery('#{1}').html(val.replace('${{Output}}',{2})); 
                    }});
                
                    //cancel support
                     var init{4} = jQuery('#{1}').html(); 
                     jQuery('#cancelSkinCustomization').click(function () {{ 
                        jQuery('#{1}').html(init{4}); 
                    }});
                    ",
                    ControlClientId,
                    TargetID,
                    ClientSideGetValueScript,
                    ClientSidePreviewEventType,
                    Guid.NewGuid().ToString().Replace("-", ""),
                    Value);
            }
            else
            {
                return string.Format(
                   @"jQuery('#{0}').bind('{4}', function() {{ 
                        var val = '{6}'; 
                        jQuery('#{1}').attr('{2}',val.replace('${{Output}}',{3})); 
                    }});


                    //cancel support
                     var init{5} = jQuery('#{1}').attr('{2}'); 
                     jQuery('#cancelSkinCustomization').click(function () {{ 
                        jQuery('#{1}').attr('{2}',init{5}); 
                    }});
                    ",
                   ControlClientId,
                   TargetID,
                   TargetAttribute,
                    ClientSideGetValueScript,
                    ClientSidePreviewEventType,
                    Guid.NewGuid().ToString().Replace("-", ""),
                    Value);
            }
        }
    }
}
