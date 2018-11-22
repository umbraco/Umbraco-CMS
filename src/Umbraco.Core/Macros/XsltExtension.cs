namespace Umbraco.Core.Macros
{
    /// <summary>
    /// Encapsulates what an xslt extension object is when used for macros
    /// </summary>
    internal sealed class XsltExtension
    {
        public XsltExtension(string ns, object extensionObject)
        {
            Namespace = ns;
            ExtensionObject = extensionObject;
        }

        public string Namespace { get; private set; }
        public object ExtensionObject { get; private set; }
    }
}