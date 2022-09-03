using System.Globalization;

namespace Umbraco.Cms.Core.Dictionary;

public interface ICultureDictionaryFactory
{
    ICultureDictionary CreateDictionary();
    ICultureDictionary CreateDictionary(CultureInfo specificCulture);
}
