using System.IO;
using System.Xml.Linq;

namespace Umbraco.Web.Media.TypeDetector
{
    public class SvgDetector
    {
        public static bool IsOfType(Stream fileStream)
        {
            var document = new XDocument();

            try
            {
                document = XDocument.Load(fileStream);
            }
            catch (System.Exception)
            {
                return false;
            }

            return document.Root?.Name.LocalName == "svg";
        }
    }
}
