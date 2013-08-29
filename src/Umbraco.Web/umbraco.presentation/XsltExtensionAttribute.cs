using System;
using System.Security.Permissions;
using System.Web;

namespace umbraco
{
    /// <summary>
    /// Allows App_Code XSLT extensions to be declared using the [XsltExtension] class attribute.
    /// </summary>
    /// <remarks>
    /// An optional XML namespace can be specified using [XsltExtension("MyNamespace")].
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Medium, Unrestricted = false)]
    [Obsolete("Use Umbraco.Core.Macros.XsltExtensionAttribute instead")]
    public class XsltExtensionAttribute : Umbraco.Core.Macros.XsltExtensionAttribute
    {
        public XsltExtensionAttribute() : base()
        {
            
        }

        public XsltExtensionAttribute(string ns) : base(ns)
        {
            
        }

    }
}