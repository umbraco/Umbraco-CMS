using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Collections;
using System.IO;
using System.Web;
using Umbraco.Core;

namespace umbraco.MacroEngines
{
	[Obsolete("This class has been superceded by Umbraco.Core.Dynamics.DynamicXml")]
	public class DynamicXml : DynamicObject, IEnumerable<DynamicXml>, IEnumerable<XElement>
	{
		private readonly Umbraco.Core.Dynamics.DynamicXml _inner;

		public XElement BaseElement
		{
			get { return _inner.BaseElement; }
			set { _inner.BaseElement = value; }
		}

        public DynamicXml(XElement baseElement)
        {
        	_inner = new Umbraco.Core.Dynamics.DynamicXml(baseElement);
        }
        public DynamicXml(string xml)
        {
			_inner = new Umbraco.Core.Dynamics.DynamicXml(xml);
        }
        public DynamicXml(XPathNodeIterator xpni)
        {
			_inner = new Umbraco.Core.Dynamics.DynamicXml(xpni);
        }
        public string InnerText
        {
            get { return _inner.InnerText; }
        }
        public string ToXml()
        {
        	return _inner.ToXml();
        }
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
        	var innerResult = _inner.TryGetIndex(binder, indexes, out result);
			//special case, we need to check if the result is of a non-legacy dynamic type because if it is, we need 
			//to return the legacy type
			result = LegacyConverter.ConvertToLegacy(result);
			return innerResult;
        }
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
        	var innerResult = _inner.TryInvokeMember(binder, args, out result);
			//special case, we need to check if the result is of a non-legacy dynamic type because if it is, we need 
			//to return the legacy type
			result = LegacyConverter.ConvertToLegacy(result);
			return innerResult;
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
			var innerResult = _inner.TryGetMember(binder, out result);
			//special case, we need to check if the result is of a non-legacy dynamic type because if it is, we need 
			//to return the legacy type
			result = LegacyConverter.ConvertToLegacy(result);
	        return innerResult;
        }

        public DynamicXml XPath(string expression)
        {
			var matched = _inner.BaseElement.XPathSelectElements(expression);
            var root = new DynamicXml("<results/>");
            foreach (var element in matched)
            {
                root.BaseElement.Add(element);
            }
            return root;
        }
        public IHtmlString ToHtml()
        {
        	return _inner.ToHtml();
        }
        public DynamicXml Find(string expression)
        {
			return new DynamicXml(_inner.BaseElement.XPathSelectElements(expression).FirstOrDefault());
        }

        public DynamicXml Find(string attributeName, object value)
        {
            string expression = string.Format("//*[{0}='{1}']", attributeName, value);
			return new DynamicXml(_inner.BaseElement.XPathSelectElements(expression).FirstOrDefault());
        }

		IEnumerator<XElement> IEnumerable<XElement>.GetEnumerator()
		{
			return this.BaseElement.Elements().GetEnumerator();
		}

		public IEnumerator<DynamicXml> GetEnumerator()
		{
			return this.BaseElement.Elements().Select(e => new DynamicXml(e)).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

        public int Count()
        {
        	return _inner.Count();
        }

        public bool Any()
        {
            return _inner.Any();
        }

        public bool IsNull()
        {
        	return _inner.IsNull();
        }
        public bool HasValue()
        {
        	return _inner.HasValue();
        }

		public IEnumerable<DynamicXml> Take(int count)
		{
			return _inner.Take(count).Select(x => new DynamicXml(x.BaseElement));
		}

		public IEnumerable<DynamicXml> Skip(int count)
		{
			return _inner.Skip(count).Select(x => new DynamicXml(x.BaseElement));
		}

        public bool IsFirst()
        {
        	return _inner.IsFirst();
        }
        public HtmlString IsFirst(string valueIfTrue)
        {
        	return _inner.IsFirst(valueIfTrue);
        }
        public HtmlString IsFirst(string valueIfTrue, string valueIfFalse)
        {
        	return _inner.IsFirst(valueIfTrue, valueIfFalse);
        }
        public bool IsNotFirst()
        {
        	return _inner.IsNotFirst();
        }
        public HtmlString IsNotFirst(string valueIfTrue)
        {
			return _inner.IsNotFirst(valueIfTrue);
        }
        public HtmlString IsNotFirst(string valueIfTrue, string valueIfFalse)
        {
			return _inner.IsNotFirst(valueIfTrue, valueIfFalse);
        }
        public bool IsPosition(int index)
        {
			return _inner.IsPosition(index);
        }
        public HtmlString IsPosition(int index, string valueIfTrue)
        {
			return _inner.IsPosition(index, valueIfTrue);
        }
        public HtmlString IsPosition(int index, string valueIfTrue, string valueIfFalse)
        {
			return _inner.IsPosition(index, valueIfTrue, valueIfFalse);
        }
        public bool IsModZero(int modulus)
        {
			return _inner.IsModZero(modulus);
        }
        public HtmlString IsModZero(int modulus, string valueIfTrue)
        {
			return _inner.IsModZero(modulus, valueIfTrue);
        }
        public HtmlString IsModZero(int modulus, string valueIfTrue, string valueIfFalse)
        {
			return _inner.IsModZero(modulus, valueIfTrue, valueIfFalse);
        }

        public bool IsNotModZero(int modulus)
        {
			return _inner.IsNotModZero(modulus);
        }
        public HtmlString IsNotModZero(int modulus, string valueIfTrue)
        {
			return _inner.IsNotModZero(modulus, valueIfTrue);
        }
        public HtmlString IsNotModZero(int modulus, string valueIfTrue, string valueIfFalse)
        {
			return _inner.IsNotModZero(modulus, valueIfTrue, valueIfFalse);
        }
        public bool IsNotPosition(int index)
        {
			return _inner.IsNotPosition(index);
        }
        public HtmlString IsNotPosition(int index, string valueIfTrue)
        {
			return _inner.IsNotPosition(index, valueIfTrue);
        }
        public HtmlString IsNotPosition(int index, string valueIfTrue, string valueIfFalse)
        {
			return _inner.IsNotPosition(index, valueIfTrue);
        }
        public bool IsLast()
        {
			return _inner.IsLast();
        }
        public HtmlString IsLast(string valueIfTrue)
        {
			return _inner.IsLast(valueIfTrue);
        }
        public HtmlString IsLast(string valueIfTrue, string valueIfFalse)
        {
			return _inner.IsLast(valueIfTrue, valueIfFalse);
        }
        public bool IsNotLast()
        {
			return _inner.IsNotLast();
        }
        public HtmlString IsNotLast(string valueIfTrue)
        {
			return _inner.IsNotLast(valueIfTrue);
        }
        public HtmlString IsNotLast(string valueIfTrue, string valueIfFalse)
        {
			return _inner.IsNotLast(valueIfTrue, valueIfFalse);
        }
        public bool IsEven()
        {
			return _inner.IsEven();
        }
        public HtmlString IsEven(string valueIfTrue)
        {
			return _inner.IsEven(valueIfTrue);
        }
        public HtmlString IsEven(string valueIfTrue, string valueIfFalse)
        {
			return _inner.IsEven(valueIfTrue, valueIfFalse);
        }
        public bool IsOdd()
        {
			return _inner.IsOdd();
        }
        public HtmlString IsOdd(string valueIfTrue)
        {
			return _inner.IsOdd(valueIfTrue);
        }
        public HtmlString IsOdd(string valueIfTrue, string valueIfFalse)
        {
			return _inner.IsOdd(valueIfTrue, valueIfFalse);
        }

        public bool IsEqual(DynamicXml other)
        {
        	return _inner.IsEqual(new Umbraco.Core.Dynamics.DynamicXml(other.BaseElement));
        }
        public HtmlString IsEqual(DynamicXml other, string valueIfTrue)
        {
			return _inner.IsEqual(new Umbraco.Core.Dynamics.DynamicXml(other.BaseElement), valueIfTrue);
        }
        public HtmlString IsEqual(DynamicXml other, string valueIfTrue, string valueIfFalse)
        {
			return _inner.IsEqual(new Umbraco.Core.Dynamics.DynamicXml(other.BaseElement), valueIfTrue, valueIfFalse);
        }
        public bool IsNotEqual(DynamicXml other)
        {
			return _inner.IsNotEqual(new Umbraco.Core.Dynamics.DynamicXml(other.BaseElement));
        }
        public HtmlString IsNotEqual(DynamicXml other, string valueIfTrue)
        {
			return _inner.IsNotEqual(new Umbraco.Core.Dynamics.DynamicXml(other.BaseElement), valueIfTrue);
        }
        public HtmlString IsNotEqual(DynamicXml other, string valueIfTrue, string valueIfFalse)
        {
			return _inner.IsNotEqual(new Umbraco.Core.Dynamics.DynamicXml(other.BaseElement), valueIfTrue, valueIfFalse);
        }
        public bool IsDescendant(DynamicXml other)
        {
			return _inner.IsDescendant(new Umbraco.Core.Dynamics.DynamicXml(other.BaseElement));       
        }
        public HtmlString IsDescendant(DynamicXml other, string valueIfTrue)
        {
			return _inner.IsDescendant(new Umbraco.Core.Dynamics.DynamicXml(other.BaseElement), valueIfTrue);       
        }
        public HtmlString IsDescendant(DynamicXml other, string valueIfTrue, string valueIfFalse)
        {
			return _inner.IsDescendant(new Umbraco.Core.Dynamics.DynamicXml(other.BaseElement), valueIfTrue, valueIfFalse);       
        }
        public bool IsDescendantOrSelf(DynamicXml other)
        {
			return _inner.IsDescendantOrSelf(new Umbraco.Core.Dynamics.DynamicXml(other.BaseElement));       
        }
        public HtmlString IsDescendantOrSelf(DynamicXml other, string valueIfTrue)
        {
			return _inner.IsDescendantOrSelf(new Umbraco.Core.Dynamics.DynamicXml(other.BaseElement), valueIfTrue);   
        }
        public HtmlString IsDescendantOrSelf(DynamicXml other, string valueIfTrue, string valueIfFalse)
        {
			return _inner.IsDescendantOrSelf(new Umbraco.Core.Dynamics.DynamicXml(other.BaseElement), valueIfTrue, valueIfFalse);   
        }
        public bool IsAncestor(DynamicXml other)
        {
			return _inner.IsAncestor(new Umbraco.Core.Dynamics.DynamicXml(other.BaseElement));   
        }
        public HtmlString IsAncestor(DynamicXml other, string valueIfTrue)
        {
			return _inner.IsAncestor(new Umbraco.Core.Dynamics.DynamicXml(other.BaseElement), valueIfTrue);   
        }
        public HtmlString IsAncestor(DynamicXml other, string valueIfTrue, string valueIfFalse)
        {
			return _inner.IsAncestor(new Umbraco.Core.Dynamics.DynamicXml(other.BaseElement), valueIfTrue, valueIfFalse);   
        }
        public bool IsAncestorOrSelf(DynamicXml other)
        {
			return _inner.IsAncestorOrSelf(new Umbraco.Core.Dynamics.DynamicXml(other.BaseElement));   
        }
        public HtmlString IsAncestorOrSelf(DynamicXml other, string valueIfTrue)
        {
			return _inner.IsAncestorOrSelf(new Umbraco.Core.Dynamics.DynamicXml(other.BaseElement), valueIfTrue);   
        }
        public HtmlString IsAncestorOrSelf(DynamicXml other, string valueIfTrue, string valueIfFalse)
        {
			return _inner.IsAncestorOrSelf(new Umbraco.Core.Dynamics.DynamicXml(other.BaseElement), valueIfTrue, valueIfFalse);   
        }
        public List<DynamicXml> Descendants()
        {
			return _inner.Descendants().Select(x => new DynamicXml(x.BaseElement)).ToList();   
        }
        public List<DynamicXml> Descendants(Func<XElement, bool> func)
        {
			return _inner.Descendants(func).Select(x => new DynamicXml(x.BaseElement)).ToList();   
        }
        public List<DynamicXml> DescendantsOrSelf()
        {
			return _inner.DescendantsOrSelf().Select(x => new DynamicXml(x.BaseElement)).ToList();   
        }
        public List<DynamicXml> DescendantsOrSelf(Func<XElement, bool> func)
        {
			return _inner.DescendantsOrSelf(func).Select(x => new DynamicXml(x.BaseElement)).ToList();   
        }
        public List<DynamicXml> Ancestors()
        {
			return _inner.Ancestors().Select(x => new DynamicXml(x.BaseElement)).ToList();   
        }
        public List<DynamicXml> Ancestors(Func<XElement, bool> func)
        {
			return _inner.Ancestors(func).Select(x => new DynamicXml(x.BaseElement)).ToList();   
        }
        public List<DynamicXml> AncestorsOrSelf()
        {
			return _inner.AncestorsOrSelf().Select(x => new DynamicXml(x.BaseElement)).ToList();  
        }
        public List<DynamicXml> AncestorsOrSelf(Func<XElement, bool> func)
        {
			return _inner.AncestorsOrSelf(func).Select(x => new DynamicXml(x.BaseElement)).ToList();  
        }

        public int Index()
        {
			return _inner.Index();  
        }

        public bool IsHelper(Func<DynamicXml, bool> test)
        {
            return test(this);
        }
        public HtmlString IsHelper(Func<DynamicXml, bool> test, string valueIfTrue)
        {
            return IsHelper(test, valueIfTrue, string.Empty);
        }
        public HtmlString IsHelper(Func<DynamicXml, bool> test, string valueIfTrue, string valueIfFalse)
        {
            return test(this) ? new HtmlString(valueIfTrue) : new HtmlString(valueIfFalse);
        }

        [Obsolete("Use XmlHelper.StripDashesInElementOrAttributeNames instead")]
        public static string StripDashesInElementOrAttributeNames(string xml)
        {
            return XmlHelper.StripDashesInElementOrAttributeNames(xml);
        }
    }
}
