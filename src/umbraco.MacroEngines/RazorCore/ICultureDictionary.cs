using System;
using umbraco.cms.businesslogic.language;

namespace umbraco.MacroEngines
{
	[Obsolete("This class has been superceded by Umbraco.Core.Dictionary.ICultureDictionary")]
	public interface ICultureDictionary
	{
		string this[string key] { get; }
		Language Language { get; }
	}

}
