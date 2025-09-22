// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

[TestFixture]
public class DistributedCacheExtensionsTests
{
    [Test]
    public void Member_GetPayloads_CorrectlyCreatesPayloads()
    {
        var members = new List<IMember>()
        {
            CreateMember(1, "Fred", "fred", "fred@test.com"),
            CreateMember(1, "Fred", "fred", "fred@test.com"),
            CreateMember(2, "Sally", "sally", "sally@test.com"),
            CreateMember(3, "Jane", "jane", "jane@test.com", "janeold"),
        };

        var payloads = DistributedCacheExtensions.GetPayloads(members, false);
        Assert.AreEqual(3, payloads.Count());

        var payloadForFred = payloads.First();
        Assert.AreEqual("fred", payloadForFred.Username);
        Assert.AreEqual(1, payloadForFred.Id);
        Assert.IsNull(payloadForFred.PreviousUsername);

        var payloadForSally = payloads.Skip(1).First();
        Assert.AreEqual("sally", payloadForSally.Username);
        Assert.AreEqual(2, payloadForSally.Id);
        Assert.IsNull(payloadForSally.PreviousUsername);

        var payloadForJane = payloads.Skip(2).First();
        Assert.AreEqual("jane", payloadForJane.Username);
        Assert.AreEqual(3, payloadForJane.Id);
        Assert.AreEqual("janeold", payloadForJane.PreviousUsername);
    }

    private static IMember CreateMember(int id, string name, string username, string email, string? previousUserName = null)
    {
        var memberBuilder = new MemberBuilder()
            .AddMemberType()
                .Done()
            .WithId(id)
            .WithName(name)
            .WithLogin(username, "password")
            .WithEmail(email);

        if (previousUserName != null)
        {
            memberBuilder.AddAdditionalData()
                .WithKeyValue(global::Umbraco.Cms.Core.Constants.Entities.AdditionalDataKeys.MemberPreviousUserName, previousUserName)
                .Done();
        }

        return memberBuilder.Build();
    }
}
