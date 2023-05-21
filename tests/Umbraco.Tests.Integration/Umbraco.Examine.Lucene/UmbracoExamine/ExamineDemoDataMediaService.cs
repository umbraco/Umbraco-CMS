using System.Xml.Linq;
using System.Xml.XPath;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Examine.Lucene.UmbracoExamine;

// TODO: This is ultra hack and still left over from legacy but still works for testing atm
internal class ExamineDemoDataMediaService
{
    private readonly XDocument _doc;

    public ExamineDemoDataMediaService() => _doc = XDocument.Parse(TestFiles.media);

    #region IMediaService Members

    public XDocument GetLatestMediaByXpath(string xpath)
    {
        var xdoc = XDocument.Parse("<media></media>");
        xdoc.Root.Add(_doc.XPathSelectElements(xpath));
        return xdoc;
    }

    #endregion
}
