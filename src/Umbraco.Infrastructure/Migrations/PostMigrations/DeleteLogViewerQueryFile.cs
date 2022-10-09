using Umbraco.Cms.Core.Hosting;

// using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_0_0;
namespace Umbraco.Cms.Infrastructure.Migrations.PostMigrations;

/// <summary>
///     Deletes the old file that saved log queries
/// </summary>
public class DeleteLogViewerQueryFile : MigrationBase
{
    private readonly IHostingEnvironment _hostingEnvironment;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeleteLogViewerQueryFile" /> class.
    /// </summary>
    public DeleteLogViewerQueryFile(IMigrationContext context, IHostingEnvironment hostingEnvironment)
        : base(context) =>
        _hostingEnvironment = hostingEnvironment;

    /// <inheritdoc />
    protected override void Migrate()
    {
        // var logViewerQueryFile = MigrateLogViewerQueriesFromFileToDb.GetLogViewerQueryFile(_hostingEnvironment);
        //
        // if(File.Exists(logViewerQueryFile))
        // {
        //     File.Delete(logViewerQueryFile);
        // }
    }
}
