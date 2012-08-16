using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Collections;
using System.IO;
using System.Web;

namespace Umbraco.Core.Dynamics
{
    internal class DynamicXml : DynamicObject, IEnumerable
    {
        public XElement BaseElement { get; set; }

        public DynamicXml(XElement baseElement)
        {
            this.BaseElement = baseElement;
        }
        public DynamicXml(string xml)
        {
            var baseElement = XElement.Parse(xml);
            this.BaseElement = baseElement;
        }
        public DynamicXml(XPathNodeIterator xpni)
        {
            if (xpni != null)
            {
                if (xpni.Current != null)
                {
                    var xml = xpni.Current.OuterXml;
                    var baseElement = XElement.Parse(xml);
                    this.BaseElement = baseElement;
                }
            }
        }
        public string InnerText
        {
            get
            {
                return BaseElement.Value;
            }
        }
        public string ToXml()
        {
            return BaseElement.ToString(SaveOptions.DisableFormatting);
        }
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            int index = 0;
            if (indexes.Length > 0)
            {
                index = (int)indexes[0];
                result = new DynamicXml(this.BaseElement.Elements().ToList()[index]);
                return true;
            }
            return base.TryGetIndex(binder, indexes, out result);
        }
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (args.Length == 0 && binder.Name == "ToXml")
            {
                result = this.BaseElement.ToString();
                return true;
            }
            if (args.Length == 1 && binder.Name == "XPath")
            {
                var elements = this.BaseElement.XPathSelectElements(args[0].ToString());
                HandleIEnumerableXElement(elements, out result);
                return true; //anyway
            }
            return base.TryInvokeMember(binder, args, out result);
        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (BaseElement == null || binder == null)
            {
                result = null;
                return false;
            }
            //Go ahead and try to fetch all of the elements matching the member name, and wrap them
            var elements = BaseElement.Elements(binder.Name);
            if (elements.Count() == 0 && BaseElement.Name == "root" && BaseElement.Elements().Count() == 1)
            {
                //no elements matched, lets try first child
                elements = BaseElement.Elements().ElementAt(0).Elements(binder.Name);
            }
            if (HandleIEnumerableXElement(elements, out result))
            {
                return true;
            }
            else
            {

                //Ok, so no elements matched, so lets try attributes
                IEnumerable<string> attributes = BaseElement.Attributes(binder.Name).Select(attr => attr.Value);
                int count = attributes.Count();

                if (count > 0)
                {
                    if (count > 1)
                        result = attributes; //more than one attribute matched, lets return the collection
                    else
                        result = attributes.FirstOrDefault(); //only one attribute matched, lets just return it

                    return true; // return true because we matched
                }
                else
                {
                    //no attributes matched, lets try first child
                    if (BaseElement.Name == "root" && BaseElement.Elements().Count() == 1)
                    {
                        attributes = BaseElement.Elements().ElementAt(0).Attributes(binder.Name).Select(attr => attr.Value);
                        count = attributes.Count();
                        if (count > 1)
                            result = attributes; //more than one attribute matched, lets return the collection
                        else
                            result = attributes.FirstOrDefault(); //only one attribute matched, lets just return it

                        return true; // return true because we matched
                    }
                }
            }
            return base.TryGetMember(binder, out result);
        }

        private bool HandleIEnumerableXElement(IEnumerable<XElement> elements, out object result)
        {
            //Get the count now, so we don't have to call it twice
            int count = elements.Count();
            if (count > 0)
            {
                var firstElement = elements.FirstOrDefault();
                //we have a single element, does it have any children?
                if (firstElement != null && firstElement.Elements().Count() == 0 && !firstElement.HasAttributes)
                {
                    //no, return the text
                    result = firstElement.Value;
                    return true;
                }
                else
                {
                    //We have more than one matching element, so let's return the collection
                    //elements is IEnumerable<DynamicXml>
                    //but we want to be able to re-enter this code
                    XElement root = new XElement(XName.Get("root"));
                    root.Add(elements);
                    result = new DynamicXml(root);

                    //From here, you'll either end up back here (because you have <root><node><node>)
                    //or you use [] indexing and you end up with a single element
                    return true;
                }
            }
            result = null;
            return false;
        }
        public DynamicXml XPath(string expression)
        {
            var matched = this.BaseElement.XPathSelectElements(expression);
            DynamicXml root = new DynamicXml("<results/>");
            foreach (var element in matched)
            {
                root.BaseElement.Add(element);
            }
            return root;
        }
        public IHtmlString ToHtml()
        {
            return new HtmlString(this.ToXml());
        }
        public DynamicXml Find(string expression)
        {
            return new DynamicXml(this.BaseElement.XPathSelectElements(expression).FirstOrDefault());
        }

        public DynamicXml Find(string attributeName, object value)
        {
            string expression = string.Format("//*[{0}='{1}']", attributeName, value);
            return new DynamicXml(this.BaseElement.XPathSelectElements(expression).FirstOrDefault());
        }

        public IEnumerator GetEnumerator()
        {
            return this.BaseElement.Elements().Select(e => new DynamicXml(e)).GetEnumerator();
        }
        public int Count()
        {
            return this.BaseElement.Elements().Count();
        }

        public bool IsNull()
        {
            return false;
        }
        public bool HasValue()
        {
            return true;
        }

        public bool IsFirst()
        {
            return IsHelper(n => n.Index() == 0);
        }
        public HtmlString IsFirst(string valueIfTrue)
        {
            return IsHelper(n => n.Index() == 0, valueIfTrue);
        }
        public HtmlString IsFirst(string valueIfTrue, string valueIfFalse)
        {
            return IsHelper(n => n.Index() == 0, valueIfTrue, valueIfFalse);
        }
        public bool IsNotFirst()
        {
            return !IsHelper(n => n.Index() == 0);
        }
        public HtmlString IsNotFirst(string valueIfTrue)
        {
            return IsHelper(n => n.Index() != 0, valueIfTrue);
        }
        public HtmlString IsNotFirst(string valueIfTrue, string valueIfFalse)
        {
            return IsHelper(n => n.Index() != 0, valueIfTrue, valueIfFalse);
        }
        public bool IsPosition(int index)
        {
            if (this.BaseElement == null || this.BaseElement.Parent == null)
            {
                return false;
            }
            return IsHelper(n => n.Index() == index);
        }
        public HtmlString IsPosition(int index, string valueIfTrue)
        {
            if (this.BaseElement == null || this.BaseElement.Parent == null)
            {
                return new HtmlString(string.Empty);
            }
            return IsHelper(n => n.Index() == index, valueIfTrue);
        }
        public HtmlString IsPosition(int index, string valueIfTrue, string valueIfFalse)
        {
            if (this.BaseElement == null || this.BaseElement.Parent == null)
            {
                return new HtmlString(valueIfFalse);
            }
            return IsHelper(n => n.Index() == index, valueIfTrue, valueIfFalse);
        }
        public bool IsModZero(int modulus)
        {
            if (this.BaseElement == null || this.BaseElement.Parent == null)
            {
                return false;
            }
            return IsHelper(n => n.Index() % modulus == 0);
        }
        public HtmlString IsModZero(int modulus, string valueIfTrue)
        {
            if (this.BaseElement == null || this.BaseElement.Parent == null)
            {
                return new HtmlString(string.Empty);
            }
            return IsHelper(n => n.Index() % modulus == 0, valueIfTrue);
        }
        public HtmlString IsModZero(int modulus, string valueIfTrue, string valueIfFalse)
        {
            if (this.BaseElement == null || this.BaseElement.Parent == null)
            {
                return new HtmlString(valueIfFalse);
            }
            return IsHelper(n => n.Index() % modulus == 0, valueIfTrue, valueIfFalse);
        }

        public bool IsNotModZero(int modulus)
        {
            if (this.BaseElement == null || this.BaseElement.Parent == null)
            {
                return false;
            }
            return IsHelper(n => n.Index() % modulus != 0);
        }
        public HtmlString IsNotModZero(int modulus, string valueIfTrue)
        {
            if (this.BaseElement == null || this.BaseElement.Parent == null)
            {
                return new HtmlString(string.Empty);
            }
            return IsHelper(n => n.Index() % modulus != 0, valueIfTrue);
        }
        public HtmlString IsNotModZero(int modulus, string valueIfTrue, string valueIfFalse)
        {
            if (this.BaseElement == null || this.BaseElement.Parent == null)
            {
                return new HtmlString(valueIfFalse);
            }
            return IsHelper(n => n.Index() % modulus != 0, valueIfTrue, valueIfFalse);
        }
        public bool IsNotPosition(int index)
        {
            if (this.BaseElement == null || this.BaseElement.Parent == null)
            {
                return false;
            }
            return !IsHelper(n => n.Index() == index);
        }
        public HtmlString IsNotPosition(int index, string valueIfTrue)
        {
            if (this.BaseElement == null || this.BaseElement.Parent == null)
            {
                return new HtmlString(string.Empty);
            }
            return IsHelper(n => n.Index() != index, valueIfTrue);
        }
        public HtmlString IsNotPosition(int index, string valueIfTrue, string valueIfFalse)
        {
            if (this.BaseElement == null || this.BaseElement.Parent == null)
            {
                return new HtmlString(valueIfFalse);
            }
            return IsHelper(n => n.Index() != index, valueIfTrue, valueIfFalse);
        }
        public bool IsLast()
        {
            if (this.BaseElement == null || this.BaseElement.Parent == null)
            {
                return false;
            }
            int count = this.BaseElement.Parent.Elements().Count();
            return IsHelper(n => n.Index() == count - 1);
        }
        public HtmlString IsLast(string valueIfTrue)
        {
            if (this.BaseElement == null || this.BaseElement.Parent == null)
            {
                return new HtmlString(string.Empty);
            }
            int count = this.BaseElement.Parent.Elements().Count();
            return IsHelper(n => n.Index() == count - 1, valueIfTrue);
        }
        public HtmlString IsLast(string valueIfTrue, string valueIfFalse)
        {
            if (this.BaseElement == null || this.BaseElement.Parent == null)
            {
                return new HtmlString(valueIfFalse);
            }
            int count = this.BaseElement.Parent.Elements().Count();
            return IsHelper(n => n.Index() == count - 1, valueIfTrue, valueIfFalse);
        }
        public bool IsNotLast()
        {
            if (this.BaseElement == null || this.BaseElement.Parent == null)
            {
                return false;
            }
            int count = this.BaseElement.Parent.Elements().Count();
            return !IsHelper(n => n.Index() == count - 1);
        }
        public HtmlString IsNotLast(string valueIfTrue)
        {
            if (this.BaseElement == null || this.BaseElement.Parent == null)
            {
                return new HtmlString(string.Empty);
            }
            int count = this.BaseElement.Parent.Elements().Count();
            return IsHelper(n => n.Index() != count - 1, valueIfTrue);
        }
        public HtmlString IsNotLast(string valueIfTrue, string valueIfFalse)
        {
            if (this.BaseElement == null || this.BaseElement.Parent == null)
            {
                return new HtmlString(valueIfFalse);
            }
            int count = this.BaseElement.Parent.Elements().Count();
            return IsHelper(n => n.Index() != count - 1, valueIfTrue, valueIfFalse);
        }
        public bool IsEven()
        {
            return IsHelper(n => n.Index() % 2 == 0);
        }
        public HtmlString IsEven(string valueIfTrue)
        {
            return IsHelper(n => n.Index() % 2 == 0, valueIfTrue);
        }
        public HtmlString IsEven(string valueIfTrue, string valueIfFalse)
        {
            return IsHelper(n => n.Index() % 2 == 0, valueIfTrue, valueIfFalse);
        }
        public bool IsOdd()
        {
            return IsHelper(n => n.Index() % 2 == 1);
        }
        public HtmlString IsOdd(string valueIfTrue)
        {
            return IsHelper(n => n.Index() % 2 == 1, valueIfTrue);
        }
        public HtmlString IsOdd(string valueIfTrue, string valueIfFalse)
        {
            return IsHelper(n => n.Index() % 2 == 1, valueIfTrue, valueIfFalse);
        }
        public bool IsEqual(DynamicXml other)
        {
            return IsHelper(n => n.BaseElement == other.BaseElement);
        }
        public HtmlString IsEqual(DynamicXml other, string valueIfTrue)
        {
            return IsHelper(n => n.BaseElement == other.BaseElement, valueIfTrue);
        }
        public HtmlString IsEqual(DynamicXml other, string valueIfTrue, string valueIfFalse)
        {
            return IsHelper(n => n.BaseElement == other.BaseElement, valueIfTrue, valueIfFalse);
        }
        public bool IsNotEqual(DynamicXml other)
        {
            return IsHelper(n => n.BaseElement != other.BaseElement);
        }
        public HtmlString IsNotEqual(DynamicXml other, string valueIfTrue)
        {
            return IsHelper(n => n.BaseElement != other.BaseElement, valueIfTrue);
        }
        public HtmlString IsNotEqual(DynamicXml other, string valueIfTrue, string valueIfFalse)
        {
            return IsHelper(n => n.BaseElement != other.BaseElement, valueIfTrue, valueIfFalse);
        }
        public bool IsDescendant(DynamicXml other)
        {
            var ancestors = this.Ancestors();
            return IsHelper(n => ancestors.Find(ancestor => ancestor.BaseElement == other.BaseElement) != null);
        }
        public HtmlString IsDescendant(DynamicXml other, string valueIfTrue)
        {
            var ancestors = this.Ancestors();
            return IsHelper(n => ancestors.Find(ancestor => ancestor.BaseElement == other.BaseElement) != null, valueIfTrue);
        }
        public HtmlString IsDescendant(DynamicXml other, string valueIfTrue, string valueIfFalse)
        {
            var ancestors = this.Ancestors();
            return IsHelper(n => ancestors.Find(ancestor => ancestor.BaseElement == other.BaseElement) != null, valueIfTrue, valueIfFalse);
        }
        public bool IsDescendantOrSelf(DynamicXml other)
        {
            var ancestors = this.AncestorsOrSelf();
            return IsHelper(n => ancestors.Find(ancestor => ancestor.BaseElement == other.BaseElement) != null);
        }
        public HtmlString IsDescendantOrSelf(DynamicXml other, string valueIfTrue)
        {
            var ancestors = this.AncestorsOrSelf();
            return IsHelper(n => ancestors.Find(ancestor => ancestor.BaseElement == other.BaseElement) != null, valueIfTrue);
        }
        public HtmlString IsDescendantOrSelf(DynamicXml other, string valueIfTrue, string valueIfFalse)
        {
            var ancestors = this.AncestorsOrSelf();
            return IsHelper(n => ancestors.Find(ancestor => ancestor.BaseElement == other.BaseElement) != null, valueIfTrue, valueIfFalse);
        }
        public bool IsAncestor(DynamicXml other)
        {
            var descendants = this.Descendants();
            return IsHelper(n => descendants.Find(descendant => descendant.BaseElement == other.BaseElement) != null);
        }
        public HtmlString IsAncestor(DynamicXml other, string valueIfTrue)
        {
            var descendants = this.Descendants();
            return IsHelper(n => descendants.Find(descendant => descendant.BaseElement == other.BaseElement) != null, valueIfTrue);
        }
        public HtmlString IsAncestor(DynamicXml other, string valueIfTrue, string valueIfFalse)
        {
            var descendants = this.Descendants();
            return IsHelper(n => descendants.Find(descendant => descendant.BaseElement == other.BaseElement) != null, valueIfTrue, valueIfFalse);
        }
        public bool IsAncestorOrSelf(DynamicXml other)
        {
            var descendants = this.DescendantsOrSelf();
            return IsHelper(n => descendants.Find(descendant => descendant.BaseElement == other.BaseElement) != null);
        }
        public HtmlString IsAncestorOrSelf(DynamicXml other, string valueIfTrue)
        {
            var descendants = this.DescendantsOrSelf();
            return IsHelper(n => descendants.Find(descendant => descendant.BaseElement == other.BaseElement) != null, valueIfTrue);
        }
        public HtmlString IsAncestorOrSelf(DynamicXml other, string valueIfTrue, string valueIfFalse)
        {
            var descendants = this.DescendantsOrSelf();
            return IsHelper(n => descendants.Find(descendant => descendant.BaseElement == other.BaseElement) != null, valueIfTrue, valueIfFalse);
        }
        public List<DynamicXml> Descendants()
        {
            return Descendants(n => true);
        }
        public List<DynamicXml> Descendants(Func<XElement, bool> func)
        {
            var flattenedNodes = this.BaseElement.Elements().Map(func, (XElement n) => { return n.Elements(); });
            return flattenedNodes.ToList().ConvertAll(n => new DynamicXml(n));
        }
        public List<DynamicXml> DescendantsOrSelf()
        {
            return DescendantsOrSelf(n => true);
        }
        public List<DynamicXml> DescendantsOrSelf(Func<XElement, bool> func)
        {
            var flattenedNodes = this.BaseElement.Elements().Map(func, (XElement n) => { return n.Elements(); });
            var list = new List<DynamicXml>();
            list.Add(this);
            list.AddRange(flattenedNodes.ToList().ConvertAll(n => new DynamicXml(n)));
            return list;
        }
        public List<DynamicXml> Ancestors()
        {
            return Ancestors(item => true);
        }
        public List<DynamicXml> Ancestors(Func<XElement, bool> func)
        {
            List<XElement> ancestorList = new List<XElement>();
            var node = this.BaseElement;
            while (node != null)
            {
                if (node.Parent == null) break;
                XElement parent = node.Parent;
                if (parent != null)
                {
                    if (this.BaseElement != parent)
                    {
                        node = parent;
                        if (func(node))
                        {
                            ancestorList.Add(node);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            ancestorList.Reverse();
            return ancestorList.ConvertAll(item => new DynamicXml(item));
        }
        public List<DynamicXml> AncestorsOrSelf()
        {
            return AncestorsOrSelf(item => true);
        }
        public List<DynamicXml> AncestorsOrSelf(Func<XElement, bool> func)
        {
            List<XElement> ancestorList = new List<XElement>();
            var node = this.BaseElement;
            ancestorList.Add(node);
            while (node != null)
            {
                if (node.Parent == null) break;
                XElement parent = node.Parent;
                if (parent != null)
                {
                    if (this.BaseElement != parent)
                    {
                        node = parent;
                        if (func(node))
                        {
                            ancestorList.Add(node);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            ancestorList.Reverse();
            return ancestorList.ConvertAll(item => new DynamicXml(item));
        }

        public int Index()
        {
            if (this.BaseElement != null && this.BaseElement.Parent != null)
            {
                var elements = this.BaseElement.Parent.Elements();
                int index = 0;
                foreach (var element in elements)
                {
                    if (element == this.BaseElement) break;
                    index++;
                }
                return index;
            }
            return 0;
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

        public static string StripDashesInElementOrAttributeNames(string xml)
        {
            using (MemoryStream outputms = new MemoryStream())
            {
                using (TextWriter outputtw = new StreamWriter(outputms))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (TextWriter tw = new StreamWriter(ms))
                        {
                            tw.Write(xml);
                            tw.Flush();
                            ms.Position = 0;
                            using (TextReader tr = new StreamReader(ms))
                            {
                                bool IsInsideElement = false, IsInsideQuotes = false;
                                int ic = 0;
                                while ((ic = tr.Read()) != -1)
                                {
                                    if (ic == (int)'<' && !IsInsideQuotes)
                                    {
                                        if (tr.Peek() != (int)'!')
                                        {
                                            IsInsideElement = true;
                                        }
                                    }
                                    if (ic == (int)'>' && !IsInsideQuotes)
                                    {
                                        IsInsideElement = false;
                                    }
                                    if (ic == (int)'"')
                                    {
                                        IsInsideQuotes = !IsInsideQuotes;
                                    }
                                    if (!IsInsideElement || ic != (int)'-' || IsInsideQuotes)
                                    {
                                        outputtw.Write((char)ic);
                                    }
                                }

                            }
                        }
                    }
                    outputtw.Flush();
                    outputms.Position = 0;
                    using (TextReader outputtr = new StreamReader(outputms))
                    {
                        return outputtr.ReadToEnd();
                    }
                }
            }
        }
    }
}
