using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Editors;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public partial class UserServiceCrudTests : UmbracoIntegrationTestWithContent
{
    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IEntityService EntityService => GetRequiredService<IEntityService>();

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);
        services.RemoveAll<IInviteUriProvider>();
        services.AddScoped<IInviteUriProvider, TestUriProvider>();
    }

    // This is resolved from the service scope, so we have to add it to the service collection.
    private class TestUriProvider : IInviteUriProvider
    {
        public Task<Attempt<Uri, UserOperationStatus>> CreateInviteUriAsync(IUser invitee)
        {
            var fakePath = "https://localhost:44331/fakeInviteEndpoint";
            Attempt<Uri, UserOperationStatus> attempt = Attempt<Uri, UserOperationStatus>.Succeed(UserOperationStatus.Success, new Uri(fakePath));
            return Task.FromResult(attempt);
        }
    }

    private IUserService CreateUserService(
        SecuritySettings? securitySettings = null,
        IUserInviteSender? inviteSender = null,
        ILocalLoginSettingProvider? localLoginSettingProvider = null)
    {
        securitySettings ??= GetRequiredService<IOptions<SecuritySettings>>().Value;
        IOptions<SecuritySettings> securityOptions = Options.Create(securitySettings);

        if (inviteSender is null)
        {
            var senderMock = new Mock<IUserInviteSender>();
            senderMock.Setup(x => x.CanSendInvites()).Returns(true);
            inviteSender = senderMock.Object;
        }

        localLoginSettingProvider ??= GetRequiredService<ILocalLoginSettingProvider>();

        return new UserService(
            GetRequiredService<ICoreScopeProvider>(),
            GetRequiredService<ILoggerFactory>(),
            GetRequiredService<IEventMessagesFactory>(),
            GetRequiredService<IUserRepository>(),
            GetRequiredService<IUserGroupRepository>(),
            GetRequiredService<IOptions<GlobalSettings>>(),
            securityOptions,
            GetRequiredService<UserEditorAuthorizationHelper>(),
            GetRequiredService<IServiceScopeFactory>(),
            GetRequiredService<IEntityService>(),
            localLoginSettingProvider,
            inviteSender,
            GetRequiredService<MediaFileManager>(),
            GetRequiredService<ITemporaryFileService>(),
            GetRequiredService<IShortStringHelper>(),
            GetRequiredService<IOptions<ContentSettings>>(),
            GetRequiredService<IIsoCodeValidator>(),
            GetRequiredService<IUserForgotPasswordSender>(),
            GetRequiredService<IUserIdKeyResolver>());
    }


}
