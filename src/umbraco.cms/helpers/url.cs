using System;
using System.Xml;
using System.Text.RegularExpressions;

namespace umbraco.cms.helpers
{
	/// <summary>
	/// Summary description for url.
	/// </summary>
	public class url
	{
		public url()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public static string FormatUrl(string url) 
		{
			string _newUrl = url;
			XmlNode replaceChars = UmbracoSettings.UrlReplaceCharacters;
			foreach (XmlNode n in replaceChars.SelectNodes("char")) 
			{
				if (n.Attributes.GetNamedItem("org") != null && n.Attributes.GetNamedItem("org").Value != "")
					_newUrl = _newUrl.Replace(n.Attributes.GetNamedItem("org").Value,xmlHelper.GetNodeValue(n)); 
			}

            // check for double dashes
            if (UmbracoSettings.RemoveDoubleDashesFromUrlReplacing)
            {
                _newUrl = Regex.Replace(_newUrl, @"[-]{2,}", "-");
            }

			return _newUrl;
		}

	}
}
