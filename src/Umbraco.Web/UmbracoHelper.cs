using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Xml.Linq;
using System.Xml.XPath;
using HtmlAgilityPack;
using Umbraco.Core;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;
using umbraco;
using System.Collections.Generic;
using umbraco.presentation.templateControls;
using HtmlTagWrapper = Umbraco.Web.Mvc.HtmlTagWrapper;

namespace Umbraco.Web
{
	
	/// <summary>
	/// A helper class that provides many useful methods and functionality for using Umbraco in templates
	/// </summary>
	public class UmbracoHelper
	{
		private readonly UmbracoContext _umbracoContext;
		private readonly IDocument _currentPage;

		internal UmbracoHelper(UmbracoContext umbracoContext)
		{
			if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
			_umbracoContext = umbracoContext;
			_currentPage = _umbracoContext.DocumentRequest.Document;
		}


		#region RenderMacro

		/// <summary>
		/// Renders the macro with the specified alias.
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <returns></returns>
		public IHtmlString RenderMacro(string alias)
		{
			return RenderMacro(alias, new { });
		}

		/// <summary>
		/// Renders the macro with the specified alias, passing in the specified parameters.
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <param name="parameters">The parameters.</param>
		/// <returns></returns>
		public IHtmlString RenderMacro(string alias, object parameters)
		{
			return RenderMacro(alias, parameters.ToDictionary<object>());
		}

		/// <summary>
		/// Renders the macro with the specified alias, passing in the specified parameters.
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <param name="parameters">The parameters.</param>
		/// <returns></returns>
		public IHtmlString RenderMacro(string alias, IDictionary<string, object> parameters)
		{
			if (alias == null) throw new ArgumentNullException("alias");
			var containerPage = new FormlessPage();
			var m = macro.GetMacro(alias);
			if (_umbracoContext.PageId == null)
			{
				throw new InvalidOperationException("Cannot render a macro when UmbracoContext.PageId is null.");
			}
			if (_umbracoContext.DocumentRequest == null)
			{
				throw new InvalidOperationException("Cannot render a macro when there is no current DocumentRequest.");
			}
			var macroProps = new Hashtable();
			foreach (var i in parameters)
			{
				//TODO: We are doing at ToLower here because for some insane reason the UpdateMacroModel method of macro.cs 
				// looks for a lower case match. WTF. the whole macro concept needs to be rewritten.
				macroProps.Add(i.Key.ToLower(), i.Value);
			}
			var macroControl = m.renderMacro(macroProps,
				UmbracoContext.Current.DocumentRequest.UmbracoPage.Elements,
				_umbracoContext.PageId.Value);
			containerPage.Controls.Add(macroControl);
			using (var output = new StringWriter())
			{
				_umbracoContext.HttpContext.Server.Execute(containerPage, output, false);
				return new HtmlString(output.ToString());
			}
		}

		#endregion

		#region Field

		/// <summary>
		/// Renders an field to the template
		/// </summary>
		/// <param name="fieldAlias"></param>
		/// <param name="valueAlias"></param>
		/// <param name="altFieldAlias"></param>
		/// <param name="altValueAlias"></param>
		/// <param name="altText"></param>
		/// <param name="insertBefore"></param>
		/// <param name="insertAfter"></param>
		/// <param name="recursive"></param>
		/// <param name="convertLineBreaks"></param>
		/// <param name="removeParagraphTags"></param>
		/// <param name="casing"></param>
		/// <param name="encoding"></param>
		/// <param name="formatString"></param>
		/// <returns></returns>
		public IHtmlString Field(string fieldAlias, string valueAlias = "",
			string altFieldAlias = "", string altValueAlias = "", string altText = "", string insertBefore = "", string insertAfter = "",
			bool recursive = false, bool convertLineBreaks = false, bool removeParagraphTags = false,
			RenderFieldCaseType casing = RenderFieldCaseType.Unchanged,
			RenderFieldEncodingType encoding = RenderFieldEncodingType.Unchanged,
			string formatString = "")
		{
			return Field(_currentPage, fieldAlias, valueAlias, altFieldAlias, altValueAlias,
				altText, insertBefore, insertAfter, recursive, convertLineBreaks, removeParagraphTags,
				casing, encoding, formatString);
		}

