// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Verifies that the state of a member deleting notification reaches every deleted notification the operation publishes.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MemberServiceNotificationStateTests : UmbracoIntegrationTest
{
    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private IExternalMemberService ExternalMemberService => GetRequiredService<IExternalMemberService>();

    private NotificationStateRecorder Recorder => GetRequiredService<NotificationStateRecorder>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<NotificationStateRecorder>();
        builder
            .AddNotificationHandler<MemberDeletingNotification, NotificationStateWritingHandler<MemberDeletingNotification>>()
            .AddNotificationHandler<MemberDeletedNotification, NotificationStateReadingHandler<MemberDeletedNotification>>()
            .AddNotificationHandler<ExternalMemberDeletingNotification, NotificationStateWritingHandler<ExternalMemberDeletingNotification>>()
            .AddNotificationHandler<ExternalMemberDeletedNotification, NotificationStateReadingHandler<ExternalMemberDeletedNotification>>();
    }

    [Test]
    public async Task DeleteMembersOfType_Shares_Deleting_State_With_Deleted_Notifications()
    {
        IMemberType memberType = MemberTypeBuilder.CreateSimpleMemberType();
        await MemberTypeService.CreateAsync(memberType, Constants.Security.SuperUserKey);
        IMember first = MemberBuilder.CreateSimpleMember(memberType, "First", "first@test.com", "password", "first");
        IMember second = MemberBuilder.CreateSimpleMember(memberType, "Second", "second@test.com", "password", "second");
        MemberService.Save(first);
        MemberService.Save(second);

        MemberService.DeleteMembersOfType(memberType.Id);

        MemberDeletingNotification deleting = Recorder.Starting.OfType<MemberDeletingNotification>().Single();
        MemberDeletedNotification[] deleted = Recorder.Ending.OfType<MemberDeletedNotification>().ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(deleted.SelectMany(x => x.DeletedEntities).Select(x => x.Key), Is.EquivalentTo(new[] { first.Key, second.Key }));
            Assert.That(deleted.Select(x => x.State), Has.All.SameAs(deleting.State));
            Assert.That(deleting.State.ContainsKey(NotificationStateRecorder.StateKey), Is.True);
        });
    }

    [Test]
    public async Task External_Member_Delete_Shares_Deleting_State_With_Deleted_Notification()
    {
        var createResult = await ExternalMemberService.CreateAsync(ExternalMemberIdentityBuilder.CreateSimple("state-test@example.com", "State Test"));
        Assert.That(createResult.Success, Is.True);

        var deleteResult = await ExternalMemberService.DeleteAsync(createResult.Result!.Key);
        Assert.That(deleteResult.Success, Is.True);

        ExternalMemberDeletingNotification deleting = Recorder.Starting.OfType<ExternalMemberDeletingNotification>().Single();
        ExternalMemberDeletedNotification deleted = Recorder.Ending.OfType<ExternalMemberDeletedNotification>().Single();

        Assert.Multiple(() =>
        {
            Assert.That(deleted.State, Is.SameAs(deleting.State));
            Assert.That(deleting.State.ContainsKey(NotificationStateRecorder.StateKey), Is.True);
        });
    }
}
