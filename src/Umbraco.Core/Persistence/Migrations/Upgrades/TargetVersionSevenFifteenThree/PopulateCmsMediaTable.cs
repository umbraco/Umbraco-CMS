using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFifteenThree
{
    [Migration("7.15.3", 1, Constants.System.UmbracoMigrationName)]
    public class PopulateCmsMediaTable : MigrationBase
    {
        private static readonly Regex SrcPathPattern = new Regex(@"['""]src['""]:\s*['""](.+?)['""]", RegexOptions.Compiled);

        public PopulateCmsMediaTable(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            Execute.Code(database =>
            {
                database.Execute("DELETE FROM cmsMedia WHERE mediaPath IS NULL");

                var sql = @"SELECT dataNvarchar, dataNtext, contentNodeId, versionId
                            FROM cmsPropertyData
                            WHERE (dataNvarchar IS NOT NULL OR dataNtext IS NOT NULL)
                            AND contentNodeId NOT IN (SELECT nodeId FROM cmsMedia)
                            AND contentNodeId IN (SELECT id FROM umbracoNode WHERE nodeObjectType = (@nodeObjectType))
                            AND propertytypeid IN (
	                            SELECT id
	                            FROM cmsPropertyType
	                            INNER JOIN cmsDataType ON cmsDataType.nodeId = cmsPropertyType.dataTypeId
	                            WHERE cmsDataType.propertyEditorAlias = (@propertyEditorAlias)
	                            )";

                var paths = new List<MediaDto>();
                var versionsDone = new HashSet<Guid>();

                foreach (var row in database.Query<dynamic>(sql, new { propertyEditorAlias = "Umbraco.UploadField", nodeObjectType = Constants.ObjectTypes.Media }))
                {
                    var id = (int)row.contentNodeId;
                    var versionId = (Guid)row.versionId;

                    string mediaPath = (string)(String.IsNullOrEmpty(row.dataNvarchar) ? row.dataNtext : row.dataNvarchar);

                    if (!String.IsNullOrWhiteSpace(mediaPath) && versionsDone.Add(versionId))
                    {
                        paths.Add(new MediaDto
                        {
                            MediaPath = mediaPath,
                            NodeId = id,
                            VersionId = versionId
                        });
                    }
                }

                foreach (var row in database.Query<dynamic>(sql, new { propertyEditorAlias = "Umbraco.ImageCropper", nodeObjectType = Constants.ObjectTypes.Media }))
                {
                    var id = (int)row.contentNodeId;
                    var versionId = (Guid)row.versionId;

                    string value = (string)(String.IsNullOrEmpty(row.dataNvarchar) ? row.dataNtext : row.dataNvarchar);

                    var match = SrcPathPattern.Match(value ?? "");
                    if (match.Success && versionsDone.Add(versionId))
                    {
                        paths.Add(new MediaDto
                        {
                            MediaPath = match.Groups[1].Value,
                            NodeId = id,
                            VersionId = versionId
                        });
                    }
                }

                database.BulkInsertRecords(paths, SqlSyntax);

                return null;
            });

        }

        public override void Down()
        {
        }
    }
}
