using System;
using System.Dynamic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.language;

namespace umbraco.MacroEngines {

    public class UmbracoCultureDictionary : DynamicObject, ICultureDictionary {

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

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            result = this[binder.Name];
            return true;
        }

    }

}
