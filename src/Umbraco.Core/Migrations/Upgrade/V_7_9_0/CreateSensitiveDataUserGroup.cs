using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_7_9_0
{
    internal class CreateSensitiveDataUserGroup : MigrationBase
    {
        public CreateSensitiveDataUserGroup(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var sql = Sql()
                .SelectCount()
                .From<UserGroupDto>()
                .Where<UserGroupDto>(x => x.Alias == Constants.Security.SensitiveDataGroupAlias);

            var exists = Database.ExecuteScalar<int>(sql) > 0;
            if (exists) return;

            var groupId = Database.Insert(Constants.DatabaseSchema.Tables.UserGroup, "id", new UserGroupDto { StartMediaId = -1, StartContentId = -1, Alias = Constants.Security.SensitiveDataGroupAlias, Name = "Sensitive data", DefaultPermissions = "", CreateDate = DateTime.Now, UpdateDate = DateTime.Now, Icon = "icon-lock" });
            Database.Insert(new User2UserGroupDto { UserGroupId = Convert.ToInt32(groupId), UserId = Constants.Security.SuperUserId }); // add super to sensitive data
        }
    }
}
