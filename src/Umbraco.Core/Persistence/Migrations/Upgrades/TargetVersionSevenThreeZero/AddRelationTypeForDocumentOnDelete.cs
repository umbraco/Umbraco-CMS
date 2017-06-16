using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    [Migration("7.3.0", 0, Constants.System.UmbracoMigrationName)]
    public class AddRelationTypeForDocumentOnDelete : MigrationBase
    {
        public AddRelationTypeForDocumentOnDelete(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            var exists = Context.Database.FirstOrDefault<RelationTypeDtoCapture>("WHERE alias=@alias", new {alias = Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias});
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
        { }

        // need to capture the DTO as it is modified in later migrations

        [TableName("umbracoRelationType")]
        [PrimaryKey("id")]
        [ExplicitColumns]
        internal class RelationTypeDtoCapture
        {
            public const int NodeIdSeed = 3;

            [Column("id")]
            [PrimaryKeyColumn(IdentitySeed = NodeIdSeed)]
            public int Id { get; set; }

            [Column("dual")]
            public bool Dual { get; set; }

            [Column("parentObjectType")]
            public Guid ParentObjectType { get; set; }

            [Column("childObjectType")]
            public Guid ChildObjectType { get; set; }

            [Column("name")]
            [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoRelationType_name")]
            public string Name { get; set; }

            [Column("alias")]
            [NullSetting(NullSetting = NullSettings.Null)]
            [Length(100)]
            [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoRelationType_alias")]
            public string Alias { get; set; }
        }
    }
}