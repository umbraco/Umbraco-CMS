using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class MakeTagsVariant : MigrationBase
    {
        public MakeTagsVariant(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            AddColumn<TagDto>("languageId");

            Delete.Index($"IX_{Constants.DatabaseSchema.Tables.Tag}").OnTable(Constants.DatabaseSchema.Tables.Tag).Do();
            Create.Index($"IX_{Constants.DatabaseSchema.Tables.Tag}").OnTable(Constants.DatabaseSchema.Tables.Tag)
                .OnColumn("group")
                .Ascending()
                .OnColumn("tag")
                .Ascending()
                .OnColumn("languageId")
                .Ascending()
                .WithOptions().Unique()
                .Do();
        }
    }
}
