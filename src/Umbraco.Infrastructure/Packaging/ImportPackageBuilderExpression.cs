using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Core.Packaging
{
    internal class ImportPackageBuilderExpression : MigrationExpressionBase
    {
        private readonly IPackagingService _packagingService;

        public ImportPackageBuilderExpression(IPackagingService packagingService, IMigrationContext context) : base(context)
            => _packagingService = packagingService;

        public bool FromEmbeddedResource { get; set; }

        public override void Execute()
        {
            if (!FromEmbeddedResource)
            {
                throw new InvalidOperationException($"Nothing to execute, {nameof(FromEmbeddedResource)} has not been called.");
            }

            // lookup the embedded resource by convention
            Type currentType = GetType();
            Assembly currentAssembly = currentType.Assembly;
            var fileName = $"{currentType.Namespace}.package.xml";
            Stream stream = currentAssembly.GetManifestResourceStream(fileName);
            if (stream == null)
            {
                throw new FileNotFoundException("Cannot find the embedded file.", fileName);
            }
            XDocument xml;
            using (stream)
            {
                xml = XDocument.Load(stream);
            }

            // TODO: Use the packaging service
        }
    }
}
