using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.Factories;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenEightZero
{
    [Migration("7.8.0", 1, Constants.System.UmbracoMigrationName)]
    public class AddCmsMediaTable : MigrationBase
    {
        public AddCmsMediaTable(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();

            if (tables.InvariantContains("cmsMedia") == false)
            {
                Create.Table<MediaDto>();

                MigrateMediaPaths();                
            }
        }

        private void MigrateMediaPaths()
        {
            Execute.Code(database =>
            {
                //Due to how much data there can be and that this query will result in executing an Index Scan on the cmsPropertyData PK
                //which will literally iterate over every row in there (assume there's 1,000,000 and you can see why this will perform
                //horribly) ultimately we need to fix this: http://issues.umbraco.org/issue/U4-10286
                //so in the meantime I've found an ugly query to work around this Index Scan issue which is to use a sub-query of Ids. 
                //Doing this results in the same Index Scan taking place but it only visits the explicit rows that are provided, not every row
                //in the cmsPropertyData table. This results in a tremendously faster query when there is tons of data.

                //The will return any media property in nvarchar or ntext that is an umbracoFile that is not null.                
                //This will give us the least amount of data returned to work with.
                var sql = @"SELECT cmsPropertyData.dataNvarchar, cmsPropertyData.dataNtext, umbracoNode.id, cmsContentVersion.VersionId
                            FROM cmsPropertyData
                            INNER JOIN cmsPropertyType ON cmsPropertyType.id = cmsPropertyData.propertytypeid
                            INNER JOIN umbracoNode ON umbracoNode.id = cmsPropertyData.contentNodeId
                            INNER JOIN cmsContentVersion ON cmsContentVersion.ContentId = umbracoNode.id
                            WHERE cmsPropertyType.Alias = (@alias)
                                AND umbracoNode.id IN (SELECT umbracoNode.id
                                    FROM umbracoNode 
		                            INNER JOIN cmsContent ON cmsContent.nodeId = umbracoNode.id
		                            INNER JOIN cmsContentType ON cmsContentType.nodeId = cmsContent.contentType
		                            INNER JOIN cmsPropertyType ON cmsPropertyType.contentTypeId = cmsContentType.nodeId
		                            WHERE cmsPropertyType.Alias = (@alias) AND umbracoNode.nodeObjectType = (@nodeObjectType))
                                AND (cmsPropertyData.dataNvarchar IS NOT NULL OR cmsPropertyData.dataNtext IS NOT NULL)";

                var paths = new List<MediaDto>();

                //using QUERY = a db cursor, we won't load this all into memory first, just row by row
                foreach (var row in database.Query<dynamic>(sql, new {alias = "umbracoFile", nodeObjectType = Constants.ObjectTypes.Media}))
                {
                    var id = (int) row.id;
                    var versionId = (Guid)row.VersionId;

                    string mediaPath = null;

                    //if there's values in dataNvarchar then ensure there's a media path match and extract it
                    if (row.dataNvarchar != null && TryMatch((string) row.dataNvarchar, out mediaPath))
                    {
                        paths.Add(new MediaDto
                        {
                            MediaPath = mediaPath,
                            NodeId = id,
                            VersionId = versionId
                        });
                    }
                    //if there's values in dataNtext then ensure there's a media path match and extract it
                    else if (row.dataNtext != null && TryMatch((string) row.dataNtext, out mediaPath))
                    {
                        paths.Add(new MediaDto
                        {
                            MediaPath = mediaPath,
                            NodeId = id,
                            VersionId = versionId
                        });
                    }
                }

                //now we need to insert paths for each item
                database.BulkInsertRecords(paths, SqlSyntax);

                return null;
            });

        }

        public override void Down()
        {
        }

        private static readonly Regex MediaPathPattern = new Regex(@"(/media/.+?)(?:['""]|$)", RegexOptions.Compiled);

        /// <summary>
        /// Try getting a media path out of the string being stored for media
        /// </summary>
        /// <param name="text"></param>
        /// <param name="mediaPath"></param>
        /// <returns></returns>
        private static bool TryMatch(string text, out string mediaPath)
        {
            //TODO: In v8 we should allow exposing this via the property editor in a much nicer way so that the property editor
            // can tell us directly what any URL is for a given property if it contains an asset

            mediaPath = null;

            if (string.IsNullOrWhiteSpace(text))
                return false;

            var match = MediaPathPattern.Match(text);
            if (match.Success == false || match.Groups.Count != 2)
                return false;


            var url = match.Groups[1].Value;
            mediaPath = url;
            return true;
        }
    }
}
