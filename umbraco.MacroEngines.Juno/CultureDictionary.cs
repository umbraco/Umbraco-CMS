using System;
using System.Collections.Generic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.language;

namespace umbraco.MacroEngines {
    public class CultureDictionary : DynamicLambdaDictionary<string> {
        public CultureDictionary() : base(ParseDictionaryKey) { }

        public static string ParseDictionaryKey(string key) {
            if (string.IsNullOrEmpty(key))
                return string.Empty;
            var list = new List<string>();
            list.Add(key);
            if (key.Contains("_"))
                list.Add(key.Replace("_", " "));
            return GetFirstDictionaryItem(list);
        }

        public static string GetFirstDictionaryItem(IEnumerable<string> keys) {
            foreach (var item in keys) {
                try {
                    var l = Language.GetByCultureCode(System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
                    return new Dictionary.DictionaryItem(item).Value(l.id);
                } catch (Exception errDictionary) {}
            }
            return string.Empty;
        }
    }
}
