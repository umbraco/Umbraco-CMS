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
		private readonly Umbraco.Core.Dynamics.DynamicNull _inner = new Umbraco.Core.Dynamics.DynamicNull();

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
			return _inner.TryGetMember(binder, out result);
        }
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
			return _inner.TryGetIndex(binder, indexes, out result);
        }
        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
			return _inner.TryInvoke(binder, args, out result);
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
