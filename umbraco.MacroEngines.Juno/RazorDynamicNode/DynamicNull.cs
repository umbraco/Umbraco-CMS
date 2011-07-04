using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Dynamic;

namespace umbraco.MacroEngines
{
    //This type is used as a return type when TryGetMember fails on a DynamicNode
    //.Where explicitly checks for this type, to indicate that nothing was returned
    //Because it's IEnumerable, if the user is actually trying @Model.TextPages or similar
    //it will still return an enumerable object (assuming the call actually failed because there were no children of that type)
    //but in .Where, if they use a property that doesn't exist, the lambda will bypass this and return false
    public class DynamicNull : DynamicObject, IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            return (new List<DynamicNull>()).GetEnumerator();
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
            return 0;
        }
        public override string ToString()
        {
            return string.Empty;
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this;
            return true;
        }
        public bool IsNull()
        {
            return true;
        }
        public bool HasValue()
        {
            return false;
        }
        public static implicit operator bool(DynamicNull n)
        {
            return false;
        }
    }
}
