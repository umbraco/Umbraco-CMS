using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

		#region GetPropertyValue
		public static string GetPropertyValue(this IPublishedContent doc, string alias)
		{
			return doc.GetPropertyValue(alias, false);
		}
		public static string GetPropertyValue(this IPublishedContent doc, string alias, string fallback)
		{
			var prop = doc.GetPropertyValue(alias);
			return !prop.IsNullOrWhiteSpace() ? prop : fallback;
		}
		public static string GetPropertyValue(this IPublishedContent doc, string alias, bool recursive)
		{
			var p = doc.GetProperty(alias, recursive);
			return p == null ? null : Convert.ToString(p.Value);
		}
		public static string GetPropertyValue(this IPublishedContent doc, string alias, bool recursive, string fallback)
		{
			var prop = doc.GetPropertyValue(alias, recursive);
			return !prop.IsNullOrWhiteSpace() ? prop : fallback;
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

		public static bool IsVisible(this IPublishedContent doc)
		{
			var umbracoNaviHide = doc.GetProperty("umbracoNaviHide");
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

		/// <summary>
		/// Returns the property as the specified type, if the property is not found or does not convert
		/// then the default value of type T is returned.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="prop"></param>
		/// <param name="alias"></param>
		/// <returns></returns>
		public static T GetPropertyValue<T>(this IPublishedContent prop, string alias)
		{
			return prop.GetPropertyValue<T>(alias, default(T));
		}

		public static T GetPropertyValue<T>(this IPublishedContent prop, string alias, T ifCannotConvert)
		{
			var p = prop.GetProperty(alias);
			if (p == null)
				return default(T);
			var converted = p.Value.TryConvertTo<T>();
			if (converted.Success)
				return converted.Result;
			return ifCannotConvert;
		}
		
	}
}