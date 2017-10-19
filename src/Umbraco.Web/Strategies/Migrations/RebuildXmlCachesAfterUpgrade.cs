using System;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Services;
using GlobalSettings = Umbraco.Core.Configuration.GlobalSettings;

namespace Umbraco.Web.Strategies.Migrations
{
    /// <summary>
    /// Rebuilds the Xml caches after upgrading.
    /// This will execute after upgrading to rebuild the xml cache
    /// </summary>
    /// <remarks>
    /// <para>This cannot execute as part of a DB migration since it needs access to services and repositories.</para>
    /// <para>Executes for:
    /// - Media Xml : if current is less than, or equal to, 7.0.0 (superceeded by the next rule)
    /// - Media & Content Xml : if current is less than, or equal to, 7.3.0 - because 7.3.0 adds .Key to cached items
    /// </para>
    /// </remarks>
    public class RebuildXmlCachesAfterUpgrade : MigrationStartupHander
    {
        protected override void AfterMigration(MigrationRunner sender, MigrationEventArgs e)
        {
            if (e.ProductName != Constants.System.UmbracoMigrationName) return;

            var v730 = new Semver.SemVersion(new Version(7, 3, 0));

            var doMedia = e.ConfiguredSemVersion < v730;
            var doContent = e.ConfiguredSemVersion < v730;

            if (doMedia)
            {
                var mediaService = (MediaService) ApplicationContext.Current.Services.MediaService;
                mediaService.RebuildXmlStructures();

                // note: not re-indexing medias?
            }

            if (doContent)
            {
                // rebuild Xml in database
                var contentService = (ContentService) ApplicationContext.Current.Services.ContentService;
                contentService.RebuildXmlStructures();

                // refresh the Xml cache
                content.Instance.RefreshContentFromDatabase();
            }
        }
    }
}