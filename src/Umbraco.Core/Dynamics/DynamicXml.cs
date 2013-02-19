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
using Umbraco.Core;

namespace Umbraco.Core.Dynamics
{
	[TypeConverter(typeof(DynamicXmlConverter))]
	public class DynamicXml : DynamicObject, IEnumerable<DynamicXml>, IEnumerable<XElement>
    {
	    [Obsolete("Use StrippedXmlElement or RawXmlElement instead")]
	    public XElement BaseElement
	    {
	        get { return StrippedXmlElement; }
            //This should have been read only but we're stuck with it now and we'll have to assume that a person is setting the 'raw' xml
            set
            {
                Mandate.ParameterNotNull(value, "value");

                RawXmlElement = value;
                StrippedXmlElement = XElement.Parse(
                XmlHelper.StripDashesInElementOrAttributeNames(
                    RawXmlElement.ToString()));
            }
	    }

	    /// <summary>
        /// Returns the XElement used to create the DynamicXml structure
        /// </summary>
        public XElement RawXmlElement { get; internal set; }

        /// <summary>
        /// Returns the XElement for this DynamicXml structure that has been stripped of all of it's hyphens
        /// </summary>
        public XElement StrippedXmlElement { get; private set; }

        public DynamicXml(XElement baseElement)
        {
            if (baseElement == null) return;

            RawXmlElement = baseElement;
            StrippedXmlElement = XElement.Parse(
                XmlHelper.StripDashesInElementOrAttributeNames(
                    RawXmlElement.ToString()));
        }

        internal DynamicXml(XElement rawXml, XElement strippedXml)
        {
            if (rawXml == null) throw new ArgumentNullException("rawXml");
            if (strippedXml == null) throw new ArgumentNullException("strippedXml");

            RawXmlElement = rawXml;
            StrippedXmlElement = strippedXml;
        }

	    public DynamicXml(string xml)
        {
            if (xml.IsNullOrWhiteSpace()) return;

            var baseElement = XElement.Parse(xml);
            RawXmlElement = baseElement;
	        StrippedXmlElement = XElement.Parse(
	            XmlHelper.StripDashesInElementOrAttributeNames(
	                xml));
        }
        public DynamicXml(XPathNodeIterator xpni)
        {
            if (xpni == null) return;
            if (xpni.Current == null) return;

            //TODO: OuterXml is really bad for performance! Should actually use the XPathNodeIterator api
            var xml = xpni.Current.OuterXml;                    
            var baseElement = XElement.Parse(xml);
            RawXmlElement = baseElement;
            StrippedXmlElement = XElement.Parse(
                XmlHelper.StripDashesInElementOrAttributeNames(
                    xml));
        }

	    /// <summary>
        /// Returns the InnertText based on the stripped xml element
        /// </summary>
        public string InnerText
        {
            get
            {
                return StrippedXmlElement.Value;
            }
        }

        /// <summary>
        /// Returns the string representation of the dash-stripped xml element
        /// </summary>
        /// <returns></returns>
        public string ToXml()
        {
            return StrippedXmlElement.ToString(SaveOptions.DisableFormatting);
        }

        /// <summary>
        /// Returns the string representation of the raw (no dash stripping) xml element
        /// </summary>
        /// <returns></returns>
        public string ToRawXml()
        {
            return RawXmlElement.ToString(SaveOptions.DisableFormatting);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            int index = 0;
            if (indexes.Length > 0)
            {
                index = (int)indexes[0];
                result = new DynamicXml(
                    RawXmlElement.Elements().ElementAt(index),
                    StrippedXmlElement.Elements().ElementAt(index));
                return true;
            }
            return base.TryGetIndex(binder, indexes, out result);
        }
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (args.Length == 0 && binder.Name == "ToXml")
            {
                result = StrippedXmlElement.ToString();
                return true;
            }
            if (args.Length == 1 && binder.Name == "XPath")
            {
                var elements = StrippedXmlElement.XPathSelectElements(args[0].ToString());
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
						    result = ((IEnumerable<DynamicXml>) attempt.Result.ObjectResult).Select(x => new DynamicXml(
						                                                                                     x.RawXmlElement,
						                                                                                     x.StrippedXmlElement));
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
            if (RawXmlElement == null || binder == null)
            {
                result = null;
                return false;
            }

            //First check for matching name including the 'cleaned' name (i.e. removal of hyphens, etc... )
            var elementByNameAttempt = CheckNodeNameMatch(binder.Name, RawXmlElement, true);
            if (elementByNameAttempt.Success)
            {
                if (HandleIEnumerableXElement(elementByNameAttempt.Result, out result))
                {
                    return true;
                }
            }
            
            //Ok, so no elements matched, so lets try attributes
            var attributeByNameAttempt = CheckAttributeNameMatch(binder.Name, RawXmlElement, true);
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
                var cleanedXml = StrippedXmlElement;

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
                var cleanedXml = StrippedXmlElement;

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

        /// <summary>
        /// Executes an XPath expression over the dash-stripped Xml
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public DynamicXml XPath(string expression)
        {
            var matched = StrippedXmlElement.XPathSelectElements(expression);
            var root = new DynamicXml("<results/>");
            foreach (var element in matched)
            {
                root.StrippedXmlElement.Add(element);
            }
            return root;
        }

		/// <summary>
		/// Return the string version of the dash-stripped Xml
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
            return new DynamicXml(StrippedXmlElement.XPathSelectElements(expression).FirstOrDefault());
        }

