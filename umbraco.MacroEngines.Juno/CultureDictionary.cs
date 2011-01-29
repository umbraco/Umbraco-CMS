using System;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.language;

namespace umbraco.MacroEngines {

    public class CultureDictionary : LambdaDictionary<string> {
        public CultureDictionary() : base(CollectEntry) { }

        public static string CollectEntry(string key) {
            try{
                var l = Language.GetByCultureCode(System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
                return new Dictionary.DictionaryItem(key).Value(l.id);
            }
            catch (Exception errDictionary) { }
            return string.Empty;
        }
    }

}
