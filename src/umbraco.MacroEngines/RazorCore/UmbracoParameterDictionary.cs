using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using umbraco.cms.businesslogic.macro;
using System.Linq;

namespace umbraco.MacroEngines
{

    public class UmbracoParameterDictionary : DynamicObject, IParameterDictionary
    {

        private readonly IEnumerable<MacroPropertyModel> _paramsKeyValue;

        public UmbracoParameterDictionary(IEnumerable<MacroPropertyModel> paramsKeyValue)
        {
            _paramsKeyValue = paramsKeyValue;
            if (_paramsKeyValue == null)
                _paramsKeyValue = new List<MacroPropertyModel>();
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _paramsKeyValue.Select(p => new KeyValuePair<string, string>(p.Key, p.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string this[string alias]
        {
            get
            {
                return _paramsKeyValue.Where(p => p.Key.ToLower() == alias.ToLower()).Select(p => p.Value).FirstOrDefault();
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this[binder.Name];

            return true;
        }

    }

}