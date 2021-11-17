using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_2_0
{
    public class MovePackageXMLToDb : MigrationBase
    {

        private readonly ICreatedPackagesRepository _createdPackagesRepository;
        private readonly PackageDefinitionXmlParser _xmlParser;
        /// <summary>
        /// Initializes a new instance of the <see cref="MovePackageXMLToDb"/> class.
        /// </summary>
        public MovePackageXMLToDb(IMigrationContext context, ICreatedPackagesRepository createdPackagesRepository)
            : base(context)
        {
            _createdPackagesRepository = createdPackagesRepository;
            _xmlParser = new PackageDefinitionXmlParser();
        }

        private void CreateDatabaseTable()
        {
            // Add CreatedPackage table in database if it doesn't exist
            IEnumerable<string> tables = SqlSyntax.GetTablesInSchema(Context.Database);
            if (!tables.InvariantContains(CreatedPackageSchemaDto.TableName))
            {
                Create.Table<CreatedPackageSchemaDto>().Do();
            }
        }

        private void MigrateCreatedPackageFilesToDb()
        {
            // Load data from file
            IEnumerable<PackageDefinition> packages = _createdPackagesRepository.GetAll();
            var createdPackageDtos = new List<CreatedPackageSchemaDto>();
            foreach (PackageDefinition package in packages)
            {
                // Load file from path
                var xmlDoc = XDocument.Load(package.PackagePath);

                if (xmlDoc.Document != null)
                {
                    // Create dto from xmlDocument
                    var dto = new CreatedPackageSchemaDto()
                    {
                        Name = package.Name,
                        Value = _xmlParser.ToXml(package).ToString(),
                        CreateDate = DateTime.Now,
                        PackageId = Guid.NewGuid()
                    };
                    createdPackageDtos.Add(dto);
                }

                File.Delete(package.PackagePath);
            }

            // Insert dto into CreatedPackage table
            Database.InsertBulk(createdPackageDtos);
        }

        /// <inheritdoc/>
        protected override void Migrate()
        {
            CreateDatabaseTable();
            MigrateCreatedPackageFilesToDb();
        }
    }
}
