using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Xml;
using Umbraco.Core.Macros;
using Umbraco.Web.umbraco.templateControls;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;
using umbraco.IO;

namespace umbraco.presentation.templateControls
{
	public class ItemRenderer
	{
		public readonly static ItemRenderer Instance = new ItemRenderer();
		/// <summary>
		/// Initializes a new instance of the <see cref="ItemRenderer"/> class.
		/// </summary>
		protected ItemRenderer()
		{ }

		/// <summary>
		/// Renders the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="writer">The writer.</param>
		public virtual void Render(Item item, HtmlTextWriter writer)
		{
			if (item.DebugMode)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Title, string.Format("Field Tag: '{0}'", item.Field));
				writer.AddAttribute("style", "border: 1px solid #fc6;");
				writer.RenderBeginTag(HtmlTextWriterTag.Div);
			}

			try
			{
				StringWriter renderOutputWriter = new StringWriter();
				HtmlTextWriter htmlWriter = new HtmlTextWriter(renderOutputWriter);
				foreach (Control control in item.Controls)
				{
					try
					{
						control.RenderControl(htmlWriter);
					}
					catch (Exception renderException)
					{
						// TODO: Validate that the current control is within the scope of a form control
						// Even controls that are inside this scope, can produce this error in async postback.
						HttpContext.Current.Trace.Warn("ItemRenderer",
							String.Format("Error rendering control {0} of {1}.", control.ClientID, item), renderException);
					}
				}

				// parse macros and execute the XSLT transformation on the result if not empty
				string renderOutput = renderOutputWriter.ToString();
				string xsltTransformedOutput = renderOutput.Trim().Length == 0
											   ? String.Empty
											   : XsltTransform(item.Xslt, renderOutput, item.XsltDisableEscaping);
				// handle text before/after
				xsltTransformedOutput = AddBeforeAfterText(xsltTransformedOutput, helper.FindAttribute(item.LegacyAttributes, "insertTextBefore"), helper.FindAttribute(item.LegacyAttributes, "insertTextAfter"));
				string finalResult = xsltTransformedOutput.Trim().Length > 0 ? xsltTransformedOutput : GetEmptyText(item);
				writer.Write(IOHelper.ResolveUrlsFromTextString(finalResult));
			}
			catch (Exception renderException)
			{
				HttpContext.Current.Trace.Warn("ItemRenderer", String.Format("Error rendering {0}.", item), renderException);
			}
			finally
			{
				if (item.DebugMode)
				{
					writer.RenderEndTag();
				}
			}
		}

		/// <summary>
		/// Renders the field contents.
		/// Checks via the NodeId attribute whether to fetch data from another page than the current one.
		/// </summary>
		/// <returns>A string of field contents (macros not parsed)</returns>
		protected virtual string GetFieldContents(Item item)
		{
			string tempElementContent = String.Empty;

			// if a nodeId is specified we should get the data from another page than the current one
			if (!String.IsNullOrEmpty(item.NodeId))
			{
				int? tempNodeId = item.GetParsedNodeId();
				if (tempNodeId != null && tempNodeId.Value != 0)
				{
					string currentField = helper.FindAttribute(item.LegacyAttributes, "field");
					// check for a cached instance of the content
					object contents = GetContentFromCache(tempNodeId.Value, currentField);
					if (contents != null)
						tempElementContent = (string)contents;
					else
					{
						// as the field can be used for both documents, media and even members we'll use the 
						// content class to lookup field items
						try
						{
							tempElementContent = GetContentFromDatabase(item.Attributes, tempNodeId.Value, currentField);
						}
						catch
						{
							// content was not found in property fields,
							// so the last place to look for is page fields
							page itemPage = new page(content.Instance.XmlContent.GetElementById(tempNodeId.ToString()));
							tempElementContent = new item(itemPage.Elements, item.LegacyAttributes).FieldContent;
						}
					}
				}

			}
			else
			{
				// gets the field content from the current page (via the PageElements collection)
				tempElementContent = new item(item.PageElements, item.LegacyAttributes).FieldContent;
			}

			return tempElementContent;
		}

		/// <summary>
		/// Inits the specified item. To be called from the OnInit method of Item.
		/// </summary>
		/// <param name="item">The item.</param>
		public virtual void Init(Item item)
		{
			ParseMacros(item);
		}

		/// <summary>
		/// Loads the specified item. To be called from the OnLoad method of Item.
		/// </summary>
		/// <param name="item">The item.</param>
		public virtual void Load(Item item)
		{ }

		/// <summary>
		/// Parses the macros inside the text, by creating child elements for each item.
		/// </summary>
		/// <param name="item">The item.</param>
		protected virtual void ParseMacros(Item item)
		{
			// do nothing if the macros have already been rendered
			if (item.Controls.Count > 0)
				return;

			string elementText = GetFieldContents(item);

			MacroTagParser.ParseMacros(
				elementText,

				//callback for when a text block is parsed
				textBlock => item.Controls.Add(new LiteralControl(textBlock)),

				//callback for when a macro is parsed:
				(macroAlias, attributes) =>
					{
						var macroControl = new Macro
							{
								Alias = macroAlias
							};
						foreach (var i in attributes.Where(i => macroControl.Attributes[i.Key] == null))
						{
							macroControl.Attributes.Add(i.Key, i.Value);
						}
						item.Controls.Add(macroControl);
					});
		}

		/// <summary>
		/// Transforms the content using the XSLT attribute, if provided.
		/// </summary>
		/// <param name="xpath">The xpath expression.</param>
		/// <param name="itemData">The item's rendered content.</param>
		/// <param name="disableEscaping">if set to <c>true</c>, escaping is disabled.</param>
		/// <returns>The transformed content if the XSLT attribute is present, otherwise the original content.</returns>
		protected virtual string XsltTransform(string xpath, string itemData, bool disableEscaping)
		{
			if (!String.IsNullOrEmpty(xpath))
			{
				// XML-encode the expression and add the itemData parameter to it
				string xpathEscaped = xpath.Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
				string xpathExpression = string.Format(xpathEscaped, "$itemData");

				// prepare support for XSLT extensions
				StringBuilder namespaceList = new StringBuilder();
				StringBuilder namespaceDeclaractions = new StringBuilder();
				foreach (KeyValuePair<string, object> extension in macro.GetXsltExtensions())
				{
					namespaceList.Append(extension.Key).Append(' ');
					namespaceDeclaractions.AppendFormat("xmlns:{0}=\"urn:{0}\" ", extension.Key);
				}

				// add the XSLT expression into the full XSLT document, together with the needed parameters
				string xslt = string.Format(Resources.InlineXslt, xpathExpression, disableEscaping ? "yes" : "no",
																  namespaceList, namespaceDeclaractions);

				// create the parameter
				Dictionary<string, object> parameters = new Dictionary<string, object>(1);
				parameters.Add("itemData", itemData);

				// apply the XSLT transformation
				XmlTextReader xslReader = new XmlTextReader(new StringReader(xslt));
				System.Xml.Xsl.XslCompiledTransform xsl = macro.CreateXsltTransform(xslReader, false);
				itemData = macro.GetXsltTransformResult(new XmlDocument(), xsl, parameters);
				xslReader.Close();
			}
			return itemData;
		}

		protected string AddBeforeAfterText(string text, string before, string after)
		{
			if (!String.IsNullOrEmpty(text))
			{
				if (!String.IsNullOrEmpty(before))
					text = String.Format("{0}{1}", HttpContext.Current.Server.HtmlDecode(before), text);
				if (!String.IsNullOrEmpty(after))
					text = String.Format("{0}{1}", text, HttpContext.Current.Server.HtmlDecode(after));
			}

			return text;
		}

		/// <summary>
		/// Gets the text to display if the field contents are empty.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns>The text to display.</returns>
		protected virtual string GetEmptyText(Item item)
		{
			return item.TextIfEmpty;
		}

		/// <summary>
		/// Gets the field content from database instead of the published XML via the APIs.
		/// </summary>
		/// <param name="nodeIdInt">The node id.</param>
		/// <param name="currentField">The field that should be fetched.</param>
		/// <returns>The contents of the <paramref name="currentField"/> from the <paramref name="nodeIdInt"/> content object</returns>
		protected virtual string GetContentFromDatabase(AttributeCollection itemAttributes, int nodeIdInt, string currentField)
		{
			Content c = new Content(nodeIdInt);

			Property property = c.getProperty(currentField);
			if (property == null)
				throw new ArgumentException(String.Format("Could not find property {0} of node {1}.", currentField, nodeIdInt));

			item umbItem = new item(property.Value.ToString(), new AttributeCollectionAdapter(itemAttributes));
			string tempElementContent = umbItem.FieldContent;

			// If the current content object is a document object, we'll only output it if it's published
			if (c.nodeObjectType == cms.businesslogic.web.Document._objectType)
			{
				try
				{
					Document d = (Document)c;
					if (!d.Published)
						tempElementContent = "";
				}
				catch { }
			}

			// Add the content to the cache
			if (!String.IsNullOrEmpty(tempElementContent))
				HttpContext.Current.Cache.Insert(String.Format("contentItem{0}_{1}", nodeIdInt.ToString(), currentField), tempElementContent);
			return tempElementContent;
		}

		/// <summary>
		/// Gets the content from cache.
		/// </summary>
		/// <param name="nodeIdInt">The node id.</param>
		/// <param name="field">The field.</param>
		/// <returns>The cached contents of the <paramref name="currentField"/> from the <paramref name="nodeIdInt"/> content object</returns>
		protected virtual object GetContentFromCache(int nodeIdInt, string field)
		{
			object content = HttpContext.Current.Cache[String.Format("contentItem{0}_{1}", nodeIdInt.ToString(), field.ToString())];
			return content;
		}
	}
}
