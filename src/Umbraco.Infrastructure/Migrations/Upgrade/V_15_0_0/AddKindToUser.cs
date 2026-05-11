using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0;

/// <summary>
/// Represents a migration that adds the <c>Kind</c> column to the <c>User</c> table in the database as part of the upgrade to version 15.0.0.
/// </summary>
[Obsolete("Remove in Umbraco 18.")]
public class AddKindToUser : UnscopedMigrationBase
{
    private const string NewColumnName = "kind";
    private readonly IScopeProvider _scopeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.AddKindToUser"/> class.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> for the migration operation.</param>
    /// <param name="scopeProvider">The <see cref="IScopeProvider"/> used to manage database scopes.</param>
    public AddKindToUser(IMigrationContext context, IScopeProvider scopeProvider)
        : base(context)
        => _scopeProvider = scopeProvider;

    protected override void Migrate()
    {
        // If the new column already exists we'll do nothing.
        if (ColumnExists(Constants.DatabaseSchema.Tables.User, NewColumnName))
        {
            Context.Complete();
            return;
        }

        InvalidateBackofficeUserAccess = true;

        using IScope scope = _scopeProvider.CreateScope();
        using IDisposable notificationSuppression = scope.Notifications.Suppress();
        ScopeDatabase(scope);

        // SQL server can simply add the column, but for SQLite this won't work,
        // so we'll have to create a new table and copy over data.
        if (DatabaseType != DatabaseType.SQLite)
        {
            MigrateSqlServer();
        }
        else
        {
            MigrateSqlite();
        }

        Context.Complete();
        scope.Complete();
    }

    private void MigrateSqlServer()
    {
        var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();
        AddColumnIfNotExists<UserDto>(columns, NewColumnName);
    }

    private void MigrateSqlite()
    {
        /*
         * We commit the initial transaction started by the scope. This is required in order to disable the foreign keys.
         * We then begin a new transaction, this transaction will be committed or rolled back by the scope, like normal.
         * We don't have to worry about re-enabling the foreign keys, since these are enabled by default every time a connection is established.
         *
         * Ideally we'd want to do this with the unscoped database we get, however, this cannot be done,
         * since our scoped database cannot share a connection with the unscoped database, so a new one will be created, which enables the foreign keys.
         * Similarly we cannot use Database.CompleteTransaction(); since this also closes the connection,
         * so starting a new transaction would re-enable foreign keys.
         */
        Database.Execute("COMMIT;");
        Database.Execute("PRAGMA foreign_keys=off;");
        Database.Execute("BEGIN TRANSACTION;");

        IEnumerable<UserDto> users = Database.Fetch<OldUserDto>().Select(x => new UserDto
        {
            Id = x.Id,
            Key = x.Key,
            Disabled = x.Disabled,
            NoConsole = x.NoConsole,
            UserName = x.UserName,
            Login = x.Login,
            Password = x.Password,
            PasswordConfig = x.PasswordConfig,
            Email = x.Email,
            UserLanguage = x.UserLanguage,
            SecurityStampToken = x.SecurityStampToken,
            FailedLoginAttempts = x.FailedLoginAttempts,
            LastLockoutDate = x.LastLockoutDate,
            LastPasswordChangeDate = x.LastPasswordChangeDate,
            LastLoginDate = x.LastLoginDate,
            EmailConfirmedDate = x.EmailConfirmedDate,
            InvitedDate = x.InvitedDate,
            CreateDate = x.CreateDate,
            UpdateDate = x.UpdateDate,
            Avatar = x.Avatar,
            Kind = 0
        });

        Delete.Table(Constants.DatabaseSchema.Tables.User).Do();
        Create.Table<UserDto>().Do();

        foreach (UserDto user in users)
        {
            Database.Insert(Constants.DatabaseSchema.Tables.User, "id", false, user);
        }
    }

    /// <summary>
    /// Represents the legacy user data transfer object used during the migration that adds the Kind property to users.
    /// </summary>
    [TableName(TableName)]
    [PrimaryKey("id", AutoIncrement = true)]
    [ExplicitColumns]
    public class OldUserDto
    {
        public const string TableName = Constants.DatabaseSchema.Tables.User;

