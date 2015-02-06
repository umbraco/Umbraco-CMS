using System;
using System.Dynamic;
using System.Globalization;
using System.Web;
using Umbraco.Core.Logging;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.language;

namespace Umbraco.Web.Dictionary
{

	public class DefaultCultureDictionary : Umbraco.Core.Dictionary.ICultureDictionary
	{
		/// <summary>
		/// Returns the dictionary value based on the key supplied
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public string this[string key]
		{
			get
			{
				try
				{
					return new global::umbraco.cms.businesslogic.Dictionary.DictionaryItem(key).Value(Language.id);
				}
				catch (Exception e)
				{
					LogHelper.WarnWithException<DefaultCultureDictionary>("Error returning dictionary item '" + key + "'", true, e);		
					return string.Empty;
				}
			}
		}

		/// <summary>
		/// Returns the current culture
		/// </summary>
		public CultureInfo Culture
		{
			get { return System.Threading.Thread.CurrentThread.CurrentUICulture; }
		}

		private Language Language
		{
			get { return Language.GetByCultureCode(System.Threading.Thread.CurrentThread.CurrentUICulture.Name); }
		}
	}

}
