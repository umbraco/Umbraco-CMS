// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class UserExtensionsSensitiveDataTests
{
    [Test]
    public void HasAccessToSensitiveData_SensitiveDataGroup_ReturnsTrue()
    {
        ISet<string> permissions = new HashSet<string>();
        var group = Mock.Of<IReadOnlyUserGroup>(g =>
            g.Key == Constants.Security.SensitiveDataGroupKey &&
            g.Permissions == permissions);
        var user = Mock.Of<IUser>(u => u.Groups == new[] { group });

        Assert.That(user.HasAccessToSensitiveData(), Is.True);
    }

    [Test]
    public void HasAccessToSensitiveData_GroupWithSensitiveDataPermission_ReturnsTrue()
    {
        ISet<string> browse = new HashSet<string> { ActionBrowse.ActionLetter };
        ISet<string> browseAndSensitiveData = new HashSet<string>
        {
            ActionBrowse.ActionLetter,
            ActionMemberSensitiveData.ActionLetter,
        };
        var browseGroup = Mock.Of<IReadOnlyUserGroup>(g => g.Permissions == browse);
        var sensitiveDataGroup = Mock.Of<IReadOnlyUserGroup>(g => g.Permissions == browseAndSensitiveData);
        var user = Mock.Of<IUser>(u => u.Groups == new[] { browseGroup, sensitiveDataGroup });

        Assert.That(user.HasAccessToSensitiveData(), Is.True);
    }

    [Test]
    public void HasAccessToSensitiveData_NoGroupGrantsAccess_ReturnsFalse()
    {
        ISet<string> browse = new HashSet<string> { ActionBrowse.ActionLetter };
        var group = Mock.Of<IReadOnlyUserGroup>(g =>
            g.Key == Constants.Security.AdminGroupKey &&
            g.Permissions == browse);
        var user = Mock.Of<IUser>(u => u.Groups == new[] { group });

        Assert.That(user.HasAccessToSensitiveData(), Is.False);
    }
}
