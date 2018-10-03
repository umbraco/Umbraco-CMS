using System;
using System.Collections;
using System.Web;
using System.Xml;
using StackExchange.Profiling;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Core.Profiling;
using Umbraco.Core.Strings;
using Umbraco.Web.Composing;
using Umbraco.Web.Macros;

namespace umbraco
{
    /// <summary>
    ///
    /// </summary>
    public class item
    {
        private string _fieldContent = "";
        private readonly string _fieldName;

        public string FieldContent
        {
            get { return _fieldContent; }
        }

        public item(string itemValue, IDictionary attributes)
        {
            _fieldContent = itemValue;
            ParseItem(attributes);
        }

        /// <summary>
        /// Creates a new Legacy item
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="attributes"></param>
        public item(IDictionary elements, IDictionary attributes)
            : this(null, elements, attributes)
        {
        }

        /// <summary>
        /// Creates an Item with a publishedContent item in order to properly recurse and return the value.
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <param name="elements"></param>
        /// <param name="attributes"></param>
        /// <remarks>
        /// THIS ENTIRE CLASS WILL BECOME LEGACY, THE FIELD RENDERING NEEDS TO BE REPLACES SO THAT IS WHY THIS
        /// CTOR IS INTERNAL.
        /// </remarks>
        internal item(IPublishedContent publishedContent, IDictionary elements, IDictionary attributes)
        {
            _fieldName = FindAttribute(attributes, "field");

            if (_fieldName.StartsWith("#"))
            {
                var umbHelper = new UmbracoHelper(Current.UmbracoContext, Current.Services, Current.ApplicationCache);

                _fieldContent = umbHelper.GetDictionaryValue(_fieldName.Substring(1, _fieldName.Length - 1));
            }
            else
            {
                // Loop through XML children we need to find the fields recursive
                var recursive = FindAttribute(attributes, "recursive") == "true";

                if (publishedContent == null)
                {
                    if (recursive)
                    {
                        var recursiveVal = GetRecursiveValueLegacy(elements);
                        _fieldContent = recursiveVal.IsNullOrWhiteSpace() ? _fieldContent : recursiveVal;
                    }
                   
                }

                //check for published content and get its value using that
                if (publishedContent != null && (publishedContent.HasProperty(_fieldName) || recursive))
                {
                    var pval = publishedContent.Value(_fieldName, fallback: Fallback.ToAncestors);
                    var rval = pval == null ? string.Empty : pval.ToString();
                    _fieldContent = rval.IsNullOrWhiteSpace() ? _fieldContent : rval;
                }
                else
                {
                    //get the vaue the legacy way (this will not parse locallinks, etc... since that is handled with ipublishedcontent)
                    var elt = elements[_fieldName];
                    if (elt != null && string.IsNullOrEmpty(elt.ToString()) == false)
                        _fieldContent = elt.ToString().Trim();
                }

                //now we check if the value is still empty and if so we'll check useIfEmpty
                if (string.IsNullOrEmpty(_fieldContent))
                {
                    var altFieldName = FindAttribute(attributes, "useIfEmpty");
                    if (string.IsNullOrEmpty(altFieldName) == false)
                    {
                        if (publishedContent != null && (publishedContent.HasProperty(altFieldName) || recursive))
                        {
                            var pval = publishedContent.Value(altFieldName, fallback: Fallback.ToAncestors);
                            var rval = pval == null ? string.Empty : pval.ToString();
                            _fieldContent = rval.IsNullOrWhiteSpace() ? _fieldContent : rval;
                        }
                        else
                        {
                            //get the vaue the legacy way (this will not parse locallinks, etc... since that is handled with ipublishedcontent)
                            var elt = elements[altFieldName];
                            if (elt != null && string.IsNullOrEmpty(elt.ToString()) == false)
                                _fieldContent = elt.ToString().Trim();
                        }
                    }
                }

            }

            ParseItem(attributes);
        }

        static string FindAttribute(IDictionary attributes, string key)
        {
            key = key.ToLowerInvariant();
            var attributeValue = attributes.Contains(key) ? attributes[key].ToString() : string.Empty;
            return MacroRenderer.ParseAttribute(null, attributeValue);
        }

