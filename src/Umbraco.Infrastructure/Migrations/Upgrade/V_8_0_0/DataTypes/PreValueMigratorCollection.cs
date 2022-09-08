using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0.DataTypes;

public class PreValueMigratorCollection : BuilderCollectionBase<IPreValueMigrator>
{
    private readonly ILogger<PreValueMigratorCollection> _logger;

    public PreValueMigratorCollection(
        Func<IEnumerable<IPreValueMigrator>> items,
        ILogger<PreValueMigratorCollection> logger)
        : base(items) =>
        _logger = logger;

    public IPreValueMigrator? GetMigrator(string editorAlias)
    {
        IPreValueMigrator? migrator = this.FirstOrDefault(x => x.CanMigrate(editorAlias));
        _logger.LogDebug("Getting migrator for \"{EditorAlias}\" = {MigratorType}", editorAlias,
            migrator == null ? "<null>" : migrator.GetType().Name);
        return migrator;
    }
}
