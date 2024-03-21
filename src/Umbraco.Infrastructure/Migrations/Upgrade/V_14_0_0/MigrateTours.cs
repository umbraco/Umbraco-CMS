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
        using IScope scope = _scopeProvider.CreateScope();
        using IDisposable notificationSuppression = scope.Notifications.Suppress();
        ScopeDatabase(scope);

        // create table
        if (TableExists(Constants.DatabaseSchema.Tables.UserData))
        {
            return;
        }

        Create.Table<UserDataDto>().Do();

        // transform all existing UserTour fields in to userdata
        List<UserDto>? users = Database.Fetch<UserDto>();
        List<UserDataDto> userData = new List<UserDataDto>();
        foreach (UserDto user in users)
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

        scope.Complete();
    }

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
