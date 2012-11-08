using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web;

namespace umbraco.MacroEngines
{

	[Obsolete("This interface has been superceded by Umbraco.Web.Mvc.IHtmlTagWrapper")]	
    public interface HtmlTagWrapperBase : Umbraco.Web.Mvc.IHtmlTagWrapper
    {
    }
}
