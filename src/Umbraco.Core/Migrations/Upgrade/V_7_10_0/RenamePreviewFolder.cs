using System.IO;
using Umbraco.Core.IO;
using File = System.IO.File;

namespace Umbraco.Core.Migrations.Upgrade.V_7_10_0
{
    /// <summary>
    /// Renames the preview folder containing static html files to ensure it does not interfere with the MVC route
    /// that is now supposed to render these views dynamically. We don't want to delete as people may have made
    /// customizations to these files that would need to be migrated to the new .cshtml view files.
    /// </summary>
    public class RenamePreviewFolder : MigrationBase
    {
        public RenamePreviewFolder(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
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
    }
}
