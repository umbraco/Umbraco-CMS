using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Events;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;

namespace Umbraco.Web.Strategies.Migrations
{

    /// <summary>
    /// When upgrading version 7.3 the migration MigrateStylesheetDataToFile will execute but we don't want to overwrite the developers
    /// files during the migration since other parts of the migration might fail. So once the migration is complete, we'll then copy over the temp
    /// files that this migration created over top of the developer's files. We'll also create a backup of their files.
    /// </summary>
    public sealed class OverwriteStylesheetFilesFromTempFiles : MigrationStartupHander
    {
        protected override void AfterMigration(MigrationRunner sender, MigrationEventArgs e)
        {
            var target73 = new Version(7, 3, 0);

            if (e.ConfiguredVersion <= target73)
            {
                var tempCssFolder = IOHelper.MapPath("~/App_Data/TEMP/CssMigration/");
                var cssFolder = IOHelper.MapPath("~/css");
                if (Directory.Exists(tempCssFolder))
                {
                    var files = Directory.GetFiles(tempCssFolder, "*.css", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        var relativePath = file.TrimStart(tempCssFolder).TrimStart("\\");
                        var cssFilePath = Path.Combine(cssFolder, relativePath);
                        if (File.Exists(cssFilePath))
                        {
                            //backup
                            var targetPath = Path.Combine(tempCssFolder, relativePath.EnsureEndsWith(".bak"));
                            e.MigrationContext.Logger.Info<OverwriteStylesheetFilesFromTempFiles>("CSS file is being backed up from {0}, to {1} before being migrated to new format", () => cssFilePath, () => targetPath);
                            File.Copy(cssFilePath, targetPath, true);
                        }

                        //ensure the sub folder eixts
                        Directory.CreateDirectory(Path.GetDirectoryName(cssFilePath));
                        File.Copy(file, cssFilePath, true);
                    }
                }
            }
        }
    }
}