		/// <summary>
		/// Renders an field to the template
		/// </summary>
		/// <param name="currentPage"></param>
		/// <param name="fieldAlias"></param>
		/// <param name="valueAlias"></param>
		/// <param name="altFieldAlias"></param>
		/// <param name="altValueAlias"></param>
		/// <param name="altText"></param>
		/// <param name="insertBefore"></param>
		/// <param name="insertAfter"></param>
		/// <param name="recursive"></param>
		/// <param name="convertLineBreaks"></param>
		/// <param name="removeParagraphTags"></param>
		/// <param name="casing"></param>
		/// <param name="encoding"></param>
		/// <param name="formatString"></param>
		/// <returns></returns>
		public IHtmlString Field(IDocument currentPage, string fieldAlias, string valueAlias = "",
			string altFieldAlias = "", string altValueAlias = "", string altText = "", string insertBefore = "", string insertAfter = "",
			bool recursive = false, bool convertLineBreaks = false, bool removeParagraphTags = false,
			RenderFieldCaseType casing = RenderFieldCaseType.Unchanged,
			RenderFieldEncodingType encoding = RenderFieldEncodingType.Unchanged,
			string formatString = "")
		{
			//TODO: This is real nasty and we should re-write the 'item' and 'ItemRenderer' class but si fine for now

			var attributes = new Dictionary<string, string>
				{
					{"field", fieldAlias},
					{"recursive", recursive.ToString().ToLowerInvariant()},
					{"useIfEmpty", altFieldAlias},
					{"textIfEmpty", altText},
					{"stripParagraph", removeParagraphTags.ToString().ToLowerInvariant()},
					{
						"case", casing == RenderFieldCaseType.Lower ? "lower"
						        	: casing == RenderFieldCaseType.Upper ? "upper"
						        	  	: casing == RenderFieldCaseType.Title ? "title"
						        	  	  	: string.Empty
						},
					{"insertTextBefore", insertBefore},
					{"insertTextAfter", insertAfter},
					{"convertLineBreaks", convertLineBreaks.ToString().ToLowerInvariant()}
				};
			switch (encoding)
			{
				case RenderFieldEncodingType.Url:
					attributes.Add("urlEncode", "true");
					break;
				case RenderFieldEncodingType.Html:
					attributes.Add("htmlEncode", "true");
					break;
				case RenderFieldEncodingType.Unchanged:
				default:
					break;
			}

			//need to convert our dictionary over to this weird dictionary type
			var attributesForItem = new AttributeCollectionAdapter(
				new AttributeCollection(
					new StateBag()));
			foreach(var i in attributes)
			{
				attributesForItem.Add(i.Key, i.Value);
			}

			var item = new Item()
				{
					Field = fieldAlias,
					TextIfEmpty = altText,
					LegacyAttributes = attributesForItem
				};			
			var containerPage = new FormlessPage();
			containerPage.Controls.Add(item);

			using (var output = new StringWriter())
			using (var htmlWriter = new HtmlTextWriter(output))
			{
				ItemRenderer.Instance.Init(item);
				ItemRenderer.Instance.Load(item);
				ItemRenderer.Instance.Render(item, htmlWriter);
				return new HtmlString(output.ToString());
			}
		}

		#endregion

		#region Content

		public dynamic ContentById(int id)
		{
			return DocumentById(id, PublishedContentStoreResolver.Current.PublishedContentStore);
		}

		public dynamic ContentById(string id)
		{
			return DocumentById(id, PublishedContentStoreResolver.Current.PublishedContentStore);
		}

		public dynamic ContentByIds(params int[] ids)
		{
			return DocumentByIds(PublishedContentStoreResolver.Current.PublishedContentStore, ids);
		}

		public dynamic ContentByIds(params string[] ids)
		{
			return DocumentByIds(PublishedContentStoreResolver.Current.PublishedContentStore, ids);
		}
		
		#endregion

		#region Media

		public dynamic MediaById(int id)
		{
			return DocumentById(id, PublishedMediaStoreResolver.Current.PublishedMediaStore);
		}

		public dynamic MediaById(string id)
		{
			return DocumentById(id, PublishedMediaStoreResolver.Current.PublishedMediaStore);
		}

		public dynamic MediaByIds(params int[] ids)
		{
			return DocumentByIds(PublishedMediaStoreResolver.Current.PublishedMediaStore, ids);
		}

		public dynamic MediaByIds(params string[] ids)
		{
			return DocumentByIds(PublishedMediaStoreResolver.Current.PublishedMediaStore, ids);
		}

