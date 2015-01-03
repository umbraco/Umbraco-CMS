using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    [Migration("7.3.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class AddRelationTypeForDocumentOnDelete : MigrationBase
    {
        public override void Up()
        {
            Execute.Code(AddRelationType);
        }

        public static string AddRelationType(Database database)
        {
            database.Insert("umbracoRelationType", "id", false, new RelationTypeDto { Id = 2, Alias = Constants.Conventions.Relations.RelateParentDocumentOnDelete, ChildObjectType = new Guid(Constants.ObjectTypes.Document), ParentObjectType = new Guid(Constants.ObjectTypes.Document), Dual = false, Name = "Relate Parent Document On Delete" });

            return string.Empty;
        }

        public override void Down()
        {
        }
    }
}