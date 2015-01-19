using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    [Migration("7.3.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class AddRelationTypeForDocumentOnDelete : MigrationBase
    {
        public override void Up()
        {
            Insert.IntoTable("umbracoRelationType").Row(new
            {
                dual = false,
                parentObjectType = Guid.Parse(Constants.ObjectTypes.Document),
                childObjectType = Guid.Parse(Constants.ObjectTypes.Document),
                name = Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteName,
                alias = Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias
            });

        }

        public override void Down()
        {
        }
    }
}