		#endregion

		#region Used by Content/Media

		private dynamic DocumentById(int id, IPublishedStore store)
		{
			var doc = store.GetDocumentById(UmbracoContext.Current, id);
			return doc == null
					? new DynamicNull()
					: new DynamicDocument(doc).AsDynamic();
		}

		private dynamic DocumentById(string id, IPublishedStore store)
		{
			int docId;
			return int.TryParse(id, out docId) 
				? DocumentById(docId, store) 
				: new DynamicNull();
		}

		private dynamic DocumentByIds(IPublishedStore store, params int[] ids)
		{
			var nodes = ids.Select(eachId => DocumentById(eachId, store))
				.Where(x => !TypeHelper.IsTypeAssignableFrom<DynamicNull>(x))
				.Cast<DynamicDocument>();
			return new DynamicDocumentList(nodes);
		}

		private dynamic DocumentByIds(IPublishedStore store, params string[] ids)
		{
			var nodes = ids.Select(eachId => DocumentById(eachId, store))
				.Where(x => !TypeHelper.IsTypeAssignableFrom<DynamicNull>(x))
				.Cast<DynamicDocument>();
			return new DynamicDocumentList(nodes);
		}

		#endregion

		#region Search

		/// <summary>
		/// Searches content
		/// </summary>
		/// <param name="term"></param>
		/// <param name="useWildCards"></param>
		/// <param name="searchProvider"></param>
		/// <returns></returns>
		public dynamic Search(string term, bool useWildCards = true, string searchProvider = null)
		{
			var searcher = Examine.ExamineManager.Instance.DefaultSearchProvider;
			if (!string.IsNullOrEmpty(searchProvider))
				searcher = Examine.ExamineManager.Instance.SearchProviderCollection[searchProvider];

			var results = searcher.Search(term, useWildCards);
			return results.ConvertSearchResultToDynamicDocument(PublishedContentStoreResolver.Current.PublishedContentStore);
		}

		/// <summary>
		/// Searhes content
		/// </summary>
		/// <param name="criteria"></param>
		/// <param name="searchProvider"></param>
		/// <returns></returns>
		public dynamic Search(Examine.SearchCriteria.ISearchCriteria criteria, Examine.Providers.BaseSearchProvider searchProvider = null)
		{
			var s = Examine.ExamineManager.Instance.DefaultSearchProvider;
			if (searchProvider != null)
				s = searchProvider;

			var results = s.Search(criteria);
			return results.ConvertSearchResultToDynamicDocument(PublishedContentStoreResolver.Current.PublishedContentStore);
		}

		#endregion

		#region Xml

		public dynamic ToDynamicXml(string xml)
		{
			if (string.IsNullOrWhiteSpace(xml)) return null;
			var xElement = XElement.Parse(xml);
			return new DynamicXml(xElement);
		}

		public dynamic ToDynamicXml(XElement xElement)
		{
			return new DynamicXml(xElement);
		}

		public dynamic ToDynamicXml(XPathNodeIterator xpni)
		{
			return new DynamicXml(xpni);
		}

		#endregion

		#region Strings

		public HtmlString StripHtml(IHtmlString html, params string[] tags)
		{
			return StripHtml(html.ToHtmlString(), tags);
		}
		public HtmlString StripHtml(DynamicNull html, params string[] tags)
		{
			return new HtmlString(string.Empty);
		}
		public HtmlString StripHtml(string html, params string[] tags)
		{
			return StripHtmlTags(html, tags);
		}

		private HtmlString StripHtmlTags(string html, params string[] tags)
		{
			var doc = new HtmlDocument();
			doc.LoadHtml("<p>" + html + "</p>");
			using (var ms = new MemoryStream())
			{
				var targets = new List<HtmlNode>();

				var nodes = doc.DocumentNode.FirstChild.SelectNodes(".//*");
				if (nodes != null)
				{
					foreach (var node in nodes)
					{
						//is element
						if (node.NodeType != HtmlNodeType.Element) continue;
						var filterAllTags = (tags == null || !tags.Any());
						if (filterAllTags || tags.Any(tag => string.Equals(tag, node.Name, StringComparison.CurrentCultureIgnoreCase)))
						{
							targets.Add(node);
						}
					}
					foreach (var target in targets)
					{
						HtmlNode content = doc.CreateTextNode(target.InnerText);
						target.ParentNode.ReplaceChild(content, target);
					}
				}
				else
				{
					return new HtmlString(html);
				}
				return new HtmlString(doc.DocumentNode.FirstChild.InnerHtml);
			}
		}

