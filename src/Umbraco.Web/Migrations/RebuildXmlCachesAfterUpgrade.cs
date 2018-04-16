using System;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Scoping;
using Umbraco.Web.Composing;
using Umbraco.Web.PublishedCache.XmlPublishedCache;

namespace Umbraco.Web.Migrations
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
        public void Execute(string name, IScope scope, SemVersion originVersion, SemVersion targetVersion, ILogger logger)
        {
            if (name != Constants.System.UmbracoUpgradePlanName) return;

            var v730 = new SemVersion(new Version(7, 3, 0));

            var doMedia = originVersion < v730;
            var doContent = originVersion < v730;

            if (doMedia)
            {
                // fixme - maintain - for backward compatibility?! or replace with...?!
                //var mediaService = (MediaService) ApplicationContext.Current.Services.MediaService;
                //mediaService.RebuildXmlStructures();

                var svc = Current.PublishedSnapshotService as PublishedSnapshotService;
                svc?.RebuildMediaXml();

                // note: not re-indexing medias?
            }

            if (doContent)
            {
                // fixme - maintain - for backward compatibility?! or replace with...?!
                //var contentService = (ContentService) ApplicationContext.Current.Services.ContentService;
                //contentService.RebuildXmlStructures();

                var svc = Current.PublishedSnapshotService as PublishedSnapshotService;
                svc?.RebuildContentAndPreviewXml();
            }
        }
    }
}
