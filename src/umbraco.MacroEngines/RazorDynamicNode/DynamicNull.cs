using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Dynamic;
using System.Web;

namespace umbraco.MacroEngines
{
    //This type is used as a return type when TryGetMember fails on a DynamicNode
    //.Where explicitly checks for this type, to indicate that nothing was returned
    //Because it's IEnumerable, if the user is actually trying @Model.TextPages or similar
    //it will still return an enumerable object (assuming the call actually failed because there were no children of that type)
    //but in .Where, if they use a property that doesn't exist, the lambda will bypass this and return false
	[Obsolete("This class has been superceded by Umbraco.Core.Dynamics.DynamicNull")]
	public class DynamicNull : DynamicObject, IEnumerable, IHtmlString
	{
		private readonly Umbraco.Core.Dynamics.DynamicNull _inner = Umbraco.Core.Dynamics.DynamicNull.Null;

        public IEnumerator GetEnumerator()
        {
        	return _inner.GetEnumerator();
        }
        public DynamicNull Where(string predicate, params object[] values)
        {
            return this;
        }
        public DynamicNull OrderBy(string orderBy)
        {
            return this;
        }
        public int Count()
        {
            return _inner.Count();
        }
        public override string ToString()
        {
			return _inner.ToString();
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
			var innerResult =_inner.TryGetMember(binder, out result);
			//special case, we need to check if the result is of a non-legacy dynamic type because if it is, we need 
			//to return the legacy type
			result = LegacyConverter.ConvertToLegacy(result);
			return innerResult;
        }
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
			var innerResult = _inner.TryGetIndex(binder, indexes, out result);
			//special case, we need to check if the result is of a non-legacy dynamic type because if it is, we need 
			//to return the legacy type
			result = LegacyConverter.ConvertToLegacy(result);
			return innerResult;
        }
        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
			var innerResult = _inner.TryInvoke(binder, args, out result);
			//special case, we need to check if the result is of a non-legacy dynamic type because if it is, we need 
			//to return the legacy type
			result = LegacyConverter.ConvertToLegacy(result);
			return innerResult;
        }
        public bool IsNull()
        {
			return _inner.IsNull();
        }
        public bool HasValue()
        {
			return _inner.HasValue();
        }
        public string Name
        {
            get { return _inner.Name; }
        }
        public int Id
        {
            get { return _inner.Id; }
        }

        public static implicit operator bool(DynamicNull n)
        {
            return false;
        }
        public static implicit operator DateTime(DynamicNull n)
        {
            return DateTime.MinValue;
        }
        public static implicit operator int(DynamicNull n)
        {
            return 0;
        }
        public static implicit operator string(DynamicNull n)
        {
            return string.Empty;
        }

        public string ToHtmlString()
        {
			return _inner.ToHtmlString();
        }

    }
}
