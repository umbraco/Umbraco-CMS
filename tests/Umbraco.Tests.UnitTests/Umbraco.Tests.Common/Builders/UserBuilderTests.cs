// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
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
        var testCreateDate = DateTime.Now.AddHours(-1);
        var testUpdateDate = DateTime.Now;
        const int testFailedPasswordAttempts = 22;
        var testLastLockoutDate = DateTime.Now.AddHours(-2);
        var testLastLoginDate = DateTime.Now.AddHours(-3);
        var testLastPasswordChangeDate = DateTime.Now.AddHours(-4);
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
        Assert.AreEqual(testId, user.Id);
        Assert.AreEqual(testKey, user.Key);
        Assert.AreEqual(testName, user.Name);
        Assert.AreEqual(testCreateDate, user.CreateDate);
        Assert.AreEqual(testUpdateDate, user.UpdateDate);
        Assert.AreEqual(testFailedPasswordAttempts, user.FailedPasswordAttempts);
        Assert.AreEqual(testIsApproved, user.IsApproved);
        Assert.AreEqual(testIsLockedOut, user.IsLockedOut);
        Assert.AreEqual(testLastLockoutDate, user.LastLockoutDate);
        Assert.AreEqual(testLastLoginDate, user.LastLoginDate);
        Assert.AreEqual(testLastPasswordChangeDate, user.LastPasswordChangeDate);
        Assert.AreEqual(testComments, user.Comments);
        Assert.AreEqual(testSessionTimeout, user.SessionTimeout);
        Assert.AreEqual(testStartContentIds, user.StartContentIds);
        Assert.AreEqual(testStartMediaIds, user.StartMediaIds);
    }
}
