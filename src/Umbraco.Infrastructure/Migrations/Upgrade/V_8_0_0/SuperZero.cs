using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

public class SuperZero : MigrationBase
{
    public SuperZero(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        var exists = Database.Fetch<int>("select id from umbracoUser where id=-1;").Count > 0;
        if (exists)
        {
            return;
        }

        Database.Execute("update umbracoUser set userLogin = userLogin + '__' where id=0");

        Database.Execute("set identity_insert umbracoUser on;");
        Database.Execute(@"
                insert into umbracoUser (id,
                    userDisabled, userNoConsole, userName, userLogin, userPassword, passwordConfig,
                    userEmail, userLanguage, securityStampToken, failedLoginAttempts, lastLockoutDate,
	                lastPasswordChangeDate, lastLoginDate, emailConfirmedDate, invitedDate,
	                createDate, updateDate, avatar, tourData)
                select
                    -1 id,
                    userDisabled, userNoConsole, userName, substring(userLogin, 1, len(userLogin) - 2) userLogin, userPassword, passwordConfig,
	                userEmail, userLanguage, securityStampToken, failedLoginAttempts, lastLockoutDate,
	                lastPasswordChangeDate, lastLoginDate, emailConfirmedDate, invitedDate,
	                createDate, updateDate, avatar, tourData
                from umbracoUser where id=0;");
        Database.Execute("set identity_insert umbracoUser off;");

        Database.Execute("update umbracoUser2UserGroup set userId=-1 where userId=0;");
        Database.Execute("update umbracoUser2NodeNotify set userId=-1 where userId=0;");
        Database.Execute("update umbracoNode set nodeUser=-1 where nodeUser=0;");
        Database.Execute("update umbracoUserLogin set userId=-1 where userId=0;");
        Database.Execute($"update {Constants.DatabaseSchema.Tables.ContentVersion} set userId=-1 where userId=0;");
        Database.Execute("delete from umbracoUser where id=0;");
    }
}
