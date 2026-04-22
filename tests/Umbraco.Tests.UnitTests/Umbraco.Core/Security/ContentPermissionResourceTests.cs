// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Security.Authorization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Security;

[TestFixture]
public class ContentPermissionResourceTests
{
    private static readonly Guid _contentKey = Guid.NewGuid();

    private const string PublishPermission = ActionPublish.ActionLetter;
    private const string BrowsePermission = ActionBrowse.ActionLetter;

    [Test]
    public void WithKeys_Single_Key_Sets_ContentKeys_And_Permission()
    {
        var resource = ContentPermissionResource.WithKeys(PublishPermission, _contentKey);

        Assert.Multiple(() =>
        {
            CollectionAssert.AreEquivalent(new[] { _contentKey }, resource.ContentKeys);
            CollectionAssert.AreEquivalent(new[] { PublishPermission }, resource.PermissionsToCheck);
            Assert.IsFalse(resource.CheckRoot);
            Assert.IsFalse(resource.CheckRecycleBin);
            Assert.IsNull(resource.ParentKeyForBranch);
            Assert.IsNull(resource.CulturesToCheck);
        });
    }

    [Test]
    public void WithKeys_Single_Key_With_Cultures_Sets_Cultures()
    {
        string[] cultures = ["en-US", "da-DK"];

        var resource = ContentPermissionResource.WithKeys(PublishPermission, _contentKey, cultures);

        Assert.Multiple(() =>
        {
            CollectionAssert.AreEquivalent(new[] { _contentKey }, resource.ContentKeys);
            CollectionAssert.AreEquivalent(cultures, resource.CulturesToCheck!);
        });
    }

    [Test]
    public void WithKeys_Multiple_Keys_Sets_ContentKeys()
    {
        var key2 = Guid.NewGuid();
        Guid[] keys = [_contentKey, key2];

        var resource = ContentPermissionResource.WithKeys(PublishPermission, keys);

        Assert.Multiple(() =>
        {
            CollectionAssert.AreEquivalent(keys, resource.ContentKeys);
            Assert.IsFalse(resource.CheckRoot);
            Assert.IsFalse(resource.CheckRecycleBin);
        });
    }

    [Test]
    public void WithKeys_Multiple_Permissions_Sets_All_Permissions()
    {
        ISet<string> permissions = new HashSet<string> { PublishPermission, BrowsePermission };

        var resource = ContentPermissionResource.WithKeys(permissions, new[] { _contentKey });

        CollectionAssert.AreEquivalent(permissions, resource.PermissionsToCheck);
    }

    [Test]
    public void WithKeys_Nullable_Key_With_Null_Creates_Root_Resource()
    {
        var resource = ContentPermissionResource.WithKeys(PublishPermission, (Guid?)null);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(resource.CheckRoot);
            Assert.IsFalse(resource.CheckRecycleBin);
            CollectionAssert.IsEmpty(resource.ContentKeys.ToList());
        });
    }

    [Test]
    public void WithKeys_Nullable_Key_With_Value_Creates_Key_Resource()
    {
        var resource = ContentPermissionResource.WithKeys(PublishPermission, (Guid?)_contentKey);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(resource.CheckRoot);
            CollectionAssert.AreEquivalent(new[] { _contentKey }, resource.ContentKeys);
        });
    }

    [Test]
    public void WithKeys_Nullable_Keys_With_Mix_Sets_Root_And_Keys()
    {
        Guid?[] keys = [null, _contentKey];

        var resource = ContentPermissionResource.WithKeys(PublishPermission, keys);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(resource.CheckRoot);
            CollectionAssert.AreEquivalent(new[] { _contentKey }, resource.ContentKeys);
        });
    }

    [Test]
    public void Root_Sets_CheckRoot_True()
    {
        var resource = ContentPermissionResource.Root(PublishPermission);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(resource.CheckRoot);
            Assert.IsFalse(resource.CheckRecycleBin);
            Assert.IsNull(resource.ParentKeyForBranch);
            CollectionAssert.IsEmpty(resource.ContentKeys.ToList());
            CollectionAssert.AreEquivalent(new[] { PublishPermission }, resource.PermissionsToCheck);
        });
    }

    [Test]
    public void Root_With_Cultures_Sets_Cultures()
    {
        string[] cultures = ["en-US"];

        var resource = ContentPermissionResource.Root(PublishPermission, cultures);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(resource.CheckRoot);
            CollectionAssert.AreEquivalent(cultures, resource.CulturesToCheck!);
        });
    }

    [Test]
    public void Root_With_Multiple_Permissions_Sets_All()
    {
        ISet<string> permissions = new HashSet<string> { PublishPermission, BrowsePermission };

        var resource = ContentPermissionResource.Root(permissions);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(resource.CheckRoot);
            CollectionAssert.AreEquivalent(permissions, resource.PermissionsToCheck);
        });
    }

    [Test]
    public void RecycleBin_Sets_CheckRecycleBin_True()
    {
        var resource = ContentPermissionResource.RecycleBin(PublishPermission);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(resource.CheckRoot);
            Assert.IsTrue(resource.CheckRecycleBin);
            Assert.IsNull(resource.ParentKeyForBranch);
            CollectionAssert.IsEmpty(resource.ContentKeys.ToList());
        });
    }

    [Test]
    public void RecycleBin_With_Multiple_Permissions_Sets_All()
    {
        ISet<string> permissions = new HashSet<string> { PublishPermission, BrowsePermission };

        var resource = ContentPermissionResource.RecycleBin(permissions);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(resource.CheckRecycleBin);
            CollectionAssert.AreEquivalent(permissions, resource.PermissionsToCheck);
        });
    }

    [Test]
    public void Branch_Sets_ParentKey_And_Does_Not_Check_RecycleBin()
    {
        var resource = ContentPermissionResource.Branch(PublishPermission, _contentKey);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(_contentKey, resource.ParentKeyForBranch);
            Assert.IsFalse(resource.CheckRoot);
            Assert.IsFalse(resource.CheckRecycleBin);
            CollectionAssert.IsEmpty(resource.ContentKeys.ToList());
            CollectionAssert.AreEquivalent(new[] { PublishPermission }, resource.PermissionsToCheck);
            Assert.IsNull(resource.CulturesToCheck);
        });
    }

    [Test]
    public void Branch_With_Multiple_Permissions_Sets_All()
    {
        ISet<string> permissions = new HashSet<string> { PublishPermission, BrowsePermission };

        var resource = ContentPermissionResource.Branch(permissions, _contentKey);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(_contentKey, resource.ParentKeyForBranch);
            Assert.IsFalse(resource.CheckRecycleBin);
            CollectionAssert.AreEquivalent(permissions, resource.PermissionsToCheck);
        });
    }

    [Test]
    public void Branch_With_Cultures_Sets_Cultures()
    {
        string[] cultures = ["en-US", "da-DK"];

        var resource = ContentPermissionResource.Branch(PublishPermission, _contentKey, cultures);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(_contentKey, resource.ParentKeyForBranch);
            Assert.IsFalse(resource.CheckRecycleBin);
            CollectionAssert.AreEquivalent(cultures, resource.CulturesToCheck!);
        });
    }

    [Test]
    public void Branch_With_Duplicate_Cultures_Deduplicates()
    {
        string[] cultures = ["en-US", "en-US", "da-DK"];

        var resource = ContentPermissionResource.Branch(PublishPermission, _contentKey, cultures);

        Assert.AreEqual(2, resource.CulturesToCheck!.Count);
        CollectionAssert.AreEquivalent(new[] { "en-US", "da-DK" }, resource.CulturesToCheck);
    }
}
