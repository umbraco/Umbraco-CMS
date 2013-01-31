using System;
using System.Xml;

namespace umbraco.cms.businesslogic.packager.standardPackageActions
{
	public class helper
	{
		//Helper method to replace umbraco tags that breaks the xml format..
		public static string parseToValidXml(template.Template templateObj, ref bool hasAspNetContentBeginning, string template, bool toValid)
		{
			string retVal = template;
			if (toValid)
			{
				// test for asp:content as the first part of the design
				if (retVal.StartsWith("<asp:content", StringComparison.OrdinalIgnoreCase))
				{
					hasAspNetContentBeginning = true;
					retVal = retVal.Substring(retVal.IndexOf(">") + 1);
					retVal = retVal.Substring(0, retVal.Length - 14);
				}
				//shorten empty macro tags.. 
				retVal = retVal.Replace("></umbraco:macro>", " />");
				retVal = retVal.Replace("></umbraco:Macro>", " />");

				retVal = retVal.Replace("<umbraco:", "<umbraco__");
				retVal = retVal.Replace("</umbraco:", "</umbraco__");
				retVal = retVal.Replace("<asp:", "<asp__");
				retVal = retVal.Replace("</asp:", "</asp__");

				retVal = retVal.Replace("?UMBRACO_GETITEM", "UMBRACO_GETITEM");
				retVal = retVal.Replace("?UMBRACO_TEMPLATE_LOAD_CHILD", "UMBRACO_TEMPLATE_LOAD_CHILD");
				retVal = retVal.Replace("?UMBRACO_MACRO", "UMBRACO_MACRO");
				retVal = retVal.Replace("?ASPNET_FORM", "ASPNET_FORM");
				retVal = retVal.Replace("?ASPNET_HEAD", "ASPNET_HEAD");


			}
			else
			{
				retVal = retVal.Replace("<umbraco__", "<umbraco:");
				retVal = retVal.Replace("</umbraco__", "</umbraco:");
				retVal = retVal.Replace("<asp__", "<asp:");
				retVal = retVal.Replace("</asp__", "</asp:");
				retVal = retVal.Replace("UMBRACO_GETITEM", "?UMBRACO_GETITEM");
				retVal = retVal.Replace("UMBRACO_TEMPLATE_LOAD_CHILD", "?UMBRACO_TEMPLATE_LOAD_CHILD");
				retVal = retVal.Replace("UMBRACO_MACRO", "?UMBRACO_MACRO");
				retVal = retVal.Replace("ASPNET_FORM", "?ASPNET_FORM");
				retVal = retVal.Replace("ASPNET_HEAD", "?ASPNET_HEAD");
				retVal = retVal.Replace("<root>", "");
				retVal = retVal.Replace("<root xmlns:asp=\"http://microsoft.com\">", "");
				retVal = retVal.Replace("</root>", "");

				// add asp content element
				if (hasAspNetContentBeginning)
				{
					retVal = templateObj.GetMasterContentElement(templateObj.MasterTemplate) + retVal + "</asp:content>";
				}
			}

			return retVal;
		}

		public static XmlNode parseStringToXmlNode(string value)
		{
			XmlDocument doc = new XmlDocument();
			XmlNode node = xmlHelper.addTextNode(doc, "error", "");

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