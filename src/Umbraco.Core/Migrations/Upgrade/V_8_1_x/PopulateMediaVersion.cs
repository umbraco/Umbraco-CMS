using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_1_x
{
    public class PopulateMediaVersion : MigrationBase
    {
        private static readonly Regex SrcPathPattern = new Regex(@"['""]src['""]:\s*['""](.+?)['""]", RegexOptions.Compiled);
        
        public PopulateMediaVersion(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            Database.Execute($"DELETE FROM {Constants.DatabaseSchema.Tables.MediaVersion} WHERE path IS NULL");

            var sql = $@"SELECT varcharValue, textValue, versionId
                        FROM {Constants.DatabaseSchema.Tables.PropertyData}
                        WHERE (varcharValue IS NOT NULL OR textValue IS NOT NULL)
                        AND versionId IN (
	                        SELECT cv.id
	                        FROM {Constants.DatabaseSchema.Tables.Node} n
	                        INNER JOIN {Constants.DatabaseSchema.Tables.ContentVersion} cv ON cv.nodeId = n.id
	                        LEFT JOIN {Constants.DatabaseSchema.Tables.MediaVersion} mv ON mv.id = cv.id
	                        WHERE n.nodeObjectType = (@nodeObjectType)
	                        AND mv.id is null
	                        )
                        AND propertytypeid IN (
	                        SELECT id
	                        FROM {Constants.DatabaseSchema.Tables.PropertyType} pt
	                        INNER JOIN {Constants.DatabaseSchema.Tables.DataType} dt ON dt.nodeId = pt.dataTypeId
	                        WHERE dt.propertyEditorAlias = (@propertyEditorAlias)
	                        )";

            var paths = new List<MediaVersionDto>();
            var versionsDone = new HashSet<int>();

            foreach (var row in Database.Query<dynamic>(sql, new { propertyEditorAlias = "Umbraco.UploadField", nodeObjectType = Constants.ObjectTypes.Media }))
            {
                var versionId = (int)row.versionId;
                string mediaPath = (string)(String.IsNullOrEmpty(row.varcharValue) ? row.textValue : row.varcharValue);

                if (!String.IsNullOrWhiteSpace(mediaPath) && versionsDone.Add(versionId))
                {
                    paths.Add(new MediaVersionDto
                    {
                        Path = mediaPath,                        
                        Id = versionId
                    });
                }
            }

            foreach (var row in Database.Query<dynamic>(sql, new { propertyEditorAlias = "Umbraco.ImageCropper", nodeObjectType = Constants.ObjectTypes.Media }))
            {
                var versionId = (int)row.versionId;
                string value = (string)(String.IsNullOrEmpty(row.varcharValue) ? row.textValue : row.varcharValue);

                var match = SrcPathPattern.Match(value ?? "");
                if (match.Success && versionsDone.Add(versionId))
                {
                    paths.Add(new MediaVersionDto
                    {
                        Path = match.Groups[1].Value,                        
                        Id = versionId
                    });
                }
            }
            
            Database.BulkInsertRecords(paths);

            Logger.Info(typeof(PopulateMediaVersion), $"Populated {Constants.DatabaseSchema.Tables.MediaVersion} for {paths.Count} media items");
        }
    }
}
