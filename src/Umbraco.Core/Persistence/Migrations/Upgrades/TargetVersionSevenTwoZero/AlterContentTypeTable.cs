using System;
using System.Data;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven
{
    [Migration("7.2.0", 1, GlobalSettings.UmbracoMigrationName)]
    public class AlterContentTypeTable : MigrationBase
    {
        public override void Up()
        {
            if (Context == null || Context.Database == null) return;
           
            Upgrade();

        }

        private void Initial()
        {
            //new container config col
            Alter.Table("cmsContentType").AddColumn("containerConfig").AsString().Nullable();
        }

        public override void Down()
        {
            Delete.Column("containerConfig").FromTable("cmsContentType");
        }

        /// <summary>
        /// A custom class to map to so that we can linq to it easily without dynamics
        /// </summary>
        private class PropertyTypeReferenceDto
        {
            public int NodeId { get; set; }
            public int PropertyTypeId { get; set; }
        }
    }
}