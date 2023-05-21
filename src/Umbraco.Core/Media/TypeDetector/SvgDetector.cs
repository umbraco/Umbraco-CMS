using System.Xml.Linq;

namespace Umbraco.Cms.Core.Media.TypeDetector;

public class SvgDetector
{
    public static bool IsOfType(Stream fileStream)
    {
        var document = new XDocument();

        try
        {
            document = XDocument.Load(fileStream);
        }
        catch (Exception)
        {
            return false;
        }

        return document.Root?.Name.LocalName == "svg";
    }
}
