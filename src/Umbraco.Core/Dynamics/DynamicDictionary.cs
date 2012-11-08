using System.Collections.Generic;
using System.Dynamic;

namespace Umbraco.Core.Dynamics
{
    public class DynamicDictionary : DynamicObject
    {
    	internal readonly Dictionary<string, object> SourceItems;

        public DynamicDictionary(Dictionary<string, object> sourceItems)
        {
            SourceItems = sourceItems;
        }
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (SourceItems.ContainsKey(binder.Name))
            {
                SourceItems[binder.Name.ToLower()] = value;
            }
            else
            {
                SourceItems.Add(binder.Name.ToLower(), value);
            }
            return true;
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (SourceItems != null)
            {
                if (SourceItems.TryGetValue(binder.Name.ToLower(), out result))
                {
                    return true;
                }
            }
            result = null;
            return true;
        }
    }
}
