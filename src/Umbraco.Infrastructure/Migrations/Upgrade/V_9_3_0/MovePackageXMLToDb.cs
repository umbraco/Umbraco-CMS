using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_3_0;

public class MovePackageXMLToDb : MigrationBase
{
    private readonly PackagesRepository _packagesRepository;
    private readonly PackageDefinitionXmlParser _xmlParser;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MovePackageXMLToDb" /> class.
    /// </summary>
    public MovePackageXMLToDb(IMigrationContext context, PackagesRepository packagesRepository)
        : base(context)
    {
        _packagesRepository = packagesRepository;
        _xmlParser = new PackageDefinitionXmlParser();
    }

    /// <inheritdoc />
    protected override void Migrate()
    {
        CreateDatabaseTable();
        MigrateCreatedPackageFilesToDb();
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
        IEnumerable<PackageDefinition> packages = _packagesRepository.GetAll().WhereNotNull();
        var createdPackageDtos = new List<CreatedPackageSchemaDto>();
        foreach (PackageDefinition package in packages)
        {
            // Create dto from xmlDocument
            var dto = new CreatedPackageSchemaDto
            {
                Name = package.Name,
                Value = _xmlParser.ToXml(package).ToString(),
                UpdateDate = DateTime.Now,
                PackageId = Guid.NewGuid(),
            };
            createdPackageDtos.Add(dto);
        }

        _packagesRepository.DeleteLocalRepositoryFiles();
        if (createdPackageDtos.Any())
        {
            // Insert dto into CreatedPackage table
            Database.InsertBulk(createdPackageDtos);
        }
    }
}
