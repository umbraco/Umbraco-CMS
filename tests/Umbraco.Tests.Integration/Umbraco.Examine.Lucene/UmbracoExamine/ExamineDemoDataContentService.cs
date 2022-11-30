using System.Xml.Linq;
using System.Xml.XPath;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Examine.Lucene.UmbracoExamine;

// TODO: This is ultra hack and still left over from legacy but still works for testing atm
public class ExamineDemoDataContentService
{
    public const int ProtectedNode = 1142;

    private readonly XDocument _xContent;

    public ExamineDemoDataContentService(string contentXml = null)
    {
        if (contentXml == null)
        {
            contentXml = TestFiles.umbraco;
        }

        _xContent = XDocument.Parse(contentXml);
    }

    /// <summary>
    ///     Return the XDocument containing the xml from the umbraco.config xml file
    /// </summary>
    /// <param name="xpath"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This is no different in the test suite as published content
    /// </remarks>
    public XDocument GetLatestContentByXPath(string xpath)
    {
        var xdoc = XDocument.Parse("<content></content>");
        xdoc.Root.Add(_xContent.XPathSelectElements(xpath));

        return xdoc;
    }

    /// <summary>
    ///     Return the XDocument containing the xml from the umbraco.config xml file
    /// </summary>
    /// <param name="xpath"></param>
    /// <returns></returns>
    public XDocument GetPublishedContentByXPath(string xpath) => GetContentByXPath(xpath, _xContent);

    private XDocument GetContentByXPath(string xpath, XDocument content)
    {
        var xdoc = XDocument.Parse("<content></content>");
        xdoc.Root.Add(content.XPathSelectElements(xpath));

        return xdoc;
    }
}
