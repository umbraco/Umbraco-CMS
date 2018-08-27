using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_7_12_0
{
    public class AddRelationTypeForMediaFolderOnDelete : MigrationBase
    {

        public AddRelationTypeForMediaFolderOnDelete(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            var relationTypeCount = Context.Database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoRelationType WHERE alias=@alias",
                new { alias = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias });

            if (relationTypeCount > 0)
                return;

            var uniqueId = (Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias + "____" + Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteName).ToGuid();
            Insert.IntoTable("umbracoRelationType").Row(new
            {
                typeUniqueId = uniqueId,
                alias = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias,
                name = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteName,
                childObjectType = Constants.ObjectTypes.MediaType,
                parentObjectType = Constants.ObjectTypes.MediaType,
                dual = false
                }).Do();
        }

    }
}
