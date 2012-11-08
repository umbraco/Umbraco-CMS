using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace umbraco.MacroEngines
{

	[Obsolete("This class has been superceded by Umbraco.Core.Dynamics.DynamicQueryableGetMemberBinder")]	
    public class DynamicQueryableGetMemberBinder : GetMemberBinder
	{
		private readonly Umbraco.Core.Dynamics.DynamicQueryableGetMemberBinder _inner;

        public DynamicQueryableGetMemberBinder(string name, bool ignoreCase) 
			: base(name, ignoreCase)
        {
        	_inner = new Umbraco.Core.Dynamics.DynamicQueryableGetMemberBinder(name, ignoreCase);
        }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
        	return _inner.FallbackGetMember(target, errorSuggestion);
        }
    }
}
