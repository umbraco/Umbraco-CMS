using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.Configuration;
using Umbraco.Core.Profiling;
using umbraco.BusinessLogic;
using System.Xml;
using umbraco.presentation;

namespace umbraco
{
    /// <summary>
    /// Summary description for helper.
    /// </summary>
    public class helper
    {
        public static bool IsNumeric(string Number)
        {
            int result;
            return int.TryParse(Number, out result);
        }

        public static User GetCurrentUmbracoUser()
        {
            return umbraco.BasePages.UmbracoEnsuredPage.CurrentUser;
        }

		[Obsolete("This method has been superceded. Use the extension method for HttpRequest or HttpRequestBase method: GetItemAsString instead.")]
        public static string Request(string text)
		{
			if (HttpContext.Current == null)
				return string.Empty;

            if (HttpContext.Current.Request[text.ToLower()] != null)
				if (HttpContext.Current.Request[text] != string.Empty)
					return HttpContext.Current.Request[text];
            
            return String.Empty;
        }

		[Obsolete("Has been superceded by Umbraco.Core.XmlHelper.GetAttributesFromElement")]
        public static Hashtable ReturnAttributes(String tag)
		{
			var h = new Hashtable();
            foreach(var i in Umbraco.Core.XmlHelper.GetAttributesFromElement(tag))
            {
            	h.Add(i.Key, i.Value);
            }
			return h;
		}

        public static String FindAttribute(IDictionary attributes, String key)
        {
            return FindAttribute(null, attributes, key);
        }

        public static String FindAttribute(IDictionary pageElements, IDictionary attributes, String key)
        {
            // fix for issue 14862: lowercase for case insensitive matching
            key = key.ToLower();

            string attributeValue = string.Empty;
            if (attributes[key] != null)
                attributeValue = attributes[key].ToString();

            attributeValue = parseAttribute(pageElements, attributeValue);
            return attributeValue;
        }

        /// <summary>
        /// This method will parse the attribute value to look for some special syntax such as
        ///     [@requestKey]
        ///     [%sessionKey]
        ///     [#pageElement]
        ///     [$recursiveValue]
        /// </summary>
        /// <param name="pageElements"></param>
        /// <param name="attributeValue"></param>
        /// <returns></returns>
        /// <remarks>
        /// You can even apply fallback's separated by comma's like:
        /// 
        ///     [@requestKey],[%sessionKey]
        /// 
        /// </remarks>
        public static string parseAttribute(IDictionary pageElements, string attributeValue)
        {
            // Check for potential querystring/cookie variables
            // SD: not sure why we are checking for len 3 here?
            if (attributeValue.Length > 3 && attributeValue.StartsWith("["))
            {
                var attributeValueSplit = (attributeValue).Split(',');

                // before proceeding, we don't want to process anything here unless each item starts/ends with a [ ]
                // this is because the attribute value could actually just be a json array like [1,2,3] which we don't want to parse
                //
                // however, the last one can be a literal, must take care of this!
                // so here, don't check the last one, which can be just anything
                if (attributeValueSplit.Take(attributeValueSplit.Length - 1).All(x =>
                    //must end with [
                    x.EndsWith("]") &&
                    //must start with [ and a special char
                    (x.StartsWith("[@") || x.StartsWith("[%") || x.StartsWith("[#") || x.StartsWith("[$"))) == false)
                {
                    return attributeValue;
                }

                foreach (var attributeValueItem in attributeValueSplit)
                {
                    attributeValue = attributeValueItem;
                    var trimmedValue = attributeValue.Trim();

                    // Check for special variables (always in square-brackets like [name])
                    if (trimmedValue.StartsWith("[") &&
                        trimmedValue.EndsWith("]"))
                    {
                        attributeValue = trimmedValue;

                        // find key name
                        var keyName = attributeValue.Substring(2, attributeValue.Length - 3);
                        var keyType = attributeValue.Substring(1, 1);

                        switch (keyType)
                        {
                            case "@":
                                attributeValue = HttpContext.Current.Request[keyName];
                                break;
                            case "%":
                                attributeValue = StateHelper.GetSessionValue<string>(keyName);
                                if (String.IsNullOrEmpty(attributeValue))
                                    attributeValue = StateHelper.GetCookieValue(keyName);
                                break;
                            case "#":
                                if (pageElements[keyName] != null)
                                    attributeValue = pageElements[keyName].ToString();
                                else
                                    attributeValue = "";
                                break;
                            case "$":
                                if (pageElements[keyName] != null && pageElements[keyName].ToString() != string.Empty)
                                {
                                    attributeValue = pageElements[keyName].ToString();
                                }
                                else
                                {
                                    // reset attribute value in case no value has been found on parents
                                    attributeValue = String.Empty;
                                    XmlDocument umbracoXML = presentation.UmbracoContext.Current.GetXml();

                                    String[] splitpath = (String[])pageElements["splitpath"];
                                    for (int i = 0; i < splitpath.Length - 1; i++)
                                    {
                                        XmlNode element = umbracoXML.GetElementById(splitpath[splitpath.Length - i - 1].ToString());
                                        if (element == null)
                                            continue;
                                        string xpath = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? "./data [@alias = '{0}']" : "{0}";
                                        XmlNode currentNode = element.SelectSingleNode(string.Format(xpath,
                                            keyName));
                                        if (currentNode != null && currentNode.FirstChild != null &&
                                           !string.IsNullOrEmpty(currentNode.FirstChild.Value) &&
                                           !string.IsNullOrEmpty(currentNode.FirstChild.Value.Trim()))
                                        {
                                            HttpContext.Current.Trace.Write("parameter.recursive", "Item loaded from " + splitpath[splitpath.Length - i - 1]);
                                            attributeValue = currentNode.FirstChild.Value;
                                            break;
                                        }
                                    }
                                }
                                break;
                        }

                        if (attributeValue != null)
                        {
                            attributeValue = attributeValue.Trim();
                            if (attributeValue != string.Empty)
                                break;
                        }
                        else
                            attributeValue = string.Empty;
                    }
                }
            }

            return attributeValue;
        }

        [UmbracoWillObsolete("We should really obsolete that one.")]
        public static string SpaceCamelCasing(string text)
        {
            return text.SplitPascalCasing().ToFirstUpperInvariant();
        }

        [Obsolete("Use umbraco.presentation.UmbracContext.Current.GetBaseUrl()")]
        public static string GetBaseUrl(HttpContext Context)
        {
            return Context.Request.Url.GetLeftPart(UriPartial.Authority);
        }
    }
}