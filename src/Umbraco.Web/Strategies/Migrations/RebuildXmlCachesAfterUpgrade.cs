using System;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Services;
using Umbraco.Web.PublishedCache.XmlPublishedCache;
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
    public class RebuildXmlCachesAfterUpgrade : IPostMigration
    {
        public void Migrated(MigrationRunner sender, MigrationEventArgs args)
        {
            if (args.ProductName != GlobalSettings.UmbracoMigrationName) return;

            var v730 = new Semver.SemVersion(new Version(7, 3, 0));

            var doMedia = args.ConfiguredSemVersion < v730;
            var doContent = args.ConfiguredSemVersion < v730;

            if (doMedia)
            {
                // fixme - maintain - for backward compatibility?! or replace with...?!
                //var mediaService = (MediaService) ApplicationContext.Current.Services.MediaService;
                //mediaService.RebuildXmlStructures();

                var svc = Current.FacadeService as FacadeService;
                svc?.RebuildMediaXml();

                // note: not re-indexing medias?
            }

            if (doContent)
            {
                // fixme - maintain - for backward compatibility?! or replace with...?!
                //var contentService = (ContentService) ApplicationContext.Current.Services.ContentService;
                //contentService.RebuildXmlStructures();

                var svc = Current.FacadeService as FacadeService;
                svc?.RebuildContentAndPreviewXml();
            }
        }
    }
}