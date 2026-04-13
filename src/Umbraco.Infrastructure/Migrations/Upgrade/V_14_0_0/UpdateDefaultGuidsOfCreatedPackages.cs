using System.Xml.Linq;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

/// <summary>
/// Migration that updates the default GUID values assigned to newly created packages in version 14.0.0.
/// </summary>
public class UpdateDefaultGuidsOfCreatedPackages : MigrationBase
{
    private readonly PackageDefinitionXmlParser _xmlParser;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateDefaultGuidsOfCreatedPackages"/> class with the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to be used for the migration.</param>
    public UpdateDefaultGuidsOfCreatedPackages(IMigrationContext context)
        : base(context)
    {
        _xmlParser = new PackageDefinitionXmlParser();
    }

    protected override void Migrate()
    {
        IEnumerable<CreatedPackageSchemaDto> createdPackages = Database.Fetch<CreatedPackageSchemaDto>();

        foreach (CreatedPackageSchemaDto package in createdPackages)
        {
            if (package.PackageId != default)
            {
                continue;
            }

            var guid = Guid.NewGuid();
            package.PackageId = guid;

            var packageDefinition = _xmlParser.ToPackageDefinition(XElement.Parse(package.Value));

            if (packageDefinition is not null)
            {
                packageDefinition.PackageId = guid;

                // Update the package XML value with the correct GUID
                package.Value = _xmlParser.ToXml(packageDefinition).ToString();
            }

            Database.Update(package);
        }
    }
}
