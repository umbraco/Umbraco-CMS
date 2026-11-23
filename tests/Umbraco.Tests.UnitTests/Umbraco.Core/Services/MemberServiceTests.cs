// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class MemberServiceTests
{
    [TestCase("INFO@EXAMPLE.COM", "info@example.com")]
    [TestCase(" Iris@Example.com ", "iris@example.com")]
    [SetCulture("tr-TR")] // tr-TR lower-cases "I" to a dotless "ı"
    public void CreateMember_Lowercases_The_Email_With_The_Invariant_Culture(string email, string expected)
    {
        var memberService = new MemberService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<ILoggerFactory>(),
            Mock.Of<IEventMessagesFactory>(),
            Mock.Of<IMemberGroupService>(),
            Mock.Of<IMemberRepository>(),
            Mock.Of<IMemberTypeRepository>(),
            Mock.Of<IMemberGroupRepository>(),
            Mock.Of<IAuditService>(),
            new Lazy<IIdKeyMap>(Mock.Of<IIdKeyMap>()),
            Mock.Of<IUserIdKeyResolver>());
        IMemberType memberType = new MemberTypeBuilder().Build();

        IMember member = memberService.CreateMember("user", email, "User", memberType);

        Assert.AreEqual(expected, member.Email);
    }
}
