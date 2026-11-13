// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Models.Membership.Permissions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models.Membership.Permissions;

[TestFixture]
public class ElementContainerGranularPermissionTests
{
    [Test]
    public void Equals_Returns_True_For_Same_Key_And_Permission()
    {
        var key = Guid.NewGuid();
        var a = new ElementContainerGranularPermission { Key = key, Permission = "Umb.ElementContainer.Create" };
        var b = new ElementContainerGranularPermission { Key = key, Permission = "Umb.ElementContainer.Create" };

        Assert.That(a, Is.EqualTo(b));
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void Equals_Returns_False_For_Different_Permission()
    {
        var key = Guid.NewGuid();
        var a = new ElementContainerGranularPermission { Key = key, Permission = "Umb.ElementContainer.Create" };
        var b = new ElementContainerGranularPermission { Key = key, Permission = "Umb.Element.Create" };

        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public void Equals_Returns_False_For_Different_Key()
    {
        var a = new ElementContainerGranularPermission { Key = Guid.NewGuid(), Permission = "Umb.ElementContainer.Create" };
        var b = new ElementContainerGranularPermission { Key = Guid.NewGuid(), Permission = "Umb.ElementContainer.Create" };

        Assert.That(a, Is.Not.EqualTo(b));
    }
}
