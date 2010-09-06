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
    }
}
