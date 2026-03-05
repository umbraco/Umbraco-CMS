using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;

namespace Umbraco.Cms.Persistence.EFCore.Sqlite;

/// <summary>
/// Applies <c>COLLATE NOCASE</c> to every string property in the EF Core model so that
/// SQLite string comparisons are case-insensitive, matching NPoco's
/// <c>SqliteSyntaxProvider</c> which creates all string columns as <c>TEXT COLLATE NOCASE</c>.
/// </summary>
public class SqliteCollationModelCustomizer : IEFCoreModelCustomizer
{
    /// <inheritdoc />
    public void Apply(ModelBuilder modelBuilder)
    {
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (IMutableProperty property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(string))
                {
                    property.SetCollation("NOCASE");
                }
            }
        }
    }
}
