using System;
using System.Collections.Generic;
using System.Collections;
using System.Dynamic;
using System.Web;

namespace Umbraco.Core.Dynamics
{
    //This type is used as a return type when TryGetMember fails on a DynamicNode
    //.Where explicitly checks for this type, to indicate that nothing was returned
    //Because it's IEnumerable, if the user is actually trying @Model.TextPages or similar
    //it will still return an enumerable object (assuming the call actually failed because there were no children of that type)
    //but in .Where, if they use a property that doesn't exist, the lambda will bypass this and return false

    // returned when TryGetMember fails on a DynamicPublishedContent
    //
    // so if user does @CurrentPage.TextPages it will get something that is enumerable (but empty)
    // note - not sure I understand the stuff about .Where, though

    public class DynamicNull : DynamicObject, IEnumerable, IHtmlString
    {
        public static readonly DynamicNull Null = new DynamicNull();

        private DynamicNull() {}

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

        public DynamicNull ToContentSet()
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

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            result = this;
            return true;
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
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

        public string Name
        {
            get { return string.Empty; }
        }

        public int Id
        {
            get { return 0; }
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
            return string.Empty;
        }
    }
}
