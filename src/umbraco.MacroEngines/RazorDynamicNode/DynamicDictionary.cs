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
        	return _internal.TryGetMember(binder, out result);
        }
    }
}
