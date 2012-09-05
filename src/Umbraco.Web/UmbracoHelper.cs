using System;
using System.Collections;
using System.IO;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using Umbraco.Core;
using Umbraco.Core.Models;
using umbraco;
using System.Collections.Generic;
using umbraco.presentation.templateControls;

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
			foreach(var i in parameters.ToDictionary<object>())
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
				_umbracoContext.HttpContext.Server.Execute(containerPage, output, false);
				return new HtmlString(output.ToString());
			}
		}

		#endregion

	}
}
