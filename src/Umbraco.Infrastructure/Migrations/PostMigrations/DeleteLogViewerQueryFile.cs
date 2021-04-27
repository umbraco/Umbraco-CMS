using System.IO;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_0_0;

namespace Umbraco.Cms.Infrastructure.Migrations.PostMigrations
{
    /// <summary>
    /// Deletes the old file that saved log queries
    /// </summary>
    public class DeleteLogViewerQueryFile : IMigration
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteLogViewerQueryFile"/> class.
        /// </summary>
        public DeleteLogViewerQueryFile(IMigrationContext context, IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        /// <inheritdoc />
        public void Migrate()
        {
            var logViewerQueryFile = MigrateLogViewerQueriesFromFileToDb.GetLogViewerQueryFile(_hostingEnvironment);

            if(File.Exists(logViewerQueryFile))
            {
                File.Delete(logViewerQueryFile);
            }
        }
    }
}
