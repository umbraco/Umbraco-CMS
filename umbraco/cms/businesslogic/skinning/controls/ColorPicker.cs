using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using ClientDependency.Core;
using System.Web.UI;

namespace umbraco.cms.businesslogic.skinning.controls
{
    //[ClientDependency(ClientDependencyType.Javascript, "colorpicker/js/colorpicker.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Css, "colorpicker/css/colorpicker.css", "UmbracoClient")]
    public class ColorPicker: TextBox
    {


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ScriptManager.RegisterClientScriptInclude(this, this.GetType(), "colorpicker.js", 
                this.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco_client + "/colorpicker/js/colorpicker.js"));


            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "cp" + this.ClientID,
               string.Format(@"$(document).ready(function() {{
                            $('#{0}').ColorPicker({{
            	                onSubmit: function(hsb, hex, rgb, el) {{
            		                $(el).val('#' + hex);
            		                $(el).ColorPickerHide();
                                    jQuery(el).trigger('change');
            	                }},
            	                onBeforeShow: function () {{
            		                $(this).ColorPickerSetColor(this.value);
            	                }}
                            }})
                            .bind('keyup', function(){{
            	                $(this).ColorPickerSetColor(this.value);
                            }});}});", this.ClientID), true);
        }
    }
}