		public string Coalesce(params object[] args)
		{
			return Coalesce<DynamicNull>(args);
		}

		internal string Coalesce<TIgnore>(params object[] args)
		{
			foreach (var sArg in args.Where(arg => arg != null && arg.GetType() != typeof(TIgnore)).Select(arg => string.Format("{0}", arg)).Where(sArg => !string.IsNullOrWhiteSpace(sArg)))
			{
				return sArg;
			}
			return string.Empty;
		}

		public string Concatenate(params object[] args)
		{
			return Concatenate<DynamicNull>(args);
		}

		internal string Concatenate<TIgnore>(params object[] args)
		{
			var result = new StringBuilder();
			foreach (var sArg in args.Where(arg => arg != null && arg.GetType() != typeof(TIgnore)).Select(arg => string.Format("{0}", arg)).Where(sArg => !string.IsNullOrWhiteSpace(sArg)))
			{
				result.Append(sArg);
			}
			return result.ToString();
		}

		public string Join(string seperator, params object[] args)
		{
			return Join<DynamicNull>(seperator, args);
		}

		internal string Join<TIgnore>(string seperator, params object[] args)
		{
			var results = args.Where(arg => arg != null && arg.GetType() != typeof (TIgnore)).Select(arg => string.Format("{0}", arg)).Where(sArg => !string.IsNullOrWhiteSpace(sArg)).ToList();
			return string.Join(seperator, results);
		}

		public IHtmlString Truncate(IHtmlString html, int length)
		{
			return Truncate(html.ToHtmlString(), length, true, false);
		}
		public IHtmlString Truncate(IHtmlString html, int length, bool addElipsis)
		{
			return Truncate(html.ToHtmlString(), length, addElipsis, false);
		}
		public IHtmlString Truncate(IHtmlString html, int length, bool addElipsis, bool treatTagsAsContent)
		{
			return Truncate(html.ToHtmlString(), length, addElipsis, treatTagsAsContent);
		}
		public IHtmlString Truncate(DynamicNull html, int length)
		{
			return new HtmlString(string.Empty);
		}
		public IHtmlString Truncate(DynamicNull html, int length, bool addElipsis)
		{
			return new HtmlString(string.Empty);
		}
		public IHtmlString Truncate(DynamicNull html, int length, bool addElipsis, bool treatTagsAsContent)
		{
			return new HtmlString(string.Empty);
		}
		public IHtmlString Truncate(string html, int length)
		{
			return Truncate(html, length, true, false);
		}
		public IHtmlString Truncate(string html, int length, bool addElipsis)
		{
			return Truncate(html, length, addElipsis, false);
		}
		public IHtmlString Truncate(string html, int length, bool addElipsis, bool treatTagsAsContent)
		{
			using (var outputms = new MemoryStream())
			{
				using (var outputtw = new StreamWriter(outputms))
				{
					using (var ms = new MemoryStream())
					{
						using (var tw = new StreamWriter(ms))
						{
							tw.Write(html);
							tw.Flush();
							ms.Position = 0;
							var tagStack = new Stack<string>();
							using (TextReader tr = new StreamReader(ms))
							{
								bool IsInsideElement = false;
								bool lengthReached = false;
								int ic = 0;
								int currentLength = 0, currentTextLength = 0;
								string currentTag = string.Empty;
								string tagContents = string.Empty;
								bool insideTagSpaceEncountered = false;
								bool isTagClose = false;
								while ((ic = tr.Read()) != -1)
								{
									bool write = true;

									if (ic == (int)'<')
									{
										if (!lengthReached)
										{
											IsInsideElement = true;
										}
										insideTagSpaceEncountered = false;
										currentTag = string.Empty;
										tagContents = string.Empty;
										isTagClose = false;
										if (tr.Peek() == (int)'/')
										{
											isTagClose = true;
										}
									}
									else if (ic == (int)'>')
									{
										//if (IsInsideElement)
										//{
										IsInsideElement = false;
										//if (write)
										//{
										//  outputtw.Write('>');
										//}
										currentTextLength++;
										if (isTagClose && tagStack.Count > 0)
										{
											string thisTag = tagStack.Pop();
											outputtw.Write("</" + thisTag + ">");
										}
										if (!isTagClose && currentTag.Length > 0)
										{
											if (!lengthReached)
											{
												tagStack.Push(currentTag);
												outputtw.Write("<" + currentTag);
												if (tr.Peek() != (int)' ')
												{
													if (!string.IsNullOrEmpty(tagContents))
													{
														if (tagContents.EndsWith("/"))
														{
															//short close
															tagStack.Pop();
														}
														outputtw.Write(tagContents);
													}
													outputtw.Write(">");
												}
											}
										}
										//}
										continue;
									}
									else
									{
										if (IsInsideElement)
										{
											if (ic == (int)' ')
											{
												if (!insideTagSpaceEncountered)
												{
													insideTagSpaceEncountered = true;
													//if (!isTagClose)
													//{
													// tagStack.Push(currentTag);
													//}
												}
											}
											if (!insideTagSpaceEncountered)
											{
												currentTag += (char)ic;
											}
										}
									}
									if (IsInsideElement || insideTagSpaceEncountered)
									{
										write = false;
										if (insideTagSpaceEncountered)
										{
											tagContents += (char)ic;
										}
									}
									if (!IsInsideElement || treatTagsAsContent)
									{
										currentTextLength++;
									}
									currentLength++;
									if (currentTextLength <= length || (lengthReached && IsInsideElement))
									{
										if (write)
										{
											outputtw.Write((char)ic);
										}
									}
									if (!lengthReached && currentTextLength >= length)
									{
										//reached truncate point
										if (addElipsis)
										{
											outputtw.Write("&hellip;");
										}
										lengthReached = true;
									}

								}

							}
						}
					}
					outputtw.Flush();
					outputms.Position = 0;
					using (TextReader outputtr = new StreamReader(outputms))
					{
						return new HtmlString(outputtr.ReadToEnd().Replace("  ", " ").Trim());
					}
				}
			}
		}