        public DynamicXml Find(string attributeName, object value)
        {
            string expression = string.Format("//*[{0}='{1}']", attributeName, value);
            return new DynamicXml(StrippedXmlElement.XPathSelectElements(expression).FirstOrDefault());
        }

	    IEnumerator<XElement> IEnumerable<XElement>.GetEnumerator()
	    {
            return StrippedXmlElement.Elements().GetEnumerator();
	    }

	    public IEnumerator<DynamicXml> GetEnumerator()
	    {
            return StrippedXmlElement.Elements().Select(e => new DynamicXml(e)).GetEnumerator();
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
            if (StrippedXmlElement == null || StrippedXmlElement.Parent == null)
            {
                return false;
            }
            return IsHelper(n => n.Index() == index);
        }
        public HtmlString IsPosition(int index, string valueIfTrue)
        {
            if (StrippedXmlElement == null || StrippedXmlElement.Parent == null)
            {
                return new HtmlString(string.Empty);
            }
            return IsHelper(n => n.Index() == index, valueIfTrue);
        }
        public HtmlString IsPosition(int index, string valueIfTrue, string valueIfFalse)
        {
            if (StrippedXmlElement == null || StrippedXmlElement.Parent == null)
            {
                return new HtmlString(valueIfFalse);
            }
            return IsHelper(n => n.Index() == index, valueIfTrue, valueIfFalse);
        }
        public bool IsModZero(int modulus)
        {
            if (StrippedXmlElement == null || StrippedXmlElement.Parent == null)
            {
                return false;
            }
            return IsHelper(n => n.Index() % modulus == 0);
        }
        public HtmlString IsModZero(int modulus, string valueIfTrue)
        {
            if (StrippedXmlElement == null || StrippedXmlElement.Parent == null)
            {
                return new HtmlString(string.Empty);
            }
            return IsHelper(n => n.Index() % modulus == 0, valueIfTrue);
        }
        public HtmlString IsModZero(int modulus, string valueIfTrue, string valueIfFalse)
        {
            if (StrippedXmlElement == null || StrippedXmlElement.Parent == null)
            {
                return new HtmlString(valueIfFalse);
            }
            return IsHelper(n => n.Index() % modulus == 0, valueIfTrue, valueIfFalse);
        }

