using Umbraco.Core.Persistence.Migrations;

namespace Umbraco.Tests.TestHelpers.MockedMigrations
{
    /// <summary>
    /// Mocked Column Change that'll generate the following sql:
    /// ALTER TABLE [cmsContentType] DROP COLUMN [masterContentType];
    /// </summary>
    public class DropMasterContentTypeColumn : DropColumnChange
    {
        public override string Version
        {
            get { return "4.10.0"; }
        }

        public override string TableName
        {
            get { return "cmsContentType"; }
        }

        public override string ColumnName
        {
            get { return "masterContentType"; }
        }
    }
}