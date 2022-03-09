using System.Linq;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_10_0_0;

public class AddMemberPropertiesAsColumns : MigrationBase
{
    public AddMemberPropertiesAsColumns(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();

        AddColumnIfNotExists<MemberDto>(columns, "failedPasswordAttempts");
        AddColumnIfNotExists<MemberDto>(columns, "isLockedOut");
        AddColumnIfNotExists<MemberDto>(columns, "isApproved");
        AddColumnIfNotExists<MemberDto>(columns, "lastLoginDate");
        AddColumnIfNotExists<MemberDto>(columns, "lastLockoutDate");
        AddColumnIfNotExists<MemberDto>(columns, "lastPasswordChangeDate");

    }
}
