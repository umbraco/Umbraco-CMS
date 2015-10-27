using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.language;

namespace umbraco.MacroEngines
{

	//TODO: This is legacy code now since we have the Umbraco.Core.Dictionary.ICultureDictionary and we have a DefaultCultureDictionary 
	// in the Umbraco.Web project. We need to keep this here though because the new ICultureDictionary is different and 
	// doesn't expose the 'Language' property because this is really poor design since the Language object exposes business
	// logic methods and designers could just call 'Delete' on the object and it will actually remove it from the database!! yikes.

	[Obsolete("This class has been superceded by Umbraco.Web.Dictionary.DefaultCultureDictionary")]
	public class UmbracoCultureDictionary : DynamicObject, ICultureDictionary, Umbraco.Core.Dictionary.ICultureDictionary
	{

		public string this[string key]
		{
			get
			{
				try
				{
					return new Dictionary.DictionaryItem(key).Value(Language.id);
				}
				catch (Exception)
				{
					return string.Empty;
				}
			}
		}

		CultureInfo Umbraco.Core.Dictionary.ICultureDictionary.Culture
		{
			get
			{
				return new CultureInfo(Language.CultureAlias);
			}
		}

	    [Obsolete("This returns an empty dictionary, it is not intended to be used by the legacy razor macros")]
	    public IDictionary<string, string> GetChildren(string key)
	    {
	        return new Dictionary<string, string>();
	    }

	    public Language Language
		{
			get { return Language.GetByCultureCode(System.Threading.Thread.CurrentThread.CurrentUICulture.Name); }
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			result = this[binder.Name];
			return true;
		}

	}

}
