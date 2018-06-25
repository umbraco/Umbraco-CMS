using System;
using Umbraco.Core.Logging;
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
            var exists = Context.Database.FirstOrDefault<RelationTypeDtoCapture>("WHERE alias=@alias", new { alias = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias });
            if (exists == null)
            {
                Insert.IntoTable("umbracoRelationType").Row(new
                {
                    dual = false,
                    parentObjectType = Guid.Parse(Constants.ObjectTypes.Media),
                    childObjectType = Guid.Parse(Constants.ObjectTypes.Media),
                    name = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteName,
                    alias = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias
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
            public const int NodeIdSeed = 4;

            [Column("id")]
            [PrimaryKeyColumn(IdentitySeed = NodeIdSeed)]
            public int Id { get; set; }

            [Column("typeUniqueId")]
            [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoRelationType_UniqueId")]
            public Guid UniqueId { get; set; }

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
