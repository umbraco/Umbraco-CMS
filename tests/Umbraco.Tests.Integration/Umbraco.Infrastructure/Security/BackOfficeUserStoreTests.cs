using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Security;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class BackOfficeUserStoreTests : UmbracoIntegrationTest
{
    private IEntityService EntityService => GetRequiredService<IEntityService>();

    private IExternalLoginWithKeyService ExternalLoginService => GetRequiredService<IExternalLoginWithKeyService>();

    private IUmbracoMapper UmbracoMapper => GetRequiredService<IUmbracoMapper>();

    private ILocalizedTextService TextService => GetRequiredService<ILocalizedTextService>();

    private ITwoFactorLoginService TwoFactorLoginService => GetRequiredService<ITwoFactorLoginService>();

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IUserRepository UserRepository => GetRequiredService<IUserRepository>();

    private IRuntimeState RuntimeState => GetRequiredService<IRuntimeState>();

    private IEventMessagesFactory EventMessagesFactory => GetRequiredService<IEventMessagesFactory>();

    private IBackOfficeUserReader BackOfficeUserReader => GetRequiredService<IBackOfficeUserReader>();

    private readonly ILogger<BackOfficeUserStore> _logger = NullLogger<BackOfficeUserStore>.Instance;

    [SetUp]
    public void ResetNotificationHandler() => UserNotificationHandler.SavingUser = null;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.AddNotificationHandler<UserSavingNotification, UserNotificationHandler>();

    private BackOfficeUserStore GetUserStore()
        => new(
            ScopeProvider,
            EntityService,
            ExternalLoginService,
            new TestOptionsSnapshot<GlobalSettings>(GlobalSettings),
            UmbracoMapper,
            new BackOfficeErrorDescriber(TextService),
            AppCaches,
            TwoFactorLoginService,
            UserGroupService,
            UserRepository,
            RuntimeState,
            EventMessagesFactory,
            _logger,
            BackOfficeUserReader);

    [Test]
    public async Task Can_Persist_Is_Approved()
    {
        var userStore = GetUserStore();
        var user = new BackOfficeIdentityUser(GlobalSettings, 1, [])
        {
            Name = "Test",
            Email = "test@test.com",
            UserName = "test@test.com",
        };
        var createResult = await userStore.CreateAsync(user);
        Assert.IsTrue(createResult.Succeeded);
        Assert.IsFalse(user.IsApproved);

        // update
        user.IsApproved = true;
        var saveResult = await userStore.UpdateAsync(user);
        Assert.IsTrue(saveResult.Succeeded);
        Assert.IsTrue(user.IsApproved);

        // get get
        user = await userStore.FindByIdAsync(user.Id);
        Assert.IsTrue(user.IsApproved);
    }

    [Test]
    public async Task CreateAsync_Returns_Failed_Result_When_Cancelled_By_Notification()
    {
        UserNotificationHandler.SavingUser = notification => notification.Cancel = true;

        var userStore = GetUserStore();
        var user = new BackOfficeIdentityUser(GlobalSettings, 1, [])
        {
            Name = "Test",
            Email = "test@test.com",
            UserName = "test@test.com",
        };

        var result = await userStore.CreateAsync(user);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Succeeded);
            Assert.IsTrue(result.Errors.Any(e => e.Code == nameof(UserOperationStatus.CancelledByNotification)));
        });
    }

    private class UserNotificationHandler : INotificationHandler<UserSavingNotification>
    {
        public static Action<UserSavingNotification>? SavingUser { get; set; }

        public void Handle(UserSavingNotification notification) => SavingUser?.Invoke(notification);
    }
}
