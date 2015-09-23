using System;
using System.IO;
using System.Linq;
using System.Text;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.SqlSyntax;
using File = System.IO.File;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    /// <summary>
    /// Ensures that any stylesheets that have properties defined at the db level have the correct new syntax 
    /// in their files before we remove the stylesheet db tables
    /// </summary>
    /// <remarks>
    /// Instead of modifying the files directly (since we cannot roll them back) we will create temporary migrated files. 
    /// These files will then be copied over once the entire migration is complete so that if any migration fails and the db changes are
    /// rolled back, the original files won't be affected.
    /// </remarks>
    [Migration("7.3.0", 2, GlobalSettings.UmbracoMigrationName)]
    public class MigrateStylesheetDataToFile : MigrationBase
    {
        public MigrateStylesheetDataToFile(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            //Don't exeucte if the stylesheet table is not there
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();
            if (tables.InvariantContains("cmsStylesheet") == false) return;

            //This is all rather nasty but it's how stylesheets used to work in the 2 various ugly ways so we just have to 
            // deal with that to get this migration done


            var tempFolder = IOHelper.MapPath("~/App_Data/TEMP/CssMigration/");
            if (Directory.Exists(tempFolder))
            {
                //clear any existing css files (we back the real ones up so if this migration is run again for whatever reason anything that 
                // was previously backed up is still there, backup happens in a post migration: OverwriteStylesheetFilesFromTempFiles class)
                var files = Directory.GetFiles(tempFolder, "*.css", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    File.Delete(file);
                }
            }
            //create the temp folder
            var tempDir = Directory.CreateDirectory(IOHelper.MapPath("~/App_Data/TEMP/CssMigration/"));


            var sheets = Context.Database.Fetch<dynamic>("SELECT * FROM cmsStylesheet INNER JOIN umbracoNode on cmsStylesheet.nodeId = umbracoNode.id");
            foreach (var sheet in sheets)
            {
                var fileName = sheet.text;

                string dbContent;
                //we will always use the file content over the db content if there is any
                using (var memStream = new MemoryStream(Encoding.UTF8.GetBytes(sheet.content)))
                {
                    dbContent = GetContentAboveUmbracoProps(memStream);
                }

                var fileContent = string.Empty;

                //check for file and read in it's data - umbraco properties will only be kept that are in the db, 
                // anything below the infamous: /* EDITOR PROPERTIES - PLEASE DON'T DELETE THIS LINE TO AVOID DUPLICATE PROPERTIES */
                // line is an Umbraco property and therefore anything that is there that is not in the db will be cleared.

                var filePath = IOHelper.MapPath(string.Format("{0}/{1}.css", SystemDirectories.Css, fileName));
                if (File.Exists(filePath))
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        fileContent = GetContentAboveUmbracoProps(stream);
                    }
                }

                var props = Context.Database.Fetch<dynamic>("SELECT * FROM cmsStylesheetProperty INNER JOIN umbracoNode ON cmsStylesheetProperty.nodeId = umbracoNode.id WHere umbracoNode.parentID = @id", new { id = sheet.nodeId });

                var cssFolderPath = IOHelper.MapPath(SystemDirectories.Css);
                var relativeFsPath = StringExtensions.TrimStart(StringExtensions.TrimStart(filePath, cssFolderPath), "\\");
                var stylesheetInstance = new Stylesheet(relativeFsPath)
                {
                    Content = fileContent.IsNullOrWhiteSpace() ? dbContent : fileContent
                };

                foreach (var prop in props)
                {
                    if (stylesheetInstance.Properties.Any(x => x.Name == prop.text) == false)
                    {
                        stylesheetInstance.AddProperty(new StylesheetProperty(prop.text, prop.stylesheetPropertyAlias, prop.stylesheetPropertyValue));
                    }
                }

                //Save to temp folder

                //ensure the folder for the file exists since it could be in a sub folder
                var tempFilePath = Path.Combine(tempDir.FullName, relativeFsPath);
                Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath));

                File.WriteAllText(tempFilePath, stylesheetInstance.Content, Encoding.UTF8);

            }
        }

        public override void Down()
        {
            throw new NotSupportedException("Cannot downgrade from 7.3 as there are database table deletions");
        }

        private string GetContentAboveUmbracoProps(Stream stream)
        {
            var content = string.Empty;
            using (var re = new StreamReader(stream))
            {
                string input;
                var readingProperties = false;

                while ((input = re.ReadLine()) != null)
                {
                    //This is that line that was in there before that was delimiting umb props from normal css:
                    // /* EDITOR PROPERTIES - PLEASE DON'T DELETE THIS LINE TO AVOID DUPLICATE PROPERTIES */
                    if (input.Contains("EDITOR PROPERTIES"))
                    {
                        readingProperties = true;
                        continue;
                    }

                    if (readingProperties == false)
                    {
                        content += input;
                    }
                }
            }
            return content;
        }
    }
}