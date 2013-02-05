using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Dynamic;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Collections;
using System.IO;
using System.Web;

namespace Umbraco.Core.Dynamics
{
	[TypeConverter(typeof(DynamicXmlConverter))]
	public class DynamicXml : DynamicObject, IEnumerable<DynamicXml>, IEnumerable<XElement>
    {
        public XElement BaseElement { get; set; }
	    private XElement _cleanedBaseElement;

        /// <summary>
        /// Returns a cleaned Xml element which is purely used for matching against names for elements and attributes
        /// when the normal names don't match, for example when the original names contain hyphens.
        /// </summary>
        /// <returns></returns>
	    private XElement GetCleanedBaseElement()
	    {
	        if (_cleanedBaseElement == null)
	        {
                if (BaseElement == null)
                    throw new InvalidOperationException("Cannot return a cleaned XML element when the BaseElement is null");
	            _cleanedBaseElement = XElement.Parse(XmlHelper.StripDashesInElementOrAttributeNames(BaseElement.ToString()));
	        }
	        return _cleanedBaseElement;
	    }

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

			//ok, now lets try to match by member, property, extensino method
			var attempt = DynamicInstanceHelper.TryInvokeMember(this, binder, args, new[]
				{
					typeof (IEnumerable<DynamicXml>),
					typeof (IEnumerable<XElement>),
					typeof (DynamicXml)
				});

			if (attempt.Success)
			{
				result = attempt.Result.ObjectResult;

				//need to check the return type and possibly cast if result is from an extension method found
				if (attempt.Result.Reason == DynamicInstanceHelper.TryInvokeMemberSuccessReason.FoundExtensionMethod)
				{
					//if the result is already a DynamicXml instance, then we do not need to cast
					if (attempt.Result.ObjectResult != null && !(attempt.Result.ObjectResult is DynamicXml))
					{
						if (attempt.Result.ObjectResult is XElement)
						{
							result = new DynamicXml((XElement)attempt.Result.ObjectResult);
						}
						else if (attempt.Result.ObjectResult is IEnumerable<XElement>)
						{
							result = ((IEnumerable<XElement>)attempt.Result.ObjectResult).Select(x => new DynamicXml(x));
						}
						else if (attempt.Result.ObjectResult is IEnumerable<DynamicXml>)
						{
							result = ((IEnumerable<DynamicXml>)attempt.Result.ObjectResult).Select(x => new DynamicXml(x.BaseElement));
						}
					}
				}
				return true;
			}

			//this is the result of an extension method execution gone wrong so we return dynamic null
			if (attempt.Result.Reason == DynamicInstanceHelper.TryInvokeMemberSuccessReason.FoundExtensionMethod
				&& attempt.Error != null && attempt.Error is TargetInvocationException)
			{
				result = new DynamicNull();
				return true;
			}

			result = null;
			return false;

        }
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (BaseElement == null || binder == null)
            {
                result = null;
                return false;
            }

            //First check for matching name including the 'cleaned' name (i.e. removal of hyphens, etc... )
            var elementByNameAttempt = CheckNodeNameMatch(binder.Name, BaseElement, true);
            if (elementByNameAttempt.Success)
            {
                if (HandleIEnumerableXElement(elementByNameAttempt.Result, out result))
                {
                    return true;
                }
            }
            
            //Ok, so no elements matched, so lets try attributes
            var attributeByNameAttempt = CheckAttributeNameMatch(binder.Name, BaseElement, true);
            if (attributeByNameAttempt.Success)
            {
                if (attributeByNameAttempt.Result.Count() > 1)
                {
                    //more than one attribute matched, lets return the collection
                    result = attributeByNameAttempt.Result;
                }
                else
                {
                    //only one attribute matched, lets just return it
                    result = attributeByNameAttempt.Result.FirstOrDefault(); 
                }
                return true; 
            }
           

