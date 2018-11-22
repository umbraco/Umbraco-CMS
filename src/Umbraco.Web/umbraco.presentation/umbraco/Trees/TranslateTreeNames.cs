using System.Linq;
using umbraco.cms.businesslogic.language;

namespace Umbraco.Web.umbraco.presentation.umbraco.Trees
{
    internal class TranslateTreeNames
    {
        public static string GetTranslatedName(string originalName)
        {

            if (originalName.StartsWith("#") == false)
                return originalName;

            var lang = Language.GetByCultureCode(System.Threading.Thread.CurrentThread.CurrentCulture.Name);

            if (lang != null && global::umbraco.cms.businesslogic.Dictionary.DictionaryItem.hasKey(originalName.Substring(1, originalName.Length - 1)))
            {
                var dictionaryItem = new global::umbraco.cms.businesslogic.Dictionary.DictionaryItem(originalName.Substring(1, originalName.Length - 1));
                if (dictionaryItem != null)
                    return dictionaryItem.Value(lang.id);
            }

            return "[" + originalName + "]";
        }
    }
}
