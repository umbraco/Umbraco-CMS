using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using umbraco.interfaces.skinning;

namespace umbraco.cms.businesslogic.skinning
{
    public abstract class DependencyType : ProviderBase, IDependencyType
    {
        public virtual WebControl Editor { get; set; }
        public virtual List<Object> Values { get; set; }

        public virtual string CssVariablePreviewClientScript(string ControlClientId, string VariableClientId)
        {
            return string.Format(
               @"jQuery('#{0}').bind('{2}', function() {{ 
                        {3} = {1}; 
                        PreviewCssVariables();
                }});


                //cancel support (not implemented yet)
                jQuery('#cancelSkinCustomization').click(function () {{ 

                }});

                ",
               ControlClientId,
               this.ClientSideGetValueScript(),
               this.ClientSideCssVariablePreviewEventType(),
               VariableClientId);
        }

        public virtual string ClientSideGetValueScript()
        {            
            return string.Format(
                "jQuery('#{0}').val()"
                ,Editor.ClientID);
        }
        public virtual string ClientSidePreviewEventType()
        {
            return "change";
        }

        public virtual string ClientSideCssVariablePreviewEventType()
        {
            //should be something that doesn't execute at every change
            return "change";
        }
    }
}
