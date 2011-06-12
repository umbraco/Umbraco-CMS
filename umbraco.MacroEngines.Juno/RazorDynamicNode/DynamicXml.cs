using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Collections;

namespace umbraco.MacroEngines
{
    public class DynamicXml : DynamicObject, IEnumerable
    {
        public XElement BaseElement { get; set; }

        public DynamicXml(XElement baseElement)
        {
            this.BaseElement = baseElement;
        }
        public string InnerText
        {
            get
            {
                return BaseElement.Value;
            }
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

            //Go ahead and try to fetch all of the elements matching the member name, and wrap them
            var elements = BaseElement.Elements(binder.Name);

            if (HandleIEnumerableXElement(elements, out result))
            {
                return true;
            }
            else
            {

                //Ok, so no elements matched, so lets try attributes
                var attributes = BaseElement.Attributes(binder.Name).Select(attr => attr.Value);
                int count = attributes.Count();

                if (count > 0)
                {
                    if (count > 1)
                        result = attributes; //more than one attribute matched, lets return the collection
                    else
                        result = attributes.FirstOrDefault(); //only one attribute matched, lets just return it

                    return true; // return true because we matched
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
                if (count > 1)
                {
                    //result = elements; 
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
                else
                {
                    var firstElement = elements.FirstOrDefault();
                    //we have a single element, does it have any children?
                    if (firstElement.Elements().Count() == 0)
                    {
                        //no, return the text
                        result = firstElement.Value;
                        return true;
                    }
                    else
                    {
                        //yes return this element wrapped in DynamicXml
                        result = new DynamicXml(firstElement);
                        //There is only one matching element, so let's just return it
                        return true;
                    }
                }
                return true; //return true cuz we matched
            }
            result = null;
            return false;
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
    }
}
