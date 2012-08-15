using System;
using System.Dynamic;
using System.Globalization;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.language;

namespace Umbraco.Web.Dictionary
{

	internal class DefaultCultureDictionary : Umbraco.Core.Dictionary.ICultureDictionary
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
				catch (Exception)
				{
					//NOTE: SD: I'm not sure why this is here but was copied from the UmbracoCultureDictionary in the macroEngines project
					// which previously seems to have worked so I'm leaving it for now.
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
