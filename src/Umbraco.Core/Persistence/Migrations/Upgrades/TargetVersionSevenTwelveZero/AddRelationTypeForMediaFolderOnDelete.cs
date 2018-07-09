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
                var relationTypeDto = new RelationTypeDto
                {
                    Alias = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias,
                    Name = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteName,
                    ChildObjectType = Guid.Parse(Constants.ObjectTypes.MediaType),
                    ParentObjectType = Guid.Parse(Constants.ObjectTypes.MediaType),
                    Dual = false
                };

                Context.Database.Insert(relationTypeDto);
            }
        }

        public override void Down()
        { }
    }
}