		#endregion

		#region If

		public HtmlString If(bool test, string valueIfTrue, string valueIfFalse)
		{
			return test ? new HtmlString(valueIfTrue) : new HtmlString(valueIfFalse);
		}
		public HtmlString If(bool test, string valueIfTrue)
		{
			return test ? new HtmlString(valueIfTrue) : new HtmlString(string.Empty);
		}

		#endregion

		#region Wrap

		public HtmlTagWrapper Wrap(string tag, string innerText, params IHtmlTagWrapper[] children)
		{
			var item = Wrap(tag, innerText, (object)null);
			foreach (var child in children)
			{
				item.AddChild(child);
			}
			return item;
		}	

		public HtmlTagWrapper Wrap(string tag, object inner, object anonymousAttributes, params IHtmlTagWrapper[] children)
		{
			string innerText = null;
			if (inner != null && inner.GetType() != typeof(DynamicNull))
			{
				innerText = string.Format("{0}", inner);
			}
			var item = Wrap(tag, innerText, anonymousAttributes);
			foreach (var child in children)
			{
				item.AddChild(child);
			}
			return item;
		}
		public HtmlTagWrapper Wrap(string tag, object inner)
		{
			string innerText = null;
			if (inner != null && inner.GetType() != typeof(DynamicNull))
			{
				innerText = string.Format("{0}", inner);
			}
			return Wrap(tag, innerText, (object)null);
		}
		
		public HtmlTagWrapper Wrap(string tag, string innerText, object anonymousAttributes, params IHtmlTagWrapper[] children)
		{
			var wrap = new HtmlTagWrapper(tag);
			if (anonymousAttributes != null)
			{
				wrap.ReflectAttributesFromAnonymousType(anonymousAttributes);
			}
			if (!string.IsNullOrWhiteSpace(innerText))
			{
				wrap.AddChild(new HtmlTagWrapperTextNode(innerText));
			}
			foreach (var child in children)
			{
				wrap.AddChild(child);
			}
			return wrap;
		}

		public HtmlTagWrapper Wrap(bool visible, string tag, string innerText, object anonymousAttributes, params IHtmlTagWrapper[] children)
		{
			var item = Wrap(tag, innerText, anonymousAttributes, children);
			item.Visible = visible;
			foreach (var child in children)
			{
				item.AddChild(child);
			}
			return item;
		}

		#endregion
	}
}
