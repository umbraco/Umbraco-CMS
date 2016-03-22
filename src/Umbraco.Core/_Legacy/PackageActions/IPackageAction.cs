using System.Xml;

namespace Umbraco.Core._Legacy.PackageActions
{
    public interface IPackageAction
    {
        bool Execute(string packageName, XmlNode xmlData);
        string Alias();
        bool Undo(string packageName, XmlNode xmlData);
        XmlNode SampleXml();
    }
}
