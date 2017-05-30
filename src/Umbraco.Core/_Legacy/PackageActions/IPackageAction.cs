using System.Xml;
using Umbraco.Core.Composing;

namespace Umbraco.Core._Legacy.PackageActions
{
    public interface IPackageAction : IDiscoverable
    {
        bool Execute(string packageName, XmlNode xmlData);
        string Alias();
        bool Undo(string packageName, XmlNode xmlData);
        XmlNode SampleXml();
    }
}