        /// <summary>
        /// Returns the recursive value using a legacy strategy of looking at the xml cache and the splitPath in the elements collection
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        private string GetRecursiveValueLegacy(IDictionary elements)
        {
            using (Current.ProfilingLogger.DebugDuration<item>("Checking recusively"))
            {
                var content = "";

                var umbracoContext = UmbracoContext.Current;
                var cache = umbracoContext.ContentCache as Umbraco.Web.PublishedCache.XmlPublishedCache.PublishedContentCache;
                if (cache == null)
                    throw new InvalidOperationException("Unsupported IPublishedContentCache, only the Xml one is supported.");
                var umbracoXml = cache.GetXml(umbracoContext.InPreviewMode);

                var splitpath = (string[])elements["splitpath"];
                for (int i = 0; i < splitpath.Length - 1; i++)
                {
                    XmlNode element = umbracoXml.GetElementById(splitpath[splitpath.Length - i - 1]);

                    if (element == null)
                        continue;

                    var xpath = "./{0}";
                    var currentNode = element.SelectSingleNode(string.Format(xpath, _fieldName));

                    //continue if all is null
                    if (currentNode == null || currentNode.FirstChild == null || string.IsNullOrEmpty(currentNode.FirstChild.Value) || string.IsNullOrEmpty(currentNode.FirstChild.Value.Trim()))
                        continue;

                    HttpContext.Current.Trace.Write("item.recursive", "Item loaded from " + splitpath[splitpath.Length - i - 1]);
                    content = currentNode.FirstChild.Value;
                    break;
                }

                return content;
            }
        }

        private void ParseItem(IDictionary attributes)
        {
            using (Current.ProfilingLogger.DebugDuration<item>("Start parsing " + _fieldName))
            {
                HttpContext.Current.Trace.Write("item", "Start parsing '" + _fieldName + "'");
                if (FindAttribute(attributes, "textIfEmpty") != "" && _fieldContent == "")
                    _fieldContent = FindAttribute(attributes, "textIfEmpty");

                _fieldContent = _fieldContent.Trim();

                // DATE FORMATTING FUNCTIONS
                if (FindAttribute(attributes, "formatAsDateWithTime") == "true")
                {
                    if (_fieldContent == "")
                        _fieldContent = DateTime.Now.ToString();
                    _fieldContent = Convert.ToDateTime(_fieldContent).ToLongDateString() +
                                    FindAttribute(attributes, "formatAsDateWithTimeSeparator") +
                                    Convert.ToDateTime(_fieldContent).ToShortTimeString();
                }
                else if (FindAttribute(attributes, "formatAsDate") == "true")
                {
                    if (_fieldContent == "")
                        _fieldContent = DateTime.Now.ToString();
                    _fieldContent = Convert.ToDateTime(_fieldContent).ToLongDateString();
                }


                // TODO: Needs revision to check if parameter-tags has attributes
                if (FindAttribute(attributes, "stripParagraph") == "true" && _fieldContent.Length > 5)
                {
                    _fieldContent = _fieldContent.Trim();
                    string fieldContentLower = _fieldContent.ToLower();

                    // the field starts with an opening p tag
                    if (fieldContentLower.Substring(0, 3) == "<p>"
                        // it ends with a closing p tag
                        && fieldContentLower.Substring(_fieldContent.Length - 4, 4) == "</p>"
                        // it doesn't contain multiple p-tags
                        && fieldContentLower.IndexOf("<p>", 1) < 0)
                    {
                        _fieldContent = _fieldContent.Substring(3, _fieldContent.Length - 7);
                    }
                }

                // CASING
                if (FindAttribute(attributes, "case") == "lower")
                    _fieldContent = _fieldContent.ToLower();
                else if (FindAttribute(attributes, "case") == "upper")
                    _fieldContent = _fieldContent.ToUpper();
                else if (FindAttribute(attributes, "case") == "title")
                    _fieldContent = _fieldContent.ToCleanString(CleanStringType.Ascii | CleanStringType.Alias | CleanStringType.PascalCase);

                // OTHER FORMATTING FUNCTIONS
                
                if (FindAttribute(attributes, "urlEncode") == "true")
                    _fieldContent = HttpUtility.UrlEncode(_fieldContent);
                if (FindAttribute(attributes, "htmlEncode") == "true")
                    _fieldContent = HttpUtility.HtmlEncode(_fieldContent);
                if (FindAttribute(attributes, "convertLineBreaks") == "true")
                    _fieldContent = _fieldContent.Replace("\n", "<br/>\n");

                HttpContext.Current.Trace.Write("item", "Done parsing '" + _fieldName + "'");
            }
        }
    }
}