        public bool IsNotModZero(int modulus)
        {
            if (StrippedXmlElement == null || StrippedXmlElement.Parent == null)
            {
                return false;
            }
            return IsHelper(n => n.Index() % modulus != 0);
        }
        public HtmlString IsNotModZero(int modulus, string valueIfTrue)
        {
            if (StrippedXmlElement == null || this.StrippedXmlElement.Parent == null)
            {
                return new HtmlString(string.Empty);
            }
            return IsHelper(n => n.Index() % modulus != 0, valueIfTrue);
        }
        public HtmlString IsNotModZero(int modulus, string valueIfTrue, string valueIfFalse)
        {
            if (this.StrippedXmlElement == null || this.StrippedXmlElement.Parent == null)
            {
                return new HtmlString(valueIfFalse);
            }
            return IsHelper(n => n.Index() % modulus != 0, valueIfTrue, valueIfFalse);
        }
        public bool IsNotPosition(int index)
        {
            if (this.StrippedXmlElement == null || this.StrippedXmlElement.Parent == null)
            {
                return false;
            }
            return !IsHelper(n => n.Index() == index);
        }
        public HtmlString IsNotPosition(int index, string valueIfTrue)
        {
            if (this.StrippedXmlElement == null || this.StrippedXmlElement.Parent == null)
            {
                return new HtmlString(string.Empty);
            }
            return IsHelper(n => n.Index() != index, valueIfTrue);
        }
        public HtmlString IsNotPosition(int index, string valueIfTrue, string valueIfFalse)
        {
            if (this.StrippedXmlElement == null || this.StrippedXmlElement.Parent == null)
            {
                return new HtmlString(valueIfFalse);
            }
            return IsHelper(n => n.Index() != index, valueIfTrue, valueIfFalse);
        }
        public bool IsLast()
        {
            if (this.StrippedXmlElement == null || this.StrippedXmlElement.Parent == null)
            {
                return false;
            }
            int count = this.StrippedXmlElement.Parent.Elements().Count();
            return IsHelper(n => n.Index() == count - 1);
        }
        public HtmlString IsLast(string valueIfTrue)
        {
            if (this.StrippedXmlElement == null || this.StrippedXmlElement.Parent == null)
            {
                return new HtmlString(string.Empty);
            }
            int count = this.StrippedXmlElement.Parent.Elements().Count();
            return IsHelper(n => n.Index() == count - 1, valueIfTrue);
        }
        public HtmlString IsLast(string valueIfTrue, string valueIfFalse)
        {
            if (this.StrippedXmlElement == null || this.StrippedXmlElement.Parent == null)
            {
                return new HtmlString(valueIfFalse);
            }
            int count = this.StrippedXmlElement.Parent.Elements().Count();
            return IsHelper(n => n.Index() == count - 1, valueIfTrue, valueIfFalse);
        }
        public bool IsNotLast()
        {
            if (this.StrippedXmlElement == null || this.StrippedXmlElement.Parent == null)
            {
                return false;
            }
            int count = this.StrippedXmlElement.Parent.Elements().Count();
            return !IsHelper(n => n.Index() == count - 1);
        }
        public HtmlString IsNotLast(string valueIfTrue)
        {
            if (this.StrippedXmlElement == null || this.StrippedXmlElement.Parent == null)
            {
                return new HtmlString(string.Empty);
            }
            int count = this.StrippedXmlElement.Parent.Elements().Count();
            return IsHelper(n => n.Index() != count - 1, valueIfTrue);
        }
        public HtmlString IsNotLast(string valueIfTrue, string valueIfFalse)
        {
            if (this.StrippedXmlElement == null || this.StrippedXmlElement.Parent == null)
            {
                return new HtmlString(valueIfFalse);
            }
            int count = this.StrippedXmlElement.Parent.Elements().Count();
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
            return IsHelper(n => n.StrippedXmlElement == other.StrippedXmlElement);
        }
        public HtmlString IsEqual(DynamicXml other, string valueIfTrue)
        {
            return IsHelper(n => n.StrippedXmlElement == other.StrippedXmlElement, valueIfTrue);
        }
        public HtmlString IsEqual(DynamicXml other, string valueIfTrue, string valueIfFalse)
        {
            return IsHelper(n => n.StrippedXmlElement == other.StrippedXmlElement, valueIfTrue, valueIfFalse);
        }
        public bool IsNotEqual(DynamicXml other)
        {
            return IsHelper(n => n.StrippedXmlElement != other.StrippedXmlElement);
        }
        public HtmlString IsNotEqual(DynamicXml other, string valueIfTrue)
        {
            return IsHelper(n => n.StrippedXmlElement != other.StrippedXmlElement, valueIfTrue);
        }
        public HtmlString IsNotEqual(DynamicXml other, string valueIfTrue, string valueIfFalse)
        {
            return IsHelper(n => n.StrippedXmlElement != other.StrippedXmlElement, valueIfTrue, valueIfFalse);
        }
        public bool IsDescendant(DynamicXml other)
        {
            var ancestors = this.Ancestors();
            return IsHelper(n => ancestors.FirstOrDefault(ancestor => ancestor.StrippedXmlElement == other.StrippedXmlElement) != null);
        }
        public HtmlString IsDescendant(DynamicXml other, string valueIfTrue)
        {
            var ancestors = this.Ancestors();
			return IsHelper(n => ancestors.FirstOrDefault(ancestor => ancestor.StrippedXmlElement == other.StrippedXmlElement) != null, valueIfTrue);
        }
        public HtmlString IsDescendant(DynamicXml other, string valueIfTrue, string valueIfFalse)
        {
            var ancestors = this.Ancestors();
			return IsHelper(n => ancestors.FirstOrDefault(ancestor => ancestor.StrippedXmlElement == other.StrippedXmlElement) != null, valueIfTrue, valueIfFalse);
        }
        public bool IsDescendantOrSelf(DynamicXml other)
        {
            var ancestors = this.AncestorsOrSelf();
			return IsHelper(n => ancestors.FirstOrDefault(ancestor => ancestor.StrippedXmlElement == other.StrippedXmlElement) != null);
        }
        public HtmlString IsDescendantOrSelf(DynamicXml other, string valueIfTrue)
        {
            var ancestors = this.AncestorsOrSelf();
			return IsHelper(n => ancestors.FirstOrDefault(ancestor => ancestor.StrippedXmlElement == other.StrippedXmlElement) != null, valueIfTrue);
        }
        public HtmlString IsDescendantOrSelf(DynamicXml other, string valueIfTrue, string valueIfFalse)
        {
            var ancestors = this.AncestorsOrSelf();
			return IsHelper(n => ancestors.FirstOrDefault(ancestor => ancestor.StrippedXmlElement == other.StrippedXmlElement) != null, valueIfTrue, valueIfFalse);
        }
        public bool IsAncestor(DynamicXml other)
        {
            var descendants = this.Descendants();
			return IsHelper(n => descendants.FirstOrDefault(descendant => descendant.StrippedXmlElement == other.StrippedXmlElement) != null);
        }
        public HtmlString IsAncestor(DynamicXml other, string valueIfTrue)
        {
            var descendants = this.Descendants();
			return IsHelper(n => descendants.FirstOrDefault(descendant => descendant.StrippedXmlElement == other.StrippedXmlElement) != null, valueIfTrue);
        }
        public HtmlString IsAncestor(DynamicXml other, string valueIfTrue, string valueIfFalse)
        {
            var descendants = this.Descendants();
			return IsHelper(n => descendants.FirstOrDefault(descendant => descendant.StrippedXmlElement == other.StrippedXmlElement) != null, valueIfTrue, valueIfFalse);
        }
        public bool IsAncestorOrSelf(DynamicXml other)
        {
            var descendants = this.DescendantsOrSelf();
			return IsHelper(n => descendants.FirstOrDefault(descendant => descendant.StrippedXmlElement == other.StrippedXmlElement) != null);
        }
        public HtmlString IsAncestorOrSelf(DynamicXml other, string valueIfTrue)
        {
            var descendants = this.DescendantsOrSelf();
			return IsHelper(n => descendants.FirstOrDefault(descendant => descendant.StrippedXmlElement == other.StrippedXmlElement) != null, valueIfTrue);
        }
        public HtmlString IsAncestorOrSelf(DynamicXml other, string valueIfTrue, string valueIfFalse)
        {
            var descendants = this.DescendantsOrSelf();
			return IsHelper(n => descendants.FirstOrDefault(descendant => descendant.StrippedXmlElement == other.StrippedXmlElement) != null, valueIfTrue, valueIfFalse);
        }
        public IEnumerable<DynamicXml> Descendants()
        {
            return Descendants(n => true);
        }
		public IEnumerable<DynamicXml> Descendants(Func<XElement, bool> func)
        {
            var flattenedNodes = this.StrippedXmlElement.Elements().Map(func, n => n.Elements());
            return flattenedNodes.ToList().ConvertAll(n => new DynamicXml(n));
        }
		public IEnumerable<DynamicXml> DescendantsOrSelf()
        {
            return DescendantsOrSelf(n => true);
        }
		public IEnumerable<DynamicXml> DescendantsOrSelf(Func<XElement, bool> func)
        {
            var flattenedNodes = this.StrippedXmlElement.Elements().Map(func, n => n.Elements());
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
            var node = this.StrippedXmlElement;
            while (node != null)
            {
                if (node.Parent == null) break;
                XElement parent = node.Parent;
                if (parent != null)
                {
                    if (this.StrippedXmlElement != parent)
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
            var node = this.StrippedXmlElement;
            ancestorList.Add(node);
            while (node != null)
            {
                if (node.Parent == null) break;
                XElement parent = node.Parent;
                if (parent != null)
                {
                    if (this.StrippedXmlElement != parent)
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
            if (this.StrippedXmlElement != null && this.StrippedXmlElement.Parent != null)
            {
                var elements = this.StrippedXmlElement.Parent.Elements();
                int index = 0;
                foreach (var element in elements)
                {
                    if (element == this.StrippedXmlElement) break;
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
