using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_12_0_0;

public class ResetCache : MigrationBase
{
    private readonly IHostEnvironment _hostEnvironment;

    public ResetCache(IMigrationContext context, IHostEnvironment hostEnvironment)
        : base(context) => _hostEnvironment = hostEnvironment;

    protected override void Migrate()
    {
        RebuildCache = true;
        var distCacheFolderAbsolutePath = _hostEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempFileUploads + "/DistCache");
        File.Delete(distCacheFolderAbsolutePath);
    }
}
