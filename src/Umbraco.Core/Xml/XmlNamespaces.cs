// source: mvpxml.codeplex.com

namespace Umbraco.Cms.Core.Xml;

/// <summary>
///     Provides public constants for wellknown XML namespaces.
/// </summary>
/// <remarks>Author: Daniel Cazzulino, <a href="http://clariusconsulting.net/kzu">blog</a></remarks>
public static class XmlNamespaces
{
    /// <summary>
    ///     The public XML 1.0 namespace.
    /// </summary>
    /// <remarks>See http://www.w3.org/TR/2004/REC-xml-20040204/</remarks>
    public const string Xml = "http://www.w3.org/XML/1998/namespace";

    /// <summary>
    ///     Public Xml Namespaces specification namespace.
    /// </summary>
    /// <remarks>See http://www.w3.org/TR/REC-xml-names/</remarks>
    public const string XmlNs = "http://www.w3.org/2000/xmlns/";

    /// <summary>
    ///     Public Xml Namespaces prefix.
    /// </summary>
    /// <remarks>See http://www.w3.org/TR/REC-xml-names/</remarks>
    public const string XmlNsPrefix = "xmlns";

    /// <summary>
    ///     XML Schema instance namespace.
    /// </summary>
    /// <remarks>See http://www.w3.org/TR/xmlschema-1/</remarks>
    public const string Xsi = "http://www.w3.org/2001/XMLSchema-instance";

    /// <summary>
    ///     XML 1.0 Schema namespace.
    /// </summary>
    /// <remarks>See http://www.w3.org/TR/xmlschema-1/</remarks>
    public const string Xsd = "http://www.w3.org/2001/XMLSchema";
}
