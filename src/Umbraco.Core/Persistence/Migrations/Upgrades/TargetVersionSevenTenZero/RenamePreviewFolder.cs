using System.IO;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;
using File = System.IO.File;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenTenZero
{
    /// <summary>
    /// Renames the preview folder containing static html files to ensure it does not interfere with the MVC route
    /// that is now supposed to render these views dynamically. We don't want to delete as people may have made
    /// customizations to these files that would need to be migrated to the new .cshtml view files.
    /// </summary>
    [Migration("7.10.0", 1, Constants.System.UmbracoMigrationName)]
    public class RenamePreviewFolder : MigrationBase
    {
        public RenamePreviewFolder(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            var previewFolderPath = IOHelper.MapPath(SystemDirectories.Umbraco + "/preview");
            if (Directory.Exists(previewFolderPath))
            {
                var newPath = previewFolderPath.Replace("preview", "preview.old");
                if (Directory.Exists(newPath) == false)
                {
                    Directory.Move(previewFolderPath, newPath);
                    var readmeText =
                        $"Static html files used for preview and canvas editing functionality no longer live in this directory.\r\n" +
                        $"Instead they have been recreated as MVC views and can now be found in '~/Umbraco/Views/Preview'.\r\n" +
                        $"See issue: http://issues.umbraco.org/issue/U4-11090";
                    File.WriteAllText(Path.Combine(newPath, "readme.txt"), readmeText);
                }                
            }
        }

        public override void Down()
        {
        }
    }
}
