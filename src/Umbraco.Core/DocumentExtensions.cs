using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Umbraco.Core.Models;
using umbraco.interfaces;

namespace Umbraco.Core
{
	public static class DocumentExtensions
	{
		/// <summary>
		/// Returns the property as the specified type, if the property is not found or does not convert
		/// then the default value of type T is returned.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="prop"></param>
		/// <param name="alias"></param>
		/// <returns></returns>
		public static T GetPropertyValue<T>(this IDocument prop, string alias)
		{
			return prop.GetPropertyValue<T>(alias, default(T));
		}

		public static T GetPropertyValue<T>(this IDocument prop, string alias, T ifCannotConvert)
		{
			var p = prop.GetProperty(alias);
			if (p == null)
				return default(T);
			var converted = p.TryConvertTo<T>();
			if (converted.Success)
				return converted.Result;
			return ifCannotConvert;
		}

		/// <summary>
		/// Returns the property based on the case insensitive match of the alias
		/// </summary>
		/// <param name="d"></param>
		/// <param name="alias"></param>
		/// <returns></returns>
		public static IDocumentProperty GetProperty(this IDocument d, string alias)
		{
			return d.Properties.FirstOrDefault(p => p.Alias.InvariantEquals(alias));
		}
		
	}
}