        /// <summary>
        /// Initializes a new instance of <see cref="OldUserDto"/>.
        /// </summary>
        public OldUserDto()
        {
            UserGroupDtos = new List<UserGroupDto>();
            UserStartNodeDtos = new HashSet<UserStartNodeDto>();
        }

        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        [Column("id")]
        [PrimaryKeyColumn(Name = "PK_user")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user account is disabled.
        /// </summary>
        [Column("userDisabled")]
        [Constraint(Default = "0")]
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier (key) for the user.
        /// </summary>
        [Column("key")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Constraint(Default = SystemMethods.NewGuid)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoUser_userKey")]
        public Guid Key { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is prevented from accessing the console.
        /// </summary>
        [Column("userNoConsole")]
        [Constraint(Default = "0")]
        public bool NoConsole { get; set; }

        /// <summary>
        /// Gets or sets the username associated with the user.
        /// </summary>
        [Column("userName")]
        public string UserName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the login name of the user.
        /// </summary>
        [Column("userLogin")]
        [Length(125)]
        [Index(IndexTypes.NonClustered)]
        public string? Login { get; set; }

        /// <summary>
        /// Gets or sets the user's password (hashed or encrypted as stored in the database).
        /// </summary>
        [Column("userPassword")]
        [Length(500)]
        public string? Password { get; set; }

        /// <summary>
        ///     This will represent a JSON structure of how the password has been created (i.e hash algorithm, iterations)
        /// </summary>
        [Column("passwordConfig")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(500)]
        public string? PasswordConfig { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        [Column("userEmail")]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Gets or sets the ISO language code associated with the user.
        /// </summary>
        [Column("userLanguage")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(10)]
        public string? UserLanguage { get; set; }

        /// <summary>
        /// Gets or sets the security stamp token associated with the user, typically used for validating security-related operations such as password resets or authentication changes.
        /// </summary>
        [Column("securityStampToken")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(255)]
        public string? SecurityStampToken { get; set; }

        /// <summary>
        /// Gets or sets the number of failed login attempts for the user.
        /// </summary>
        [Column("failedLoginAttempts")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? FailedLoginAttempts { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user was last locked out.
        /// </summary>
        [Column("lastLockoutDate")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? LastLockoutDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user's password was last changed.
        /// </summary>
        [Column("lastPasswordChangeDate")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? LastPasswordChangeDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user last logged in.
        /// </summary>
        [Column("lastLoginDate")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user's email was confirmed.
        /// </summary>
        [Column("emailConfirmedDate")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? EmailConfirmedDate { get; set; }

        /// <summary>
        /// Gets or sets the date when the user was invited.
        /// </summary>
        [Column("invitedDate")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public DateTime? InvitedDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user was created.
        /// </summary>
        [Column("createDate")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Constraint(Default = SystemMethods.CurrentDateTime)]
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the date and time when the user was last updated.
        /// </summary>
        [Column("updateDate")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Constraint(Default = SystemMethods.CurrentDateTime)]
        public DateTime UpdateDate { get; set; } = DateTime.Now;

        /// <summary>
        ///     Will hold the media file system relative path of the users custom avatar if they uploaded one
        /// </summary>
        [Column("avatar")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(500)]
        public string? Avatar { get; set; }

        /// <summary>
        /// Gets or sets the collection of user group DTOs to which the user belongs.
        /// </summary>
        [ResultColumn]
        [Reference(ReferenceType.Many, ReferenceMemberName = "UserId")]
        public List<UserGroupDto> UserGroupDtos { get; set; }

        /// <summary>
        /// Gets or sets the collection of start node DTOs linked to the user.
        /// </summary>
        [ResultColumn]
        [Reference(ReferenceType.Many, ReferenceMemberName = "UserId")]
        public HashSet<UserStartNodeDto> UserStartNodeDtos { get; set; }
    }
}
