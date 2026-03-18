using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

/// <summary>
/// Migration that updates notification-related database fields from single character values to string values
/// as part of the upgrade process to Umbraco version 14.0.0. This ensures compatibility with the new data model.
/// </summary>
[Obsolete("Remove in Umbraco 18.")]
public class MigrateNotificationCharsToStrings : MigrationBase
{
   /// <summary>
   /// Initializes a new instance of the <see cref="MigrateNotificationCharsToStrings"/> class with the specified migration context.
   /// </summary>
   /// <param name="context">The <see cref="IMigrationContext"/> to be used for the migration.</param>
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
        /// <summary>
        /// Gets or sets the identifier of the associated user.
        /// </summary>
        [Column("userId")]
        [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_umbracoUser2NodeNotify", OnColumns = "userId, nodeId, action")]
        [ForeignKey(typeof(UserDto))]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the related node entity.
        /// </summary>
        [Column("nodeId")]
        [ForeignKey(typeof(NodeDto))]
        public int NodeId { get; set; }

        /// <summary>
        /// Gets or sets the action code representing the notification action. This is typically a single-character string.
        /// </summary>
        [Column("action")]
        [SpecialDbType(SpecialDbTypes.NCHAR)]
        [Length(1)]

        public string? Action { get; set; }
    }
}
