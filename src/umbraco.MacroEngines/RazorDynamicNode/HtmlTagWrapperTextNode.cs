using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.MacroEngines
{

	[Obsolete("This interface has been superceded by Umbraco.Web.Mvc.HtmlTagWrapperTextNode")]	
    public class HtmlTagWrapperTextNode : Umbraco.Web.Mvc.HtmlTagWrapperTextNode
    {
        
        public HtmlTagWrapperTextNode(string content)
			: base(content)
        {
            
        }

    }
}
