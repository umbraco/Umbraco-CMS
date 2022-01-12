using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_3_0
{
    public class AddExternalLoginWithKeys : MigrationBase
    {
        private readonly IUserService _userService;

        public AddExternalLoginWithKeys(IMigrationContext context, IUserService userService) : base(context)
        {
            _userService = userService;
        }

        protected override void Migrate()
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();
            if (!tables.InvariantContains(Constants.DatabaseSchema.Tables.ExternalLoginWithKey))
            {
                Create.Table<ExternalLoginWithKeyDto>().Do();
                List<ExternalLoginDto> oldEntities = Context.Database.Fetch<ExternalLoginDto>();


                var userIds = oldEntities.Select(x => x.UserId).ToArray();
                var userIdToKey = _userService.GetUsersById(userIds).ToDictionary(x => x.Id, x => x.Key);

                IEnumerable<ExternalLoginWithKeyDto> newEntities = oldEntities.Select(x => new ExternalLoginWithKeyDto
                {
                    CreateDate = x.CreateDate,
                    LoginProvider = x.LoginProvider,
                    ProviderKey = x.ProviderKey,
                    UserData = x.UserData,
                    UserOrMemberKey = userIdToKey[x.UserId],
                    Id = x.Id
                });
                Database.InsertBulk(newEntities);
            }

            if (!tables.InvariantContains(Constants.DatabaseSchema.Tables.ExternalLoginTokenWithKey))
            {
                Create.Table<ExternalLoginTokenWithKeyDto>().Do();

                List<ExternalLoginTokenWithKeyDto> oldEntities = Context.Database.Fetch<ExternalLoginTokenWithKeyDto>();
                IEnumerable<ExternalLoginTokenWithKeyDto> newEntities = oldEntities.Select(x =>
                    new ExternalLoginTokenWithKeyDto
                    {
                        CreateDate = x.CreateDate,
                        Id = x.Id,
                        Name = x.Name,
                        Value = x.Value,
                        ExternalLoginId = x.ExternalLoginId
                    });

                Database.InsertBulk(newEntities);
            }
        }
    }
}
