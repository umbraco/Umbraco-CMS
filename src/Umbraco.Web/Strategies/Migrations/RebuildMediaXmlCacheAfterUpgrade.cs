using System;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Configuration;
using Umbraco.Web.PublishedCache.XmlPublishedCache;

namespace Umbraco.Web.Strategies.Migrations
{
    /// <summary>
    /// This will execute after upgrading to rebuild the xml cache
    /// </summary>
    /// <remarks>
    /// This cannot execute as part of a db migration since we need access to the services/repos.
    ///
    /// This will execute for specific versions -
    ///
    /// * If current is less than or equal to 7.0.0
    /// </remarks>
    public class RebuildMediaXmlCacheAfterUpgrade : IPostMigration
    {
        public void Migrated(MigrationRunner sender, MigrationEventArgs args)
        {
            if (args.ProductName != GlobalSettings.UmbracoMigrationName) return;

            var target70 = new Version(7, 0, 0);

            if (args.ConfiguredVersion <= target70)
            {
                // maintain - for backward compatibility?
                //var mediasvc = (MediaService)ApplicationContext.Current.Services.MediaService;
                //mediasvc.RebuildMediaXml();
                var svc = Current.FacadeService as FacadeService;
                svc?.RebuildMediaXml();
            }
        }
    }
}