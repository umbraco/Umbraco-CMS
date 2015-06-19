using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.UnitOfWork;
using umbraco.interfaces;
using Umbraco.Core;

namespace Umbraco.Web.Strategies.Migrations
{
    /// <summary>
    /// This event ensures that upgrades from (configured) versions lower then 6.0.0
    /// have their publish state updated after the database schema has been migrated.
    /// </summary>
    public class PublishAfterUpgradeToVersionSixth : MigrationStartupHander
    {
        protected override void AfterMigration(MigrationRunner sender, MigrationEventArgs e)
        {
            var target = new Version(6, 0, 0);
            if (e.ConfiguredVersion < target)
            {
                var sql = new Sql();
                sql.Select("*")
                    .From<DocumentDto>()
                    .InnerJoin<ContentVersionDto>()
                    .On<DocumentDto, ContentVersionDto>(left => left.VersionId, right => right.VersionId)
                    .InnerJoin<ContentDto>()
                    .On<ContentVersionDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                    .InnerJoin<NodeDto>()
                    .On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                    .Where<NodeDto>(x => x.NodeObjectType == new Guid(Constants.ObjectTypes.Document))
                    .Where<NodeDto>(x => x.Path.StartsWith("-1"));

                var dtos = e.MigrationContext.Database.Fetch<DocumentDto, ContentVersionDto, ContentDto, NodeDto>(sql);
                var toUpdate = new List<DocumentDto>();
                var versionGroup = dtos.GroupBy(x => x.NodeId);
                foreach (var grp in versionGroup)
                {
                    var published = grp.FirstOrDefault(x => x.Published);
                    var newest = grp.FirstOrDefault(x => x.Newest);

                    if (newest != null)
                    {
                        double timeDiff = new TimeSpan(newest.UpdateDate.Ticks - newest.ContentVersionDto.VersionDate.Ticks).TotalMilliseconds;
                        var hasPendingChanges = timeDiff > 2000;

                        if (hasPendingChanges == false && published != null)
                        {
                            published.Published = false;
                            toUpdate.Add(published);
                            newest.Published = true;
                            toUpdate.Add(newest);
                        }
                    }
                }

                //Commit the updated entries for the cmsDocument table
                using (var transaction = e.MigrationContext.Database.GetTransaction())
                {
                    //Loop through the toUpdate
                    foreach (var dto in toUpdate)
                    {
                        e.MigrationContext.Database.Update(dto);
                    }

                    transaction.Complete();
                }
            }
        }      
        
    }
}