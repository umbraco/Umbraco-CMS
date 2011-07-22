using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace umbraco.MacroEngines
{
    public class InternalHtmlString : HtmlString
    {
        public InternalHtmlString(string s) : base(s) { }
        public static implicit operator string(InternalHtmlString s)
        {
            return s.ToHtmlString();
        }
    }
}
