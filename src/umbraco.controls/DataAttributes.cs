using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace umbraco.uicontrols
{
    internal class DataAttributes : Dictionary<string,string>
    {
        public DataAttributes()
        {
            
        }

        public string Render()
        {
            var sb = new StringBuilder();
            foreach(var keyval in this)
            {
                sb.Append("data-" + keyval.Key + "='" + keyval.Value + "' ");
            }

            return sb.ToString().Trim();
        }

        public void AppendTo(WebControl c)
        {
            foreach (var keyval in this)
                c.Attributes.Add("data-" + keyval.Key, keyval.Value);
        }
    }
}
