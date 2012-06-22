using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.IO;
using System.Web;

namespace umbraco.MacroEngines
{
    public class HtmlTagWrapper : HtmlTagWrapperBase, IHtmlString
    {
        public HtmlTagWrapper Parent;
        public List<HtmlTagWrapperBase> Children;
        public List<KeyValuePair<string, string>> Attributes;
        public void ReflectAttributesFromAnonymousType(List<KeyValuePair<string, string>> newAttributes)
        {
            List<KeyValuePair<string, string>> mergedAttributes =
             newAttributes
             .Concat(Attributes)
             .GroupBy(kvp => kvp.Key, kvp => kvp.Value)
             .Select(g => new KeyValuePair<string, string>(g.Key, string.Join(" ", g.ToArray())))
             .ToList();

            Attributes = mergedAttributes;
        }
        public void ReflectAttributesFromAnonymousType(object anonymousAttributes)
        {
            var newAttributes =
                anonymousAttributes
                    .GetType()
                    .GetProperties()
                    .Where(prop => !string.IsNullOrWhiteSpace(string.Format("{0}", prop.GetValue(anonymousAttributes, null))))
                    .ToList()
                    .ConvertAll(prop =>
                        new KeyValuePair<string, string>(
                            prop.Name,
                            string.Format("{0}", prop.GetValue(anonymousAttributes, null))
                        )
                    );
            ReflectAttributesFromAnonymousType(newAttributes);

        }

        public List<string> CssClasses;
        public string Tag;
        public bool Visible;

        public HtmlTagWrapper(string Tag)
        {
            this.Tag = Tag;
            this.Children = new List<HtmlTagWrapperBase>();
            this.CssClasses = new List<string>();
            this.Attributes = new List<KeyValuePair<string, string>>();
            this.Visible = true;
        }
        public HtmlString Write()
        {
            if ((Children.Count > 0 || Attributes.Count > 0 || CssClasses.Count > 0) && Visible)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (TextWriter tw = new StreamWriter(ms))
                    {
                        HtmlTextWriter html = new HtmlTextWriter(tw);
                        this.WriteToHtmlTextWriter(html);
                        tw.Flush();
                        ms.Position = 0;
                        using (TextReader tr = new StreamReader(ms))
                        {
                            string result = tr.ReadToEnd();
                            return new HtmlString(result);
                        }
                    }
                }
            }
            return new HtmlString(string.Empty);
        }
        public override string ToString()
        {
            return "Use @item.Write() to emit the HTML rather than @item";
        }
        public IHtmlString ToHtml()
        {
            return this.Write();
        }
        public void WriteToHtmlTextWriter(HtmlTextWriter html)
        {
            html.WriteBeginTag(Tag);
            string cssClassNames = string.Join(" ", CssClasses.ToArray()).Trim();
            foreach (var attribute in Attributes)
            {
                html.WriteAttribute(attribute.Key, attribute.Value);
            }
            if (!string.IsNullOrWhiteSpace(cssClassNames))
            {
                html.WriteAttribute("class", cssClassNames);
            }
            html.Write(HtmlTextWriter.TagRightChar);
            foreach (var child in Children)
            {
                child.WriteToHtmlTextWriter(html);
            }
            html.WriteEndTag(Tag);
        }

        public HtmlTagWrapper AddClassName(string className)
        {
            className = className.Trim();
            if (!this.CssClasses.Contains(className))
            {
                this.CssClasses.Add(className);
            }
            return this;
        }

        public HtmlTagWrapper RemoveClassName(string className)
        {
            className = className.Trim();
            if (this.CssClasses.Contains(className))
            {
                this.CssClasses.Remove(className);
            }
            return this;
        }

        public bool HasClassName(string className)
        {
            className = className.Trim();
            return (this.CssClasses.Contains(className));
        }

        public HtmlTagWrapper Attr(object newAttributes)
        {
            this.ReflectAttributesFromAnonymousType(newAttributes);
            return this;
        }
        public HtmlTagWrapper Attr(string name, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                List<KeyValuePair<string, string>> newAttributes = new List<KeyValuePair<string, string>>();
                newAttributes.Add(new KeyValuePair<string, string>(name, value));
                this.ReflectAttributesFromAnonymousType(newAttributes);
            }
            else
            {
                var existingKey = this.Attributes.Find(item => item.Key == name);
                Attributes.Remove(existingKey);
            }
            return this;
        }

        public HtmlTagWrapper AddChild(HtmlTagWrapperBase newChild)
        {
            Children.Add(newChild);
            return this;
        }
        public HtmlTagWrapper AddChildren(params HtmlTagWrapperBase[] collection)
        {
            Children.AddRange(collection);
            return this;
        }
        public HtmlTagWrapper AddChild(string text)
        {
            Children.Add(new HtmlTagWrapperTextNode(text));
            return this;
        }
        public HtmlTagWrapper AddChildAt(int index, HtmlTagWrapperBase newChild)
        {
            Children.Insert(index, newChild);
            return this;
        }
        public HtmlTagWrapper AddChildAt(int index, string text)
        {
            Children.Insert(index, new HtmlTagWrapperTextNode(text));
            return this;
        }
        public HtmlTagWrapper AddChildrenAt(int index, params HtmlTagWrapperBase[] collection)
        {
            Children.InsertRange(index, collection);
            return this;
        }
        public HtmlTagWrapper RemoveChildAt(int index)
        {
            return this;
        }
        public int CountChildren()
        {
            return this.Children.Count;
        }
        public HtmlTagWrapper ClearChildren()
        {
            return this;
        }

        public string ToHtmlString()
        {
            return this.Write().ToHtmlString();
        }
    }

}
