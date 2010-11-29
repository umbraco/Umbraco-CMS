using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace umbraco.cms.businesslogic.skinning
{
    public class CssVariable
    {
        public string Name { get; set; }
        public string DefaultValue { get; set; }
        public List<CssVariableProperty> Properties { get; set; }

      

        public static CssVariable CreateFromXmlNode(XmlNode node)
        {
            CssVariable var = new CssVariable();

            if (node.Attributes["name"] != null)
                var.Name = node.Attributes["name"].Value;

            var.DefaultValue = node.InnerText;

            return var;
        }
    }
}
