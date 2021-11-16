using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_2_0
{
    public class MovePackageXMLToDb : MigrationBase
    {
        private readonly IUmbracoDatabase _umbracoDatabase;

        private readonly ICreatedPackagesRepository _createdPackagesRepository;
        /// <summary>
        /// Initializes a new instance of the <see cref="MovePackageXMLToDb"/> class.
        /// </summary>
        public MovePackageXMLToDb(IMigrationContext context, IUmbracoDatabase umbracoDatabase, ICreatedPackagesRepository createdPackagesRepository)
            : base(context)
        {
            _umbracoDatabase = umbracoDatabase;
            _createdPackagesRepository = createdPackagesRepository;
        }

        /// <inheritdoc/>
        protected override void Migrate()
        {
            // Load data from file
            IEnumerable<PackageDefinition> packages = _createdPackagesRepository.GetAll();
            foreach (PackageDefinition package in packages)
            {
                // Load file from path
                var xmlDoc = XDocument.Load(package.PackagePath);

                if (xmlDoc.Document != null)
                {
                    // Create dto from xmlDocument
                    var xmlString = xmlDoc.Document.CreateReader().Value;
                    var dto = new KeyValueDto
                    {
                        Key = Guid.NewGuid().ToString(),
                        Value = xmlString,
                        UpdateDate = DateTime.Now
                    };

                    // Insert dto into KeyValue table
                    _umbracoDatabase.Insert(
                        Cms.Core.Constants.DatabaseSchema.Tables.KeyValue,
                        "key",
                        false,
                        dto);
                }

                // Delete local file
                File.Delete(package.PackagePath);
            }
        }
    }
}
