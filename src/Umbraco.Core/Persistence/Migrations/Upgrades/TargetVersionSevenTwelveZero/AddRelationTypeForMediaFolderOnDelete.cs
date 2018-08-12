using System;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenTwelveZero
{
    [Migration("7.12.0", 0, Constants.System.UmbracoMigrationName)]
    public class AddRelationTypeForMediaFolderOnDelete : MigrationBase
    {
        public AddRelationTypeForMediaFolderOnDelete(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            var exists = Context.Database.FirstOrDefault<RelationTypeDto>("WHERE alias=@alias", new { alias = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias });
            if (exists == null)
            {
                var uniqueId = (Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias + "____" + Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteName).ToGuid();
                Insert.IntoTable("umbracoRelationType").Row(new
                {
                    typeUniqueId = uniqueId,
                    alias = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias,
                    name = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteName,
                    childObjectType = Constants.ObjectTypes.MediaType,
                    parentObjectType = Constants.ObjectTypes.MediaType,
                    dual = false
                });
            }
        }

        public override void Down()
        { }
    }
}
