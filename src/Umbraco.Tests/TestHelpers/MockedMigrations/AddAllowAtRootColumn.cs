using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.Migrations;

namespace Umbraco.Tests.TestHelpers.MockedMigrations
{
    /// <summary>
    /// Mocked Column Change that'll generate the following sql:
    /// ALTER TABLE [cmsContentType] 
    /// ADD [allowAtRoot] bit NOT NULL 
    /// CONSTRAINT [df_cmsContentType_allowAtRoot] DEFAULT 0
    /// </summary>
    public class AddAllowAtRootColumn : AddColumnChange
    {
        public override string TableName
        {
            get { return "cmsContentType"; }
        }

        public override string Version
        {
            get { return "4.10.0"; }
        }

        public override string ColumnName
        {
            get { return "allowAtRoot"; }
        }

        public override string Constraint
        {
            get { return "df_cmsContentType_allowAtRoot"; }
        }

        public override string DefaultForConstraint
        {
            get { return "0"; }
        }

        public override DatabaseTypes DatabaseType
        {
            get { return DatabaseTypes.Bool; }
        }

        public override NullSettings NullSetting
        {
            get { return NullSettings.NotNull; }
        }
    }
}