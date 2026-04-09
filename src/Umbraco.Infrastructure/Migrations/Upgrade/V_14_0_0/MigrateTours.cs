using System.Runtime.Serialization;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

internal class MigrateTours : UnscopedMigrationBase
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IScopeProvider _scopeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrateTours"/> class.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> providing migration state and services.</param>
    /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/> used for serializing and deserializing JSON data.</param>
    /// <param name="scopeProvider">The <see cref="IScopeProvider"/> that manages database transaction scopes.</param>
    public MigrateTours(
        IMigrationContext context,
        IJsonSerializer jsonSerializer,
        IScopeProvider scopeProvider)
        : base(context)
    {
        _jsonSerializer = jsonSerializer;
        _scopeProvider = scopeProvider;
    }

    protected override void Migrate()
    {
        // if the table already exists, do nothing
        if (TableExists(Constants.DatabaseSchema.Tables.UserData))
        {
            Context.Complete();
            return;
        }

        using IScope scope = _scopeProvider.CreateScope();
        using IDisposable notificationSuppression = scope.Notifications.Suppress();
        ScopeDatabase(scope);

        // create table
        Create.Table<UserDataDto>().Do();

        // transform all existing UserTour fields in to userdata
        List<OldUserDto>? users = Database.Fetch<OldUserDto>();
        List<UserDataDto> userData = new List<UserDataDto>();
        foreach (OldUserDto user in users)
        {
            if (user.TourData is null)
            {
                continue;
            }

            TourData[]? tourData = _jsonSerializer.Deserialize<TourData[]>(user.TourData);
            if (tourData is null)
            {
                // invalid value
                continue;
            }

            foreach (TourData data in tourData)
            {
                var userDataFromTour = new UserDataDto
                {
                    Key = Guid.NewGuid(),
                    UserKey = user.Key,
                    Group = "umbraco.tours",
                    Identifier = data.Alias,
                    Value = _jsonSerializer.Serialize(new TourValue
                    {
                        Completed = data.Completed,
                        Disabled = data.Disabled,
                    }),
                };
                userData.Add(userDataFromTour);
            }
        }

        Database.InsertBulk(userData);

        // remove old column
        if (DatabaseType != DatabaseType.SQLite)
        {
            MigrateUserTableSqlServer();
            Context.Complete();
            scope.Complete();
            return;
        }

        MigrateUserTableSqlite();
        Context.Complete();
        scope.Complete();
    }

    private void MigrateUserTableSqlite()
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

        List<NewUserDto> users = Database.Fetch<OldUserDto>().Select(x => new NewUserDto
        {
            Id = x.Id,
            Key = x.Id is -1 ? Constants.Security.SuperUserKey : Guid.NewGuid(),
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
        }).ToList();

        Delete.Table(Constants.DatabaseSchema.Tables.User).Do();
        Create.Table<NewUserDto>().Do();

        // We have to insert one at a time to be able to not auto increment the id.
        foreach (NewUserDto user in users)
        {
            Database.Insert(Constants.DatabaseSchema.Tables.User, "id", false, user);
        }
    }

    private void MigrateUserTableSqlServer()
    {
        Delete.Column("tourData").FromTable(Constants.DatabaseSchema.Tables.User).Do();
    }


    /// <summary>
    /// Represents a data transfer object for an old user, used during the migration process in version 14.0.0.
    /// </summary>
    [TableName(TableName)]
    [PrimaryKey("id", AutoIncrement = true)]
    [ExplicitColumns]
    public class OldUserDto
    {
        public const string TableName = Constants.DatabaseSchema.Tables.User;

        /// <summary>
        /// Initializes a new instance of the <see cref="OldUserDto"/> class with default values.
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
        /// Gets or sets a value indicating whether the user is disabled.
        /// </summary>
        [Column("userDisabled")]
        [Constraint(Default = "0")]
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or sets the unique key identifier for the user.
        /// </summary>
        [Column("key")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Constraint(Default = SystemMethods.NewGuid)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoUser_userKey")]
        public Guid Key { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether console access is disabled for the user.
        /// A value of <c>true</c> means the user cannot access the console.
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
        /// Gets or sets the password associated with the old user.
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
        /// Gets or sets the preferred language of the user, or <c>null</c> if not specified.
        /// </summary>
        [Column("userLanguage")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(10)]
        public string? UserLanguage { get; set; }

        /// <summary>
        /// Gets or sets the security stamp token used to validate the user's security credentials.
        /// </summary>
        [Column("securityStampToken")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(255)]
        public string? SecurityStampToken { get; set; }

        /// <summary>Gets or sets the number of failed login attempts for the user.</summary>
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
        /// Gets or sets the date and time of the user's last login.
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
        /// Gets or sets the date and time when the user was invited.
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
        ///     A Json blob stored for recording tour data for a user
        /// </summary>
        [Column("tourData")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        public string? TourData { get; set; }

        /// <summary>
        /// Gets or sets the user groups associated with this user.
        /// </summary>
        [ResultColumn]
        [Reference(ReferenceType.Many, ReferenceMemberName = "UserId")]
        public List<UserGroupDto> UserGroupDtos { get; set; }

        /// <summary>
        /// Gets or sets the collection of start node DTOs for the user.
        /// </summary>
        [ResultColumn]
        [Reference(ReferenceType.Many, ReferenceMemberName = "UserId")]
        public HashSet<UserStartNodeDto> UserStartNodeDtos { get; set; }
    }

    /// <summary>
    /// Represents a data transfer object (DTO) containing information about a new user created during the migration tours process.
    /// </summary>
    [TableName(TableName)]
    [PrimaryKey("id", AutoIncrement = true)]
    [ExplicitColumns]
    public class NewUserDto
    {
        public const string TableName = Constants.DatabaseSchema.Tables.User;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewUserDto"/> class with default values.
        /// </summary>
        public NewUserDto()
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
        /// Gets or sets a value indicating whether the user is disabled.
        /// </summary>
        [Column("userDisabled")]
        [Constraint(Default = "0")]
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or sets the unique key identifier for the user.
        /// </summary>
        [Column("key")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Constraint(Default = SystemMethods.NewGuid)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoUser_userKey")]
        public Guid Key { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is denied access to the console.
        /// A value of <c>true</c> means the user cannot access the console.
        /// </summary>
        [Column("userNoConsole")]
        [Constraint(Default = "0")]
        public bool NoConsole { get; set; }

        /// <summary>
        /// Gets or sets the username associated with the new user.
        /// </summary>
        [Column("userName")]
        public string UserName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the login name of the new user.
        /// </summary>
        [Column("userLogin")]
        [Length(125)]
        [Index(IndexTypes.NonClustered)]
        public string? Login { get; set; }

        /// <summary>
        /// Gets or sets the password of the new user.
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

        /// <summary>Gets or sets the email address of the user.</summary>
        [Column("userEmail")]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Gets or sets the user's preferred language, represented as a language code (e.g., "en-US").
        /// </summary>
        [Column("userLanguage")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(10)]
        public string? UserLanguage { get; set; }

        /// <summary>
        /// Gets or sets the security stamp token used to identify changes to the user's security credentials.
        /// This token is typically updated when sensitive user information, such as a password, is changed.
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
        /// Gets or sets the creation date of the user.
        /// </summary>
        [Column("createDate")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Constraint(Default = SystemMethods.CurrentDateTime)]
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the date when the user was last updated.
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
        /// Gets or sets the list of user group DTOs associated with the user.
        /// </summary>
        [ResultColumn]
        [Reference(ReferenceType.Many, ReferenceMemberName = "UserId")]
        public List<UserGroupDto> UserGroupDtos { get; set; }

        /// <summary>
        /// Gets or sets the collection of start node DTOs for this user.
        /// </summary>
        [ResultColumn]
        [Reference(ReferenceType.Many, ReferenceMemberName = "UserId")]
        public HashSet<UserStartNodeDto> UserStartNodeDtos { get; set; }
    }

    /// <summary>
    /// Data structure representing a tour used during the migration process.
    /// </summary>
    public class TourData()
    {
        /// <summary>
        ///     The tour alias
        /// </summary>
        [DataMember(Name = "alias")]
        public string Alias { get; set; } = string.Empty;

        /// <summary>
        ///     If the tour is completed
        /// </summary>
        [DataMember(Name = "completed")]
        public bool Completed { get; set; }

        /// <summary>
        ///     If the tour is disabled
        /// </summary>
        [DataMember(Name = "disabled")]
        public bool Disabled { get; set; }
    }

    /// <summary>
    /// Represents a value associated with a tour during the migration process in Umbraco upgrades.
    /// </summary>
    public class TourValue()
    {
        /// <summary>
        ///     If the tour is completed
        /// </summary>
        [DataMember(Name = "completed")]
        public bool Completed { get; set; }

        /// <summary>
        ///     If the tour is disabled
        /// </summary>
        [DataMember(Name = "disabled")]
        public bool Disabled { get; set; }
    }
}
