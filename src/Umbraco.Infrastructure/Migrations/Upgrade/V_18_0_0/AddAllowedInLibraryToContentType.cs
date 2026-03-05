using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0;

/// <summary>
///     Adds the <c>allowedInLibrary</c> column to the <c>cmsContentType</c> table.
/// </summary>
public class AddAllowedInLibraryToContentType : AsyncMigrationBase
{
    private const string ColumnName = "allowedInLibrary";

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddAllowedInLibraryToContentType"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AddAllowedInLibraryToContentType(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    protected override Task MigrateAsync()
    {
        if (ColumnExists(Constants.DatabaseSchema.Tables.ContentType, ColumnName))
        {
            return Task.CompletedTask;
        }

        AddColumn<ContentTypeDto>(Constants.DatabaseSchema.Tables.ContentType, ColumnName);
        return Task.CompletedTask;
    }
}
