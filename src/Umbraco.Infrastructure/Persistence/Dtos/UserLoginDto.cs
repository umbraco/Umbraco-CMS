using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
internal sealed class UserLoginDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.UserLogin;
    public const string PrimaryKeyColumnName = "sessionId";

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false)]
    public Guid SessionId { get; set; }

    [Column("userId")]
    [ForeignKey(typeof(UserDto), Name = "FK_" + TableName + "_umbracoUser_id")]
    public int? UserId { get; set; }

    /// <summary>
    ///     Tracks when the session is created
    /// </summary>
    [Column("loggedInUtc")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public DateTime LoggedIn { get; set; }

    /// <summary>
    ///     Updated every time a user's session is validated
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This allows us to guess if a session is timed out if a user doesn't actively
    ///         log out and also allows us to trim the data in the table.
    ///     </para>
    ///     <para>
    ///         The index is IMPORTANT as it prevents deadlocks during deletion of
    ///         old sessions (DELETE ... WHERE lastValidatedUtc &lt; date).
    ///     </para>
    /// </remarks>
    [Column("lastValidatedUtc")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.NonClustered, Name = "IX_umbracoUserLogin_lastValidatedUtc")]
    public DateTime LastValidated { get; set; }

    /// <summary>
    ///     Tracks when the session is removed when the user's account is logged out
    /// </summary>
    [Column("loggedOutUtc")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? LoggedOut { get; set; }

    /// <summary>
    ///     Logs the IP address of the session if available
    /// </summary>
    [Column("ipAddress")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? IpAddress { get; set; }
}
