using System;
using System.Collections;
using System.Web;
using System.Xml;

namespace umbraco
{
	/// <summary>
	/// 
	/// </summary>
	public class item
	{
		private String _fieldContent = "";
		private String _fieldName;

		public String FieldContent
		{
			get { return _fieldContent; }
		}

		public item(string itemValue, IDictionary attributes)
		{
			_fieldContent = itemValue;
			parseItem(attributes);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="umbPage"></param>
		/// <param name="attributes"></param>
		/// 
        public item(IDictionary elements, IDictionary attributes)
		{
			_fieldName = helper.FindAttribute(attributes, "field");

			if(_fieldName.StartsWith("#"))
			{
				_fieldContent = library.GetDictionaryItem(_fieldName.Substring(1, _fieldName.Length - 1));
			}
			else
			{
				// Loop through XML children we need to find the fields recursive
				if(helper.FindAttribute(attributes, "recursive") == "true")
				{

					XmlDocument umbracoXML = presentation.UmbracoContext.Current.GetXml();

					String[] splitpath = (String[]) elements["splitpath"];
					for(int i = 0; i < splitpath.Length - 1; i++)
					{
						XmlNode element = umbracoXML.GetElementById(splitpath[splitpath.Length - i - 1].ToString());
						if (element == null)
							continue;
					    string xpath = UmbracoSettings.UseLegacyXmlSchema ? "./data [@alias = '{0}']" : "./{0}";
						XmlNode currentNode = element.SelectSingleNode(string.Format(xpath,
							_fieldName));
						if(currentNode != null && currentNode.FirstChild != null &&
						   !string.IsNullOrEmpty(currentNode.FirstChild.Value) &&
						   !string.IsNullOrEmpty(currentNode.FirstChild.Value.Trim()))
						{
							HttpContext.Current.Trace.Write("item.recursive", "Item loaded from " + splitpath[splitpath.Length - i - 1]);
							_fieldContent = currentNode.FirstChild.Value;
							break;
						}
					}
				}
				else
				{
                    if (elements[_fieldName] != null && !string.IsNullOrEmpty(elements[_fieldName].ToString()))
                        _fieldContent = elements[_fieldName].ToString().Trim();
					else if(!string.IsNullOrEmpty(helper.FindAttribute(attributes, "useIfEmpty")))
                        if (elements[helper.FindAttribute(attributes, "useIfEmpty")] != null && !string.IsNullOrEmpty(elements[helper.FindAttribute(attributes, "useIfEmpty")].ToString()))
                            _fieldContent = elements[helper.FindAttribute(attributes, "useIfEmpty")].ToString().Trim();
				}
			}

			parseItem(attributes);
		}

        private void parseItem(IDictionary attributes)
		{
			HttpContext.Current.Trace.Write("item", "Start parsing '" + _fieldName + "'");
			if(helper.FindAttribute(attributes, "textIfEmpty") != "" && _fieldContent == "")
				_fieldContent = helper.FindAttribute(attributes, "textIfEmpty");

			_fieldContent = _fieldContent.Trim();

			// DATE FORMATTING FUNCTIONS
			if(helper.FindAttribute(attributes, "formatAsDateWithTime") == "true")
			{
				if(_fieldContent == "")
					_fieldContent = DateTime.Now.ToString();
				_fieldContent = Convert.ToDateTime(_fieldContent).ToLongDateString() +
				                helper.FindAttribute(attributes, "formatAsDateWithTimeSeparator") +
				                Convert.ToDateTime(_fieldContent).ToShortTimeString();
			}
			else if(helper.FindAttribute(attributes, "formatAsDate") == "true")
			{
				if(_fieldContent == "")
					_fieldContent = DateTime.Now.ToString();
				_fieldContent = Convert.ToDateTime(_fieldContent).ToLongDateString();
			}


			// TODO: Needs revision to check if parameter-tags has attributes
			if(helper.FindAttribute(attributes, "stripParagraph") == "true" && _fieldContent.Length > 5)
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
            if (helper.FindAttribute(attributes, "case") == "lower")
                _fieldContent = _fieldContent.ToLower();
            else if (helper.FindAttribute(attributes, "case") == "upper")
                _fieldContent = _fieldContent.ToUpper();

			// OTHER FORMATTING FUNCTIONS
            // If we use masterpages, this is moved to the ItemRenderer to add support for before/after in inline XSLT
            if (!UmbracoSettings.UseAspNetMasterPages)
            {
                if (_fieldContent != "" && helper.FindAttribute(attributes, "insertTextBefore") != "")
                    _fieldContent = HttpContext.Current.Server.HtmlDecode(helper.FindAttribute(attributes, "insertTextBefore")) +
                                    _fieldContent;
                if (_fieldContent != "" && helper.FindAttribute(attributes, "insertTextAfter") != "")
                    _fieldContent += HttpContext.Current.Server.HtmlDecode(helper.FindAttribute(attributes, "insertTextAfter"));
            }
			if(helper.FindAttribute(attributes, "urlEncode") == "true")
                _fieldContent = HttpUtility.UrlEncode(_fieldContent);
            if (helper.FindAttribute(attributes, "htmlEncode") == "true")
                _fieldContent = HttpUtility.HtmlEncode(_fieldContent);
			if(helper.FindAttribute(attributes, "convertLineBreaks") == "true")
				_fieldContent = _fieldContent.Replace("\n", "<br/>\n");

			HttpContext.Current.Trace.Write("item", "Done parsing '" + _fieldName + "'");
		}
	}
}