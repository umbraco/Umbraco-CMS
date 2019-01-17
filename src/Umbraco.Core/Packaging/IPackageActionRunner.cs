using System.Collections.Generic;
using System.Xml.Linq;

namespace Umbraco.Core.Packaging
{
    public interface IPackageActionRunner
    {
        /// <summary>
        /// Runs the package action with the specified action alias.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="actionAlias">The action alias.</param>
        /// <param name="actionXml">The action XML.</param>
        /// <param name="errors"></param>
        bool RunPackageAction(string packageName, string actionAlias, XElement actionXml, out IEnumerable<string> errors);

        /// <summary>
        /// Undos the package action with the specified action alias.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="actionAlias">The action alias.</param>
        /// <param name="actionXml">The action XML.</param>
        /// <param name="errors"></param>
        bool UndoPackageAction(string packageName, string actionAlias, XElement actionXml, out IEnumerable<string> errors);
    }
}
