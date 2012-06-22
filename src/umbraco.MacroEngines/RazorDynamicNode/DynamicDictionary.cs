using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace umbraco.MacroEngines
{
    public class DynamicDictionary : DynamicObject
    {
        Dictionary<string, object> _dictionary;
        public DynamicDictionary(Dictionary<string, object> sourceItems)
        {
            _dictionary = sourceItems;
        }
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (_dictionary.ContainsKey(binder.Name))
            {
                _dictionary[binder.Name.ToLower()] = value;
            }
            else
            {
                _dictionary.Add(binder.Name.ToLower(), value);
            }
            return true;
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_dictionary != null)
            {
                if (_dictionary.TryGetValue(binder.Name.ToLower(), out result))
                {
                    return true;
                }
            }
            result = null;
            return true;
        }
    }
}
