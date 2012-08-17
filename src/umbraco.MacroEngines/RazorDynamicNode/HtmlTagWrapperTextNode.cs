using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.MacroEngines
{

	[Obsolete("This interface has been superceded by Umbraco.Core.HtmlTagWrapperTextNode")]	
    public class HtmlTagWrapperTextNode : HtmlTagWrapperBase
    {
        public string content;
        public HtmlTagWrapperTextNode(string content)
        {
            this.content = content;
        }

        public void WriteToHtmlTextWriter(System.Web.UI.HtmlTextWriter html)
        {
            html.Write(content);
        }
    }
}
