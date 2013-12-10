using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenZeroOne
{
    /// <summary>
    /// Due to this bug: http://issues.umbraco.org/issue/U4-3820 we need to remove the cached media
    /// xml found in the cmsContentXml table for any media that has been recycled.
    /// </summary>
    [Migration("7.0.1", 1, GlobalSettings.UmbracoMigrationName)]
    public class RemoveCachedRecycleMediaXml : MigrationBase
    {
        public override void Up()
        {
            //now that the controlId column is renamed and now a string we need to convert
            if (Context == null || Context.Database == null) return;

            Execute.Code(database =>
                {
                    var mediasvc = (MediaService) ApplicationContext.Current.Services.MediaService;
                    mediasvc.RebuildXmlStructures();

                    return string.Empty;
                });
        }

        public override void Down()
        {
        }
    }
}