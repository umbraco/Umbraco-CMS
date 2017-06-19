using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace Umbraco.Web._Legacy.Controls
{
    internal class DataAttributes : Dictionary<string,string>
    {

        public void AppendTo(WebControl c)
        {
            foreach (var keyval in this)
                c.Attributes.Add("data-" + keyval.Key, keyval.Value);
        }
    }
}
