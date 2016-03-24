﻿using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.Strategies.Migrations
{
    /// <summary>
    /// This event ensures that upgrades from (configured) versions lower then 6.0.0
    /// have their publish state updated after the database schema has been migrated.
    /// </summary>
    public class PublishAfterUpgradeToVersionSixth : MigrationStartupHander
    {
        private readonly ISqlSyntaxProvider _sqlSyntax;

        public PublishAfterUpgradeToVersionSixth(ISqlSyntaxProvider sqlSyntax)
        {
            _sqlSyntax = sqlSyntax;
        }

        protected override void AfterMigration(MigrationRunner sender, MigrationEventArgs e)
        {
            if (e.ProductName != GlobalSettings.UmbracoMigrationName) return;

            var target = new Version(6, 0, 0);
            if (e.ConfiguredVersion < target)
            {
                var sql = new Sql();
                sql.Select("*")
                    .From<DocumentDto>(_sqlSyntax)
                    .InnerJoin<ContentVersionDto>(_sqlSyntax)
                    .On<DocumentDto, ContentVersionDto>(_sqlSyntax, left => left.VersionId, right => right.VersionId)
                    .InnerJoin<ContentDto>(_sqlSyntax)
                    .On<ContentVersionDto, ContentDto>(_sqlSyntax, left => left.NodeId, right => right.NodeId)
                    .InnerJoin<NodeDto>(_sqlSyntax)
                    .On<ContentDto, NodeDto>(_sqlSyntax, left => left.NodeId, right => right.NodeId)
                    .Where<NodeDto>(_sqlSyntax, x => x.NodeObjectType == new Guid(Constants.ObjectTypes.Document))
                    .Where<NodeDto>(_sqlSyntax, x => x.Path.StartsWith("-1"));

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