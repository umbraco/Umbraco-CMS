using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_5_0
{
    public class UpdateRelationTypeTable : MigrationBase
    {
        public UpdateRelationTypeTable(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {

            Alter.Table(Constants.DatabaseSchema.Tables.RelationType).AlterColumn("parentObjectType").AsGuid().Nullable();
            Alter.Table(Constants.DatabaseSchema.Tables.RelationType).AlterColumn("childObjectType").AsGuid().Nullable();

            //TODO: We have to update this field to ensure it's not null, we can just copy across the name since that is not nullable
            Alter.Table(Constants.DatabaseSchema.Tables.RelationType).AlterColumn("alias").AsString(100).NotNullable();
        }
    }
}
