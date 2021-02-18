using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.PackageActions;

namespace Umbraco.Cms.Core.Packaging
{
    /// <summary>
    /// Package actions are executed on package install / uninstall.
    /// </summary>
    public class PackageActionRunner : IPackageActionRunner
    {
        private readonly ILogger<PackageActionRunner> _logger;
        private readonly PackageActionCollection _packageActions;

        public PackageActionRunner(ILogger<PackageActionRunner> logger, PackageActionCollection packageActions)
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
                    _logger.LogError(ex, "Error loading package action '{PackageActionAlias}' for package {PackageName}", ipa.Alias(), packageName);
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
                    _logger.LogError(ex, "Error undoing package action '{PackageActionAlias}' for package {PackageName}", ipa.Alias(), packageName);
                }
            }
            errors = e;
            return e.Count == 0;
        }

    }
}
