using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class HybridMemberCacheTests : UmbracoIntegrationTest
{
    private IPublishedMemberHybridCache PublishedMemberHybridCache => GetRequiredService<IPublishedMemberHybridCache>();

    private IMemberEditingService MemberEditingService => GetRequiredService<IMemberEditingService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    private IMemberGroupService MemberGroupService => GetRequiredService<IMemberGroupService>();

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.AddHybridCache();
        services.AddSingleton<IPublishedMemberHybridCache, MemberCache>();
        services.AddSingleton<IMemberCacheService, MemberCacheService>();
        services.AddSingleton<IPropertyCacheCompressionOptions, NoopPropertyCacheCompressionOptions>();
        services.AddNotificationAsyncHandler<ContentRefreshNotification, CacheRefreshingNotificationHandler>();
        services.AddTransient<IPublishedContentFactory, PublishedContentFactory>();
    }

    [Test]
    public async Task Can_Get_Draft_Content_By_Id()
    {
        Guid key = Guid.NewGuid();
        await CreateMemberAsync(key);

        // Act
        var member = await PublishedMemberHybridCache.GetById(key);

        // Assert
        Assert.IsNotNull(member);
    }

    private async Task CreateMemberAsync(Guid? key = null, bool titleIsSensitive = false)
    {
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        memberType.SetIsSensitiveProperty("title", titleIsSensitive);
        MemberTypeService.Save(memberType);
        MemberService.AddRole("RoleOne");
        var group = MemberGroupService.GetByName("RoleOne");

        var createModel = new MemberCreateModel
        {
            Key = key,
            Email = "test@test.com",
            Username = "test",
            Password = "SuperSecret123",
            IsApproved = true,
            ContentTypeKey = memberType.Key,
            Roles = new [] { group.Key },
            InvariantName = "T. Est",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The title value" },
                new PropertyValueModel { Alias = "author", Value = "The author value" }
            }
        };

        var result = await MemberEditingService.CreateAsync(createModel, SuperUser());
        Assert.IsTrue(result.Success);
    }

    private IUser SuperUser() => GetRequiredService<IUserService>().GetAsync(Constants.Security.SuperUserKey).GetAwaiter().GetResult();

}
