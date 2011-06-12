using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.IO;
using System.Web;

namespace umbraco.MacroEngines
{
    public class HtmlTagWrapper : HtmlTagWrapperBase
    {
        public HtmlTagWrapper Parent;
        public List<HtmlTagWrapperBase> Children;
        public List<KeyValuePair<string, string>> Attributes;
        public void ReflectAttributesFromAnonymousType(object anonymousAttributes)
        {
            Attributes.AddRange(
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
                    )
              );
        }
        public List<string> CssClasses;
        public string Tag;

        public HtmlTagWrapper(string Tag)
        {
            this.Tag = Tag;
            this.Children = new List<HtmlTagWrapperBase>();
            this.CssClasses = new List<string>();
            this.Attributes = new List<KeyValuePair<string, string>>();
        }
        public HtmlString Write()
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
        public override string ToString()
        {
            return "Use @item.Write() to emit the HTML rather than @item";
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
    }

}
