﻿using System;
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

        public void AppendTo(WebControl c)
        {
            foreach (var keyval in this)
                c.Attributes.Add("data-" + keyval.Key, keyval.Value);
        }
    }
}
