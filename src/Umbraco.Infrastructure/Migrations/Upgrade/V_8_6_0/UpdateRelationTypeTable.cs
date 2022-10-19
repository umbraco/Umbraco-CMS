using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_6_0;

public class UpdateRelationTypeTable : MigrationBase
{
    public UpdateRelationTypeTable(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        Alter.Table(Constants.DatabaseSchema.Tables.RelationType).AlterColumn("parentObjectType").AsGuid().Nullable()
            .Do();
        Alter.Table(Constants.DatabaseSchema.Tables.RelationType).AlterColumn("childObjectType").AsGuid().Nullable()
            .Do();

        // TODO: We have to update this field to ensure it's not null, we can just copy across the name since that is not nullable

        // drop index before we can alter the column
        if (IndexExists("IX_umbracoRelationType_alias"))
        {
            Delete
                .Index("IX_umbracoRelationType_alias")
                .OnTable(Constants.DatabaseSchema.Tables.RelationType)
                .Do();
        }

        // change the column to non nullable
        Alter.Table(Constants.DatabaseSchema.Tables.RelationType).AlterColumn("alias").AsString(100).NotNullable().Do();

        // re-create the index
        Create
            .Index("IX_umbracoRelationType_alias")
            .OnTable(Constants.DatabaseSchema.Tables.RelationType)
            .OnColumn("alias")
            .Ascending()
            .WithOptions().Unique().WithOptions().NonClustered()
            .Do();
    }
}
