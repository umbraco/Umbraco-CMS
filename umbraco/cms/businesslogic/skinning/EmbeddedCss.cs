using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace umbraco.cms.businesslogic.skinning
{
    public class EmbeddedCss
    {
        public string TargetFile { get; set; }
        public List<CssVariable> Variables { get; set; }
        public string Content { get; set; }


        public EmbeddedCss()
        {
            Variables = new List<CssVariable>();
        }


        public static EmbeddedCss CreateFromXmlNode(XmlNode node)
        {
            EmbeddedCss css = new EmbeddedCss();

            if (node.Attributes["targetfile"] != null)
                css.TargetFile = node.Attributes["targetfile"].Value;

            if (node.SelectSingleNode("Content") != null)
                css.Content = node.SelectSingleNode("Content").InnerText;


            foreach (XmlNode variableNode in node.SelectNodes("Variables/Variable"))
            {
                
               css.Variables.Add(CssVariable.CreateFromXmlNode(variableNode));

            }

            return css;

        }
    }
}
