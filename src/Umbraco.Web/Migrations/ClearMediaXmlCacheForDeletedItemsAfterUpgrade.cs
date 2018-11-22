using Semver;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Scoping;

namespace Umbraco.Web.Migrations
{
    /// <summary>
    /// This will execute after upgrading to remove any xml cache for media that are currently in the bin
    /// </summary>
    /// <remarks>
    /// This will execute for specific versions -
    ///
    /// * If current is less than or equal to 7.0.0
    /// </remarks>
    public class ClearMediaXmlCacheForDeletedItemsAfterUpgrade : IPostMigration
    {
        private readonly ILogger _logger;

        public ClearMediaXmlCacheForDeletedItemsAfterUpgrade(ILogger logger)
        {
            _logger = logger;
        }

        public void Execute(string name, IScope scope, SemVersion originVersion, SemVersion targetVersion, ILogger logger)
        {
            if (name != Constants.System.UmbracoUpgradePlanName) return;

            var target70 = new SemVersion(7 /*, 0, 0*/);

            if (originVersion <= target70)
            {
                //This query is structured to work with MySql, SQLCE and SqlServer:
                // http://issues.umbraco.org/issue/U4-3876

                var syntax = scope.SqlContext.SqlSyntax;

                var sql = @"DELETE FROM cmsContentXml WHERE nodeId IN
    (SELECT nodeId FROM (SELECT DISTINCT cmsContentXml.nodeId FROM cmsContentXml
    INNER JOIN umbracoNode ON cmsContentXml.nodeId = umbracoNode.id
    WHERE nodeObjectType = '" + Constants.ObjectTypes.Media + "' AND " + syntax.GetQuotedColumnName("path") + " LIKE '%-21%') x)";

                var count = scope.Database.Execute(sql);

                _logger.Info<ClearMediaXmlCacheForDeletedItemsAfterUpgrade>("Cleared {Total} items from the media xml cache that were trashed and not meant to be there", count);
            }
        }
    }
}
