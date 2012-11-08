using System;
using System.Dynamic;

namespace Umbraco.Core.Dynamics
{
    internal class DynamicQueryableGetMemberBinder : GetMemberBinder
    {
        public DynamicQueryableGetMemberBinder(string name, bool ignoreCase) : base(name, ignoreCase) { }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            throw new NotImplementedException();
        }
    }
}
