using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.PublishedCache;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_12_0_0;

public class ResetCache : MigrationBase
{
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IPublishedSnapshotService _publishedSnapshotService;

    [Obsolete("Use ctor with all params - This will be removed in Umbraco 14.")]
    public ResetCache(IMigrationContext context, IHostingEnvironment hostingEnvironment)
        : this(context, hostingEnvironment, StaticServiceProvider.Instance.GetRequiredService<IPublishedSnapshotService>())
    {
    }

    public ResetCache(IMigrationContext context, IHostingEnvironment hostingEnvironment, IPublishedSnapshotService publishedSnapshotService)
        : base(context)
    {
        _hostingEnvironment = hostingEnvironment;
        _publishedSnapshotService = publishedSnapshotService;
    }

    protected override void Migrate()
    {
        RebuildCache = true;
        var distCacheFolderAbsolutePath = Path.Combine(_hostingEnvironment.LocalTempPath, "DistCache");
        DeleteAllFilesInFolder(distCacheFolderAbsolutePath);
        _publishedSnapshotService.ResetLocalDb();
    }

    private void DeleteAllFilesInFolder(string path)
    {
        if (Directory.Exists(path) == false)
        {
            return;
        }

        var directoryInfo = new DirectoryInfo(path);

        foreach (FileInfo file in directoryInfo.GetFiles())
        {
            file.Delete();
        }
    }
}
