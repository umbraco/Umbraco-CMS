using System;
using System.Xml.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core._Legacy.PackageActions;

namespace Umbraco.Core.Packaging
{
    /// <summary>
    /// Package actions are executed on packge install / uninstall.
    /// </summary>
    internal class PackageActionRunner : IPackageActionRunner
    {
        private readonly ILogger _logger;
        private readonly PackageActionCollection _packageActions;

        public PackageActionRunner(ILogger logger, PackageActionCollection packageActions)
        {
            _logger = logger;
            _packageActions = packageActions;
        }

        /// <summary>
        /// Runs the package action with the specified action alias.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="actionAlias">The action alias.</param>
        /// <param name="actionXml">The action XML.</param>
        public void RunPackageAction(string packageName, string actionAlias, XElement actionXml)
        {

            foreach (var ipa in _packageActions)
            {
                try
                {
                    if (ipa.Alias() == actionAlias)
                        ipa.Execute(packageName, actionXml);
                }
                catch (Exception ex)
                {
                    _logger.Error<PackageActionRunner>(ex, "Error loading package action '{PackageActionAlias}' for package {PackageName}", ipa.Alias(), packageName);
                }
            }
        }

        /// <summary>
        /// Undos the package action with the specified action alias.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="actionAlias">The action alias.</param>
        /// <param name="actionXml">The action XML.</param>
        public void UndoPackageAction(string packageName, string actionAlias, XElement actionXml)
        {
            foreach (var ipa in _packageActions)
            {
                try
                {
                    if (ipa.Alias() == actionAlias)
                        ipa.Undo(packageName, actionXml);
                }
                catch (Exception ex)
                {
                    _logger.Error<PackageActionRunner>(ex, "Error undoing package action '{PackageActionAlias}' for package {PackageName}", ipa.Alias(), packageName);
                }
            }
        }

    }
}
