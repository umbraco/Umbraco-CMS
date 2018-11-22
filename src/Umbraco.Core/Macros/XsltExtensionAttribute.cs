using System;
using System.Security.Permissions;
using System.Web;

namespace Umbraco.Core.Macros
{
    /// <summary>
    /// Allows App_Code XSLT extensions to be declared using the [XsltExtension] class attribute.
    /// </summary>
    /// <remarks>
    /// An optional XML namespace can be specified using [XsltExtension("MyNamespace")].
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class XsltExtensionAttribute : Attribute
    {
        public XsltExtensionAttribute()
        {
            Namespace = String.Empty;
        }

        public XsltExtensionAttribute(string ns)
        {
            Namespace = ns;
        }

        public string Namespace { get; set; }

        public override string ToString()
        {
            return Namespace;
        }
    }
}