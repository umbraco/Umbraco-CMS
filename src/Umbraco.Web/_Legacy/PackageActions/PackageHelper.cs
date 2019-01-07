using System.Xml;
using Umbraco.Core.Models;
using Umbraco.Core.Xml;

namespace Umbraco.Web._Legacy.PackageActions
{
    public class PackageHelper
    {
        //Helper method to replace umbraco tags that breaks the xml format..
        public static string ParseToValidXml(ITemplate templateObj, string template, bool toValid)
        {
            string retVal = template;

            if (toValid)
            {
                retVal = retVal.Replace("?UMBRACO_MACRO", "UMBRACO_MACRO");
            }
            else
            {
                retVal = retVal.Replace("UMBRACO_MACRO", "?UMBRACO_MACRO");
                retVal = retVal.Replace("<root>", "");
                retVal = retVal.Replace("<root xmlns:asp=\"http://microsoft.com\">", "");
                retVal = retVal.Replace("</root>", "");
            }

            return retVal;
        }

        public static XmlNode ParseStringToXmlNode(string value)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode node = XmlHelper.AddTextNode(doc, "error", "");

            try
            {
                doc.LoadXml(value);
                return doc.SelectSingleNode(".");
            }
            catch
            {
                return node;
            }

        }
    }
}
