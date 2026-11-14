using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
///     Adds the <c>invariantEdited</c> column to the <c>umbracoDocument</c> and <c>umbracoElement</c> tables.
/// </summary>
public class AddInvariantEditedToDocumentAndElement : AsyncMigrationBase
{
    private const string ColumnName = "invariantEdited";

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddInvariantEditedToDocumentAndElement"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AddInvariantEditedToDocumentAndElement(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc/>
    protected override Task MigrateAsync()
    {
        if (!ColumnExists(Constants.DatabaseSchema.Tables.Document, ColumnName))
        {
            AddColumn<DocumentDto>(Constants.DatabaseSchema.Tables.Document, ColumnName);
        }

        if (!ColumnExists(Constants.DatabaseSchema.Tables.Element, ColumnName))
        {
            AddColumn<ElementDto>(Constants.DatabaseSchema.Tables.Element, ColumnName);
        }

        return Task.CompletedTask;
    }
}
