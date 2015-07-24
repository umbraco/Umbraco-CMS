using System;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using umbraco.interfaces;

namespace Umbraco.Web.Strategies.Migrations
{
    /// <summary>
    /// This will execute after upgrading to remove any xml cache for media that are currently in the bin
    /// </summary>
    /// <remarks>    
    /// This will execute for specific versions - 
    /// 
    /// * If current is less than or equal to 7.0.0
    /// </remarks>
    public class ClearMediaXmlCacheForDeletedItemsAfterUpgrade : MigrationStartupHander
    {       
        protected override void AfterMigration(MigrationRunner sender, MigrationEventArgs e)
        {
            var target70 = new Version(7, 0, 0);

            if (e.ConfiguredVersion <= target70)
            {
                //This query is structured to work with MySql, SQLCE and SqlServer:
                // http://issues.umbraco.org/issue/U4-3876

                var sql = @"DELETE FROM cmsContentXml WHERE nodeId IN
    (SELECT nodeId FROM (SELECT DISTINCT cmsContentXml.nodeId FROM cmsContentXml 
    INNER JOIN umbracoNode ON cmsContentXml.nodeId = umbracoNode.id
    WHERE nodeObjectType = '" + Constants.ObjectTypes.Media + "' AND " + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("path") + " LIKE '%-21%') x)";

                var count = e.MigrationContext.Database.Execute(sql);

                LogHelper.Info<ClearMediaXmlCacheForDeletedItemsAfterUpgrade>("Cleared " + count + " items from the media xml cache that were trashed and not meant to be there");
            }
        }
    }
}