﻿using System;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Services;
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
    public class ClearMediaXmlCacheForDeletedItemsAfterUpgrade : IApplicationStartupHandler
    {
        public ClearMediaXmlCacheForDeletedItemsAfterUpgrade()
        {
            MigrationRunner.Migrated += MigrationRunner_Migrated;
        }

        void MigrationRunner_Migrated(MigrationRunner sender, Core.Events.MigrationEventArgs e)
        {
            var target70 = new Version(7, 0, 0);

            if (e.ConfiguredVersion <= target70)
            {


                var sql = @"DELETE cmsContentXml.* FROM cmsContentXml
INNER JOIN umbracoNode ON cmsContentXml.nodeId = umbracoNode.id
        WHERE nodeObjectType = 'B796F64C-1F99-4FFB-B886-4BF4BC011A9C' AND " + SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName("path") + " like '%-21%'";

                var count = e.MigrationContext.Database.Execute(sql);

                LogHelper.Info<ClearMediaXmlCacheForDeletedItemsAfterUpgrade>("Cleared " + count + " items from the media xml cache that were trashed and not meant to be there");

            }

        }
    }
}