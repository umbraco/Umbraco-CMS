using System;
using System.IO;
using System.Linq;
using Umbraco.Cms.Core.Models.Packaging;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Packaging
{
    public class PackageInstallation : IPackageInstallation
    {
        private readonly PackageDataInstallation _packageDataInstallation;
        private readonly CompiledPackageXmlParser _parser;


        /// <summary>
        /// Initializes a new instance of the <see cref="PackageInstallation"/> class.
        /// </summary>
        public PackageInstallation(PackageDataInstallation packageDataInstallation, CompiledPackageXmlParser parser)
        {
            _packageDataInstallation = packageDataInstallation ?? throw new ArgumentNullException(nameof(packageDataInstallation));
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        public CompiledPackage ReadPackage(FileInfo packageXmlFile)
        {
            if (packageXmlFile == null) throw new ArgumentNullException(nameof(packageXmlFile));

            var compiledPackage = _parser.ToCompiledPackage(packageXmlFile);
            return compiledPackage;
        }

        public InstallationSummary InstallPackageData(CompiledPackage compiledPackage, int userId, out PackageDefinition packageDefinition)
        {
            packageDefinition = new PackageDefinition
            {
                Name = compiledPackage.Name
            };

            var installationSummary = _packageDataInstallation.InstallPackageData(compiledPackage, userId);

            //make sure the definition is up to date with everything
            foreach (var x in installationSummary.DataTypesInstalled) packageDefinition.DataTypes.Add(x.Id.ToInvariantString());
            foreach (var x in installationSummary.LanguagesInstalled) packageDefinition.Languages.Add(x.Id.ToInvariantString());
            foreach (var x in installationSummary.DictionaryItemsInstalled) packageDefinition.DictionaryItems.Add(x.Id.ToInvariantString());
            foreach (var x in installationSummary.MacrosInstalled) packageDefinition.Macros.Add(x.Id.ToInvariantString());
            foreach (var x in installationSummary.TemplatesInstalled) packageDefinition.Templates.Add(x.Id.ToInvariantString());
            foreach (var x in installationSummary.DocumentTypesInstalled) packageDefinition.DocumentTypes.Add(x.Id.ToInvariantString());
            foreach (var x in installationSummary.StylesheetsInstalled) packageDefinition.Stylesheets.Add(x.Id.ToInvariantString());
            var contentInstalled = installationSummary.ContentInstalled.ToList();
            packageDefinition.ContentNodeId = contentInstalled.Count > 0 ? contentInstalled[0].Id.ToInvariantString() : null;

            return installationSummary;
        }

    }
}
