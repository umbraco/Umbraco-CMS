using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Examine;

/// <summary>
/// Verifies that members are refreshed - and so re-indexed - when a property of their member type that already
/// existed is marked as sensitive, so values indexed before the change are removed from the members index.
/// </summary>
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
[TestFixture]
internal sealed class MemberTypeSensitivePropertyIndexingTests : UmbracoIntegrationTest
{
    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
        builder.Services.AddUnique(_ => new AppCaches(
            new DeepCloneAppCache(new ObjectCacheAppCache()),
            NoAppCache.Instance,
            new IsolatedCaches(_ => new DeepCloneAppCache(new ObjectCacheAppCache()))));
        builder.AddNotificationHandler<MemberCacheRefresherNotification, RefreshedMembersRecorder>();
    }

    [SetUp]
    public void ClearRecordedMembers() => RefreshedMembersRecorder.RefreshedMemberIds.Clear();

    [Test]
    public async Task Members_Are_Refreshed_When_An_Existing_Property_Is_Made_Sensitive()
    {
        (MemberType memberType, IMember member) = await CreateMemberTypeWithMemberAsync();

        memberType.SetIsSensitiveProperty("title", true);
        await MemberTypeService.UpdateAsync(memberType, Constants.Security.SuperUserKey);

        CollectionAssert.AreEquivalent(new[] { member.Id }, RefreshedMembersRecorder.RefreshedMemberIds);
    }

    [Test]
    public async Task Members_Are_Not_Refreshed_When_No_Property_Is_Made_Sensitive()
    {
        (MemberType memberType, _) = await CreateMemberTypeWithMemberAsync(sensitiveAlias: "title");

        memberType.SetIsSensitiveProperty("title", false);
        memberType.Name = "Renamed";
        await MemberTypeService.UpdateAsync(memberType, Constants.Security.SuperUserKey);

        CollectionAssert.IsEmpty(RefreshedMembersRecorder.RefreshedMemberIds);
    }

    [Test]
    public async Task Members_Are_Not_Refreshed_When_A_New_Property_Is_Sensitive()
    {
        (MemberType memberType, _) = await CreateMemberTypeWithMemberAsync();

        memberType.AddPropertyType(
            new PropertyTypeBuilder()
                .WithAlias("newProperty")
                .WithName("New property")
                .Build(),
            "content",
            "Content");
        memberType.SetIsSensitiveProperty("newProperty", true);
        await MemberTypeService.UpdateAsync(memberType, Constants.Security.SuperUserKey);

        CollectionAssert.IsEmpty(RefreshedMembersRecorder.RefreshedMemberIds);
    }

    private async Task<(MemberType MemberType, IMember Member)> CreateMemberTypeWithMemberAsync(string? sensitiveAlias = null)
    {
        MemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        if (sensitiveAlias is not null)
        {
            memberType.SetIsSensitiveProperty(sensitiveAlias, true);
        }

        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);

        IMember member = MemberService.CreateMember("member", "member@example.com", "Member", memberType);
        member.SetValue("title", "Title value");
        MemberService.Save(member);

        RefreshedMembersRecorder.RefreshedMemberIds.Clear();

        return (MemberTypeService.Get(memberType.Key)! as MemberType ?? throw new InvalidOperationException(), member);
    }

    private sealed class RefreshedMembersRecorder : INotificationHandler<MemberCacheRefresherNotification>
    {
        public static List<int> RefreshedMemberIds { get; } = [];

        public void Handle(MemberCacheRefresherNotification notification)
        {
            if (notification.MessageType == MessageType.RefreshByPayload)
            {
                RefreshedMemberIds.AddRange(((MemberCacheRefresher.JsonPayload[])notification.MessageObject).Select(x => x.Id));
            }
        }
    }
}
