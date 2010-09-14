using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using ClientDependency.Core;
using System.Web.UI;
using ClientDependency.Core.Controls;

namespace umbraco.cms.businesslogic.skinning.controls
{
    [ClientDependency(450,ClientDependencyType.Javascript, "colorpicker/js/colorpicker.js", "UmbracoClient")]
    [ClientDependency(460, ClientDependencyType.Javascript, "colorpicker/js/initcolorpicker.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Css, "colorpicker/css/colorpicker.css", "UmbracoClient")]
    public class ColorPicker: TextBox
    {
        
    }
}
