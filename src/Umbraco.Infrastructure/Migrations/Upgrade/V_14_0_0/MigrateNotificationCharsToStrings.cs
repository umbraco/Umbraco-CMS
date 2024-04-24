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
    private static Dictionary<string, string> _charToStringPermissionDictionary =
        new()
        {
            ["I"] = ActionAssignDomain.ActionLetter,
            ["F"] = ActionBrowse.ActionLetter,
            ["O"] = ActionCopy.ActionLetter,
            ["ï"] = ActionCreateBlueprintFromContent.ActionLetter,
            ["D"] = ActionDelete.ActionLetter,
            ["M"] = ActionMove.ActionLetter,
            ["C"] = ActionNew.ActionLetter,
            ["N"] = ActionNotify.ActionLetter,
            ["P"] = ActionProtect.ActionLetter,
            ["U"] = ActionPublish.ActionLetter,
            ["V"] = ActionRestore.ActionLetter,
            ["R"] = ActionRights.ActionLetter,
            ["K"] = ActionRollback.ActionLetter,
            ["S"] = ActionSort.ActionLetter,
            ["Z"] = ActionUnpublish.ActionLetter,
            ["A"] = ActionUpdate.ActionLetter,
        };

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
        // When we expanded the column it padded it with space, so we need to strip it.
        dto.Action = _charToStringPermissionDictionary.TryGetValue(dto.Action!.StripWhitespace(), out var action) ? action : dto.Action?.StripWhitespace();

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
