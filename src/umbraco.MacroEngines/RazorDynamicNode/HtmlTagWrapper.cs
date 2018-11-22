using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.IO;
using System.Web;

namespace umbraco.MacroEngines
{

	[Obsolete("This class has been superceded by Umbraco.Web.Mvc.HtmlTagWrapper")]	
    public class HtmlTagWrapper : Umbraco.Web.Mvc.HtmlTagWrapper
    {

        public HtmlTagWrapper(string tag)
			: base(tag)
        {
            
        }       
    }

}
