// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

[TestFixture]
public class DistributedCacheExtensionsTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void Member_GetPayloads_CorrectlyCreatesPayloads(bool removed)
    {
        var member1Key = Guid.NewGuid();
        var member2Key = Guid.NewGuid();
        var member3Key = Guid.NewGuid();
        var members = new List<IMember>()
        {
            CreateMember(1, member1Key, "Fred", "fred", "fred@test.com"),
            CreateMember(1, member1Key, "Fred", "fred", "fred@test.com"),
            CreateMember(2, member2Key, "Sally", "sally", "sally@test.com"),
            CreateMember(3, member3Key, "Jane", "jane", "jane@test.com"),
        };

        var state = new Dictionary<string, object>
        {
            {
                MemberSavedNotification.PreviousUsernameStateKey,
                new Dictionary<Guid, string> { { member3Key, "janeold" } }
            },
        };

        var payloads = DistributedCacheExtensions.GetPayloads(members, state, removed);
        Assert.That(payloads.Count(), Is.EqualTo(3));

        var payloadForFred = payloads.First();
        Assert.That(payloadForFred.Username, Is.EqualTo("fred"));
        Assert.That(payloadForFred.Id, Is.EqualTo(1));
        Assert.That(payloadForFred.PreviousUsername, Is.Null);
        Assert.That(payloadForFred.Removed, Is.EqualTo(removed));

        var payloadForSally = payloads.Skip(1).First();
        Assert.That(payloadForSally.Username, Is.EqualTo("sally"));
        Assert.That(payloadForSally.Id, Is.EqualTo(2));
        Assert.That(payloadForSally.PreviousUsername, Is.Null);
        Assert.That(payloadForSally.Removed, Is.EqualTo(removed));

        var payloadForJane = payloads.Skip(2).First();
        Assert.That(payloadForJane.Username, Is.EqualTo("jane"));
        Assert.That(payloadForJane.Id, Is.EqualTo(3));
        Assert.That(payloadForJane.PreviousUsername, Is.EqualTo("janeold"));
        Assert.That(payloadForJane.Removed, Is.EqualTo(removed));
    }

    private static IMember CreateMember(int id, Guid key, string name, string username, string email)
        => new MemberBuilder()
            .AddMemberType()
                .Done()
            .WithId(id)
            .WithKey(key)
            .WithName(name)
            .WithLogin(username, "password")
            .WithEmail(email)
            .Build();
}
