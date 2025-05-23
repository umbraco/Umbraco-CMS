using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

[Obsolete("Remove in Umbraco 18.")]
public class MigrateNotificationCharsToStrings : MigrationBase
{
   public MigrateNotificationCharsToStrings(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        List<LegacyUser2NodeNotifyDto>? legacyNotifyDtos = Database.Fetch<LegacyUser2NodeNotifyDto>();
        Delete.Table(Constants.DatabaseSchema.Tables.User2NodeNotify).Do();

        Create.Table<User2NodeNotifyDto>().Do();
        var notifyDtos = legacyNotifyDtos.Select(ReplaceAction).ToList();
        Database.InsertBulk(notifyDtos);
    }

    private User2NodeNotifyDto ReplaceAction(LegacyUser2NodeNotifyDto dto)
    {
        // Action is non-nullable
        dto.Action = MigrateCharPermissionsToStrings.CharToStringPermissionDictionary.TryGetValue(dto.Action!.ToCharArray().First(), out var action) ? string.Join(string.Empty, action) : dto.Action;

        return new User2NodeNotifyDto
        {
            UserId = dto.UserId,
            NodeId = dto.NodeId,
            Action = dto.Action
        };
    }


    [TableName(Constants.DatabaseSchema.Tables.User2NodeNotify)]
    [PrimaryKey("userId", AutoIncrement = false)]
    [ExplicitColumns]
    private class LegacyUser2NodeNotifyDto
    {
        [Column("userId")]
        [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_umbracoUser2NodeNotify", OnColumns = "userId, nodeId, action")]
        [ForeignKey(typeof(UserDto))]
        public int UserId { get; set; }

        [Column("nodeId")]
        [ForeignKey(typeof(NodeDto))]
        public int NodeId { get; set; }
        [Column("action")]
        [SpecialDbType(SpecialDbTypes.NCHAR)]
        [Length(1)]

        public string? Action { get; set; }
    }
}
