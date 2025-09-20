using Umbraco.Cms.Infrastructure.Templates.PartialViews;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_10_4_0;

[Obsolete("This is no longer used and will be removed in V14.")]
public class AddBlockGridPartialViews : MigrationBase
{
    private readonly IPartialViewPopulator _partialViewPopulator;
    private const string FolderPath = "/Views/Partials/blockgrid";
    private static readonly string[] _filesToAdd =
    {
        "area.cshtml",
    };

    public AddBlockGridPartialViews(IMigrationContext context, IPartialViewPopulator partialViewPopulator) : base(context)
        => _partialViewPopulator = partialViewPopulator;

    protected override void Migrate()
    {
        var embeddedBasePath = _partialViewPopulator.CoreEmbeddedPath + ".BlockGrid";

        foreach (var fileName in _filesToAdd)
        {
            _partialViewPopulator.CopyPartialViewIfNotExists(
                _partialViewPopulator.GetCoreAssembly(),
                $"{embeddedBasePath}.{fileName}",
                $"{FolderPath}/{fileName}");
        }
    }
}
