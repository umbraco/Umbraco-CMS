using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

public class RenameMediaVersionTable : MigrationBase
{
    public RenameMediaVersionTable(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        Rename.Table("cmsMedia").To(Constants.DatabaseSchema.Tables.MediaVersion).Do();

        // that is not supported on SqlCE
        // Rename.Column("versionId").OnTable(Constants.DatabaseSchema.Tables.MediaVersion).To("id").Do();
        AddColumn<MediaVersionDto>("id", out IEnumerable<string> sqls);

        Database.Execute($@"UPDATE {Constants.DatabaseSchema.Tables.MediaVersion} SET id=v.id
FROM {Constants.DatabaseSchema.Tables.MediaVersion} m
JOIN cmsContentVersion v on m.versionId = v.versionId
JOIN umbracoNode n on v.contentId=n.id
WHERE n.nodeObjectType='{Constants.ObjectTypes.Media}'");

        foreach (var sql in sqls)
        {
            Execute.Sql(sql).Do();
        }

        AddColumn<MediaVersionDto>("path", out sqls);

        Execute.Sql($"UPDATE {Constants.DatabaseSchema.Tables.MediaVersion} SET path=mediaPath").Do();

        foreach (var sql in sqls)
        {
            Execute.Sql(sql).Do();
        }

        // we had to run sqls to get the NULL constraints, but we need to get rid of most
        Delete.KeysAndIndexes(Constants.DatabaseSchema.Tables.MediaVersion).Do();

        Delete.Column("mediaPath").FromTable(Constants.DatabaseSchema.Tables.MediaVersion).Do();
        Delete.Column("versionId").FromTable(Constants.DatabaseSchema.Tables.MediaVersion).Do();
        Delete.Column("nodeId").FromTable(Constants.DatabaseSchema.Tables.MediaVersion).Do();
    }
}
