using System.Linq;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;


namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_4_0
{
    internal class UpdateRelationTypesToHandleDependencies : MigrationBase
    {
        public UpdateRelationTypesToHandleDependencies(IMigrationContext context)
            : base(context)
        {
        }

        protected override void Migrate()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

            AddColumnIfNotExists<RelationTypeDto>(columns, "isDependency");

            var aliasesWithDependencies = new[]
            {
                Core.Constants.Conventions.RelationTypes.RelatedDocumentAlias,
                Core.Constants.Conventions.RelationTypes.RelatedMediaAlias
            };

            Database.Execute(
                Sql()
                    .Update<RelationTypeDto>(u => u.Set(x => x.IsDependency, true))
                    .WhereIn<RelationTypeDto>(x => x.Alias, aliasesWithDependencies));

        }
    }
}