            return base.TryGetMember(binder, out result);
        }

        private Attempt<IEnumerable<string>> CheckAttributeNameMatch(string name, XElement xmlElement, bool checkCleanedName)
        {
            var attributes = xmlElement.Attributes(name).Select(attr => attr.Value).ToArray();
            if (attributes.Any())
            {
                return new Attempt<IEnumerable<string>>(true, attributes);
            }

            //NOTE: this seems strange we're checking for the term 'root', this is legacy code so we'll keep it i guess
            if (!attributes.Any() && xmlElement.Name == "root" && xmlElement.Elements().Count() == 1)
            {
                //no elements matched and this node is called 'root' and only has one child... lets see if it matches.
                var childElements = xmlElement.Elements().ElementAt(0).Attributes(name).Select(attr => attr.Value).ToArray();
                if (childElements.Any())
                {
                    //we've found a match by the first child of an element called 'root' (strange, but sure)
                    return new Attempt<IEnumerable<string>>(true, childElements);
                }
            }

            if (checkCleanedName)
            {
                //still no match, we'll try to match with a 'cleaned' name
                var cleanedXml = GetCleanedBaseElement();

                //pass false in to this as we don't want an infinite loop and clean the already cleaned xml
                return CheckAttributeNameMatch(name, cleanedXml, false);  
            }

            //no deal
            return Attempt<IEnumerable<string>>.False;
	    }

	    /// <summary>
        /// Checks if the 'name' matches any elements of xmlElement
        /// </summary>
        /// <param name="name">The name to match</param>
        /// <param name="xmlElement">The xml element to check against</param>
        /// <param name="checkCleanedName">If there are no matches, we'll clean the xml (i.e. remove hyphens, etc..) and then retry</param>
        /// <returns></returns>
        private Attempt<IEnumerable<XElement>> CheckNodeNameMatch(string name, XElement xmlElement, bool checkCleanedName)
        {
            //Go ahead and try to fetch all of the elements matching the member name, and wrap them
            var elements = xmlElement.Elements(name).ToArray();

            //Check if we've got any matches, if so then return true
            if (elements.Any())
            {
                return new Attempt<IEnumerable<XElement>>(true, elements);
            }

            //NOTE: this seems strange we're checking for the term 'root', this is legacy code so we'll keep it i guess
            if (!elements.Any() && xmlElement.Name == "root" && xmlElement.Elements().Count() == 1)
            {
                //no elements matched and this node is called 'root' and only has one child... lets see if it matches.
                var childElements = xmlElement.Elements().ElementAt(0).Elements(name).ToArray();
                if (childElements.Any())
                {
                    //we've found a match by the first child of an element called 'root' (strange, but sure)
                    return new Attempt<IEnumerable<XElement>>(true, childElements);
                }
            }
            
            if (checkCleanedName)
            {
                //still no match, we'll try to match with a 'cleaned' name
                var cleanedXml = GetCleanedBaseElement();

                //pass false in to this as we don't want an infinite loop and clean the already cleaned xml
                return CheckNodeNameMatch(name, cleanedXml, false);                        
            }

            //no deal
            return Attempt<IEnumerable<XElement>>.False;
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
                    var root = new XElement(XName.Get("root"));
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

		/// <summary>
		/// Return the string version of Xml
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return ToXml();
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
			return ((IEnumerable<XElement>)this).Count();
        }

        public bool Any()
        {
			return ((IEnumerable<XElement>)this).Any();
        }

		public IEnumerable<DynamicXml> Take(int count)
		{
			return ((IEnumerable<DynamicXml>)this).Take(count);
		}

		public IEnumerable<DynamicXml> Skip(int count)
		{
			return ((IEnumerable<DynamicXml>)this).Skip(count);
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
            return IsHelper(n => ancestors.FirstOrDefault(ancestor => ancestor.BaseElement == other.BaseElement) != null);
        }
        public HtmlString IsDescendant(DynamicXml other, string valueIfTrue)
        {
            var ancestors = this.Ancestors();
			return IsHelper(n => ancestors.FirstOrDefault(ancestor => ancestor.BaseElement == other.BaseElement) != null, valueIfTrue);
        }
        public HtmlString IsDescendant(DynamicXml other, string valueIfTrue, string valueIfFalse)
        {
            var ancestors = this.Ancestors();
			return IsHelper(n => ancestors.FirstOrDefault(ancestor => ancestor.BaseElement == other.BaseElement) != null, valueIfTrue, valueIfFalse);
        }
        public bool IsDescendantOrSelf(DynamicXml other)
        {
            var ancestors = this.AncestorsOrSelf();
			return IsHelper(n => ancestors.FirstOrDefault(ancestor => ancestor.BaseElement == other.BaseElement) != null);
        }
        public HtmlString IsDescendantOrSelf(DynamicXml other, string valueIfTrue)
        {
            var ancestors = this.AncestorsOrSelf();
			return IsHelper(n => ancestors.FirstOrDefault(ancestor => ancestor.BaseElement == other.BaseElement) != null, valueIfTrue);
        }
        public HtmlString IsDescendantOrSelf(DynamicXml other, string valueIfTrue, string valueIfFalse)
        {
            var ancestors = this.AncestorsOrSelf();
			return IsHelper(n => ancestors.FirstOrDefault(ancestor => ancestor.BaseElement == other.BaseElement) != null, valueIfTrue, valueIfFalse);
        }
        public bool IsAncestor(DynamicXml other)
        {
            var descendants = this.Descendants();
			return IsHelper(n => descendants.FirstOrDefault(descendant => descendant.BaseElement == other.BaseElement) != null);
        }
        public HtmlString IsAncestor(DynamicXml other, string valueIfTrue)
        {
            var descendants = this.Descendants();
			return IsHelper(n => descendants.FirstOrDefault(descendant => descendant.BaseElement == other.BaseElement) != null, valueIfTrue);
        }
        public HtmlString IsAncestor(DynamicXml other, string valueIfTrue, string valueIfFalse)
        {
            var descendants = this.Descendants();
			return IsHelper(n => descendants.FirstOrDefault(descendant => descendant.BaseElement == other.BaseElement) != null, valueIfTrue, valueIfFalse);
        }
        public bool IsAncestorOrSelf(DynamicXml other)
        {
            var descendants = this.DescendantsOrSelf();
			return IsHelper(n => descendants.FirstOrDefault(descendant => descendant.BaseElement == other.BaseElement) != null);
        }
        public HtmlString IsAncestorOrSelf(DynamicXml other, string valueIfTrue)
        {
            var descendants = this.DescendantsOrSelf();
			return IsHelper(n => descendants.FirstOrDefault(descendant => descendant.BaseElement == other.BaseElement) != null, valueIfTrue);
        }
        public HtmlString IsAncestorOrSelf(DynamicXml other, string valueIfTrue, string valueIfFalse)
        {
            var descendants = this.DescendantsOrSelf();
			return IsHelper(n => descendants.FirstOrDefault(descendant => descendant.BaseElement == other.BaseElement) != null, valueIfTrue, valueIfFalse);
        }
        public IEnumerable<DynamicXml> Descendants()
        {
            return Descendants(n => true);
        }
		public IEnumerable<DynamicXml> Descendants(Func<XElement, bool> func)
        {
            var flattenedNodes = this.BaseElement.Elements().Map(func, (XElement n) => { return n.Elements(); });
            return flattenedNodes.ToList().ConvertAll(n => new DynamicXml(n));
        }
		public IEnumerable<DynamicXml> DescendantsOrSelf()
        {
            return DescendantsOrSelf(n => true);
        }
		public IEnumerable<DynamicXml> DescendantsOrSelf(Func<XElement, bool> func)
        {
            var flattenedNodes = this.BaseElement.Elements().Map(func, (XElement n) => { return n.Elements(); });
            var list = new List<DynamicXml>();
            list.Add(this);
            list.AddRange(flattenedNodes.ToList().ConvertAll(n => new DynamicXml(n)));
            return list;
        }
		public IEnumerable<DynamicXml> Ancestors()
        {
            return Ancestors(item => true);
        }
		public IEnumerable<DynamicXml> Ancestors(Func<XElement, bool> func)
        {
            var ancestorList = new List<XElement>();
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
		public IEnumerable<DynamicXml> AncestorsOrSelf()
        {
            return AncestorsOrSelf(item => true);
        }
		public IEnumerable<DynamicXml> AncestorsOrSelf(Func<XElement, bool> func)
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

        [Obsolete("Use XmlHelper.StripDashesInElementOrAttributeNames instead")]
        public static string StripDashesInElementOrAttributeNames(string xml)
        {
            return XmlHelper.StripDashesInElementOrAttributeNames(xml);
        }

	    
    }
}
