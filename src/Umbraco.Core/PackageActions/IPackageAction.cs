using System.Xml.Linq;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PackageActions
{
    public interface IPackageAction : IDiscoverable
    {
        bool Execute(string packageName, XElement xmlData);
        string Alias();
        bool Undo(string packageName, XElement xmlData);
    }
}
