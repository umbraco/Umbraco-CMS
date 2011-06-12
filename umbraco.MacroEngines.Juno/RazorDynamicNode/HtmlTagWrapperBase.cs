using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace umbraco.MacroEngines
{
    public interface HtmlTagWrapperBase
    {
        void WriteToHtmlTextWriter(HtmlTextWriter html);
    }
}
