using Umbraco.Core.Migrations.Install;

namespace Umbraco.Core.Migrations.Upgrade.V_8_6_0
{
    /// <summary>
    /// Ensures the new relation types are created
    /// </summary>
    public class AddNewRelationTypes : MigrationBase
    {
        public AddNewRelationTypes(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            CreateRelation(
                Constants.Conventions.RelationTypes.RelatedMediaAlias,
                Constants.Conventions.RelationTypes.RelatedMediaName);

            CreateRelation(
                Constants.Conventions.RelationTypes.RelatedDocumentAlias,
                Constants.Conventions.RelationTypes.RelatedDocumentName);
        }

        private void CreateRelation(string alias, string name)
        {
            var uniqueId = DatabaseDataCreator.CreateUniqueRelationTypeId(alias ,name); //this is the same as how it installs so everything is consistent
            Insert.IntoTable(Constants.DatabaseSchema.Tables.RelationType)
               .Row(new { typeUniqueId = uniqueId, dual = 0, name, alias })
               .Do();
        }
    }
}
