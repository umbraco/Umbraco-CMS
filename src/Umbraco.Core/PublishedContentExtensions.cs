using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Mime;
using System.Web;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using umbraco.interfaces;

namespace Umbraco.Core
{
	/// <summary>
	/// Extension methods for IPublishedContent
	/// </summary>
	public static class PublishedContentExtensions
	{

		#region GetProperty
		public static IPublishedContentProperty GetProperty(this IPublishedContent content, string alias, bool recursive)
		{
			return content.GetPropertyRecursive(alias, recursive);
		}

		private static IPublishedContentProperty GetPropertyRecursive(this IPublishedContent content, string alias, bool recursive = false)
		{
			if (!recursive)
			{
				return content.GetProperty(alias);
			}
			var context = content;
			var prop = content.GetPropertyRecursive(alias);
			while (prop == null || prop.Value == null || prop.Value.ToString().IsNullOrWhiteSpace())
			{
                if (context.Parent == null) break;
                context = context.Parent;
                prop = context.GetPropertyRecursive(alias);
			}
			return prop;
		} 
		#endregion

		#region HasValue

		public static bool HasValue(this IPublishedContentProperty prop)
		{
			if (prop == null) return false;
			if (prop.Value == null) return false;
			return !prop.Value.ToString().IsNullOrWhiteSpace();			
		}

		public static bool HasValue(this IPublishedContent doc, string alias)
		{
			return doc.HasValue(alias, false);
		}
		public static bool HasValue(this IPublishedContent doc, string alias, bool recursive)
		{
			var prop = doc.GetProperty(alias, recursive);
			if (prop == null) return false;
			return prop.HasValue();
		}
		public static IHtmlString HasValue(this IPublishedContent doc, string alias, string valueIfTrue, string valueIfFalse)
		{
			return doc.HasValue(alias, false) ? new HtmlString(valueIfTrue) : new HtmlString(valueIfFalse);
		}
		public static IHtmlString HasValue(this IPublishedContent doc, string alias, bool recursive, string valueIfTrue, string valueIfFalse)
		{
			return doc.HasValue(alias, recursive) ? new HtmlString(valueIfTrue) : new HtmlString(valueIfFalse);
		}
		public static IHtmlString HasValue(this IPublishedContent doc, string alias, string valueIfTrue)
		{
			return doc.HasValue(alias, false) ? new HtmlString(valueIfTrue) : new HtmlString(string.Empty);
		}
		public static IHtmlString HasValue(this IPublishedContent doc, string alias, bool recursive, string valueIfTrue)
		{
			return doc.HasValue(alias, recursive) ? new HtmlString(valueIfTrue) : new HtmlString(string.Empty);
		}
		#endregion

		/// <summary>
		/// Returns the recursive value of a field by iterating up the parent chain but starting at the publishedContent passed in
		/// </summary>
		/// <param name="publishedContent"></param>
		/// <param name="fieldname"></param>
		/// <returns></returns>
		public static string GetRecursiveValue(this IPublishedContent publishedContent, string fieldname)
		{
			//check for the cached value in the objects properties first
			var cachedVal = publishedContent["__recursive__" + fieldname];
			if (cachedVal != null)
			{
				return cachedVal.ToString();
			}

			var contentValue = "";
			var currentContent = publishedContent;

			while (contentValue.IsNullOrWhiteSpace())
			{
				var val = currentContent[fieldname];
				if (val == null || val.ToString().IsNullOrWhiteSpace())
				{
					if (currentContent.Parent == null)
					{
						break; //we've reached the top
					}
					currentContent = currentContent.Parent;
				}
				else
				{
					contentValue = val.ToString(); //we've found a recursive val
				}
			}

			//cache this lookup in a new custom (hidden) property
			publishedContent.Properties.Add(new PropertyResult("__recursive__" + fieldname, contentValue, Guid.Empty, PropertyResultType.CustomProperty));

			return contentValue;
		}

		public static bool IsVisible(this IPublishedContent doc)
		{
			var umbracoNaviHide = doc.GetProperty(Constants.Conventions.Content.NaviHide);
			if (umbracoNaviHide != null)
			{
				return umbracoNaviHide.Value.ToString().Trim() != "1";
			}
			return true;
		}

		public static bool HasProperty(this IPublishedContent doc, string name)
		{
			if (doc != null)
			{
				var prop = doc.GetProperty(name);

				return (prop != null);
			}
			return false;
		}


		
	}
}