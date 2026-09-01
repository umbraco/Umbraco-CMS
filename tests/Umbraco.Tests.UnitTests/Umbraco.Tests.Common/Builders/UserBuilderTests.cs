// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Tests.Common.Builders;

[TestFixture]
public class UserBuilderTests
{
    [Test]
    public void Is_Built_Correctly()
    {
        // Arrange
        const int testId = 10;
        const string testName = "Fred";
        const string testUsername = "fred";
        const string testRawPasswordValue = "raw pass";
        const string testEmail = "email@email.com";
        const bool testIsApproved = true;
        const bool testIsLockedOut = true;
        var testKey = Guid.NewGuid();
        var testCreateDate = DateTime.UtcNow.AddHours(-1);
        var testUpdateDate = DateTime.UtcNow;
        const int testFailedPasswordAttempts = 22;
        var testLastLockoutDate = DateTime.UtcNow.AddHours(-2);
        var testLastLoginDate = DateTime.UtcNow.AddHours(-3);
        var testLastPasswordChangeDate = DateTime.UtcNow.AddHours(-4);
        var testComments = "comments";
        var testSessionTimeout = 5;
        var testStartContentIds = new[] { 3 };
        var testStartMediaIds = new[] { 8 };

        var builder = new UserBuilder();

        // Act
        var user = builder
            .WithId(testId)
            .WithKey(testKey)
            .WithName(testName)
            .WithLogin(testUsername, testRawPasswordValue)
            .WithEmail(testEmail)
            .WithCreateDate(testCreateDate)
            .WithUpdateDate(testUpdateDate)
            .WithFailedPasswordAttempts(testFailedPasswordAttempts)
            .WithIsApproved(testIsApproved)
            .WithIsLockedOut(testIsLockedOut, testLastLockoutDate)
            .WithLastLoginDate(testLastLoginDate)
            .WithLastPasswordChangeDate(testLastPasswordChangeDate)
            .WithComments(testComments)
            .WithSessionTimeout(5)
            .WithStartContentIds(new[] { 3 })
            .WithStartMediaIds(new[] { 8 })
            .Build();

        // Assert
        Assert.That(user.Id, Is.EqualTo(testId));
        Assert.That(user.Key, Is.EqualTo(testKey));
        Assert.That(user.Name, Is.EqualTo(testName));
        Assert.That(user.CreateDate, Is.EqualTo(testCreateDate));
        Assert.That(user.UpdateDate, Is.EqualTo(testUpdateDate));
        Assert.That(user.FailedPasswordAttempts, Is.EqualTo(testFailedPasswordAttempts));
        Assert.That(user.IsApproved, Is.EqualTo(testIsApproved));
        Assert.That(user.IsLockedOut, Is.EqualTo(testIsLockedOut));
        Assert.That(user.LastLockoutDate, Is.EqualTo(testLastLockoutDate));
        Assert.That(user.LastLoginDate, Is.EqualTo(testLastLoginDate));
        Assert.That(user.LastPasswordChangeDate, Is.EqualTo(testLastPasswordChangeDate));
        Assert.That(user.Comments, Is.EqualTo(testComments));
        Assert.That(user.SessionTimeout, Is.EqualTo(testSessionTimeout));
        Assert.That(user.StartContentIds, Is.EqualTo(testStartContentIds));
        Assert.That(user.StartMediaIds, Is.EqualTo(testStartMediaIds));
    }
}
