using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.PackageActions;

namespace Umbraco.Core.Packaging
{
    /// <summary>
    /// Package actions are executed on package install / uninstall.
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

        /// <inheritdoc />
        public bool RunPackageAction(string packageName, string actionAlias, XElement actionXml, out IEnumerable<string> errors)
        {
            var e = new List<string>();
            foreach (var ipa in _packageActions)
            {
                try
                {
                    if (ipa.Alias() == actionAlias)
                        ipa.Execute(packageName, actionXml);
                }
                catch (Exception ex)
                {
                    e.Add($"{ipa.Alias()} - {ex.Message}");
                    _logger.Error<PackageActionRunner, string, string>(ex, "Error loading package action '{PackageActionAlias}' for package {PackageName}", ipa.Alias(), packageName);
                }
            }

            errors = e;
            return e.Count == 0;
        }

        /// <inheritdoc />
        public bool UndoPackageAction(string packageName, string actionAlias, XElement actionXml, out IEnumerable<string> errors)
        {
            var e = new List<string>();
            foreach (var ipa in _packageActions)
            {
                try
                {
                    if (ipa.Alias() == actionAlias)
                        ipa.Undo(packageName, actionXml);
                }
                catch (Exception ex)
                {
                    e.Add($"{ipa.Alias()} - {ex.Message}");
                    _logger.Error<PackageActionRunner, string, string>(ex, "Error undoing package action '{PackageActionAlias}' for package {PackageName}", ipa.Alias(), packageName);
                }
            }
            errors = e;
            return e.Count == 0;
        }

    }
}
