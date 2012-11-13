using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Mime;
using System.Web;
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
		public static object GetPropertyValue(this IPublishedContent doc, string alias)
		{
			return doc.GetPropertyValue(alias, false);
		}
		public static object GetPropertyValue(this IPublishedContent doc, string alias, string fallback)
		{
			var prop = doc.GetPropertyValue(alias);
			return (prop != null && !Convert.ToString(prop).IsNullOrWhiteSpace()) ? prop : fallback;
		}
		public static object GetPropertyValue(this IPublishedContent doc, string alias, bool recursive)
		{
			var p = doc.GetProperty(alias, recursive);
			if (p == null) return null;

			//Here we need to put the value through the IPropertyEditorValueConverter's
			//get the data type id for the current property
			var dataType = PublishedContentHelper.GetDataType(doc.DocumentTypeAlias, alias);
			//convert the string value to a known type
			var converted = PublishedContentHelper.ConvertPropertyValue(p.Value, dataType, doc.DocumentTypeAlias, alias);
			return converted.Success 
				? converted.Result
				: p.Value;
		}
		public static object GetPropertyValue(this IPublishedContent doc, string alias, bool recursive, string fallback)
		{
			var prop = doc.GetPropertyValue(alias, recursive);
			return (prop != null && !Convert.ToString(prop).IsNullOrWhiteSpace()) ? prop : fallback;
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
		/// <param name="doc"></param>
		/// <param name="alias"></param>
		/// <returns></returns>
		public static T GetPropertyValue<T>(this IPublishedContent doc, string alias)
		{
			return doc.GetPropertyValue<T>(alias, default(T));
		}

		public static T GetPropertyValue<T>(this IPublishedContent prop, string alias, bool recursive, T ifCannotConvert)
		{
			var p = prop.GetProperty(alias, recursive);
			if (p == null)
				return ifCannotConvert;

			//before we try to convert it manually, lets see if the PropertyEditorValueConverter does this for us
			//Here we need to put the value through the IPropertyEditorValueConverter's
			//get the data type id for the current property
			var dataType = PublishedContentHelper.GetDataType(prop.DocumentTypeAlias, alias);
			//convert the value to a known type
			var converted = PublishedContentHelper.ConvertPropertyValue(p.Value, dataType, prop.DocumentTypeAlias, alias);
			if (converted.Success)
			{
				//if its successful, check if its the correct type and return it
				if (converted.Result is T)
				{
					return (T)converted.Result;
				}
				//if that's not correct, try converting the converted type
				var reConverted = converted.Result.TryConvertTo<T>();
				if (reConverted.Success)
				{
					return reConverted.Result;
				}
			}

			//last, if all the above has failed, we'll just try converting the raw value straight to 'T'
			var manualConverted = p.Value.TryConvertTo<T>();
			if (manualConverted.Success)
				return manualConverted.Result;
			return ifCannotConvert;
		}

		public static T GetPropertyValue<T>(this IPublishedContent prop, string alias, T ifCannotConvert)
		{
			return prop.GetPropertyValue<T>(alias, false, ifCannotConvert);
		}
		
	}
}