using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    [Migration("7.3.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class AddRelationTypeForDocumentOnDelete : MigrationBase
    {
        public AddRelationTypeForDocumentOnDelete(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            var exists = Context.Database.FirstOrDefault<RelationTypeDto>("WHERE alias=@alias", new {alias = Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias});
            if (exists == null)
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

            

        }

        public override void Down()
        {
        }
    }
}