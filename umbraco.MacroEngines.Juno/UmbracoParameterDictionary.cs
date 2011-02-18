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
            get { return _paramsKeyValue.Where(p => p.Key == alias).Select(p => p.Value).FirstOrDefault(); }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this[binder.Name];

            // we'll check if parameters no matter the casing (due to the macro parser lower case all parameter aliases)
            if (result == null || String.IsNullOrEmpty(result.ToString()))
            {

                if (this.Any(x => x.Key.ToLower() == binder.Name.ToLower()))
                    result = this.First(x => x.Key.ToLower() == binder.Name.ToLower()).Value;
            }
            return true;
        }
    }

}