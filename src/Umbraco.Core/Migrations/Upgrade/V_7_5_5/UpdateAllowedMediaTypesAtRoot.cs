namespace Umbraco.Core.Migrations.Upgrade.V_7_5_5
{
    /// <summary>
    /// See: http://issues.umbraco.org/issue/U4-4196
    /// </summary>
    public class UpdateAllowedMediaTypesAtRoot : MigrationBase
    {
        public UpdateAllowedMediaTypesAtRoot(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            Database.Execute("UPDATE cmsContentType SET allowAtRoot = 1 WHERE nodeId = 1032 OR nodeId = 1033");
        }
    }
}
