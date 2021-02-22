using System.Xml.Linq;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.PackageActions
{
    public interface IPackageAction : IDiscoverable
    {
        bool Execute(string packageName, XElement xmlData);
        string Alias();
        bool Undo(string packageName, XElement xmlData);
    }
}
