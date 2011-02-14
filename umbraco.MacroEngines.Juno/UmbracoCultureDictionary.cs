using System;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.language;

namespace umbraco.MacroEngines {

    public class UmbracoCultureDictionary : ICultureDictionary {

        public string this[string key] {
            get {
                try {
                    return new Dictionary.DictionaryItem(key).Value(Language.id);
                } catch (Exception errDictionary) { }
                return string.Empty;
            }
        }

        public Language Language {
            get { return Language.GetByCultureCode(System.Threading.Thread.CurrentThread.CurrentUICulture.Name); }
        }

    }

}
