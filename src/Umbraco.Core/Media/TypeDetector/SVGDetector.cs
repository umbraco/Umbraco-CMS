using System.IO;
using System.Xml.Linq;

namespace Umbraco.Core.Media.TypeDetector
{
    public class SVGDetector
    {
        public static bool IsOfType(Stream fileStream)
        {
            var document = new XDocument();

            try
            {
                document = XDocument.Load(fileStream);
            }
            catch (System.Exception ex)
            {
                return false;
            }

            return document.Root?.Name.LocalName == "svg";
        }
    }
}
