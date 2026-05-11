using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0;

/// <summary>
/// Represents a migration that adds a 'DocumentUrl' column to the relevant database table during the upgrade to version 15.0.0.
/// </summary>
[Obsolete("Remove in Umbraco 18.")]
public class AddDocumentUrl : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of <see cref="AddDocumentUrl"/>.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> for the migration.</param>
    public AddDocumentUrl(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        if (TableExists(Constants.DatabaseSchema.Tables.DocumentUrl) is false)
        {
            Create.Table<DocumentUrlDto>().Do();
        }
    }
}
