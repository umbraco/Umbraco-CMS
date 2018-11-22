using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace umbraco.MacroEngines
{

	[Obsolete("This class has been superceded by Umbraco.Core.Dynamics.DynamicDictionary")]
    public class DynamicDictionary : DynamicObject
	{
		private readonly Umbraco.Core.Dynamics.DynamicDictionary _internal;

        public DynamicDictionary(Dictionary<string, object> sourceItems)
        {
        	_internal = new Umbraco.Core.Dynamics.DynamicDictionary(sourceItems);
        }
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
        	return _internal.TrySetMember(binder, value);
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
        	var innerResult = _internal.TryGetMember(binder, out result);
			//special case, we need to check if the result is of a non-legacy dynamic type because if it is, we need 
			//to return the legacy type
			result = LegacyConverter.ConvertToLegacy(result);
	        return innerResult;
        }
    }
}
