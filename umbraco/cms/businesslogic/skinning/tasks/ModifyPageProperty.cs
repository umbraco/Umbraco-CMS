using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.interfaces.skinning;
using umbraco.cms.businesslogic.web;
using System.Web;

namespace umbraco.cms.businesslogic.skinning.tasks
{
    public class ModifyPageProperty : TaskType
    {
        public string PropertyAlias { get; set; }
        public string ClientSideContainerID { get; set; }

        public ModifyPageProperty()
        {
            this.Name = " Modify page property";
            this.Description = "Will modify a property on the current page";
        }

        public override TaskExecutionDetails Execute(string Value)
        {
            TaskExecutionDetails d = new TaskExecutionDetails();

            string id = HttpContext.Current.Items["pageID"].ToString();

            Document doc = new Document(Convert.ToInt32(id));

            if (doc.getProperty(PropertyAlias) != null)
            {
                d.OriginalValue = doc.getProperty(PropertyAlias).Value.ToString();

                doc.getProperty(PropertyAlias).Value = Value;
                doc.Publish(new BusinessLogic.User(0));

                d.NewValue = Value;
                d.TaskExecutionStatus = TaskExecutionStatus.Completed;
            }
            else
                d.TaskExecutionStatus = TaskExecutionStatus.Cancelled;

            return d;
        }


        public override string PreviewClientScript(string ControlClientId, string ClientSidePreviewEventType, string ClientSideGetValueScript)
        {

           
            return string.Format(
                @"jQuery('#{0}').bind('{3}', function() {{ 
                    jQuery('#{1}').html({2}); 
                }});
                
                //cancel support
                    var init{4} = jQuery('#{1}').html(); 
                    jQuery('#cancelSkinCustomization').click(function () {{ 
                    jQuery('#{1}').html(init{4}); 
                }});
                ",
                ControlClientId,
                ClientSideContainerID,
                ClientSideGetValueScript,
                ClientSidePreviewEventType,
                Guid.NewGuid().ToString().Replace("-", ""));

            
        }
    }
}
