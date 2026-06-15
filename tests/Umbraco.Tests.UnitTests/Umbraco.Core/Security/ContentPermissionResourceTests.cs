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
            Assert.That(resource.ContentKeys, Is.EquivalentTo(new[] { _contentKey }));
            Assert.That(resource.PermissionsToCheck, Is.EquivalentTo(new[] { PublishPermission }));
            Assert.That(resource.CheckRoot, Is.False);
            Assert.That(resource.CheckRecycleBin, Is.False);
            Assert.That(resource.ParentKeyForBranch, Is.Null);
            Assert.That(resource.CulturesToCheck, Is.Null);
        });
    }

    [Test]
    public void WithKeys_Single_Key_With_Cultures_Sets_Cultures()
    {
        string[] cultures = ["en-US", "da-DK"];

        var resource = ContentPermissionResource.WithKeys(PublishPermission, _contentKey, cultures);

        Assert.Multiple(() =>
        {
            Assert.That(resource.ContentKeys, Is.EquivalentTo(new[] { _contentKey }));
            Assert.That(resource.CulturesToCheck!, Is.EquivalentTo(cultures));
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
            Assert.That(resource.ContentKeys, Is.EquivalentTo(keys));
            Assert.That(resource.CheckRoot, Is.False);
            Assert.That(resource.CheckRecycleBin, Is.False);
        });
    }

    [Test]
    public void WithKeys_Multiple_Permissions_Sets_All_Permissions()
    {
        ISet<string> permissions = new HashSet<string> { PublishPermission, BrowsePermission };

        var resource = ContentPermissionResource.WithKeys(permissions, new[] { _contentKey });

        Assert.That(resource.PermissionsToCheck, Is.EquivalentTo(permissions));
    }

    [Test]
    public void WithKeys_Nullable_Key_With_Null_Creates_Root_Resource()
    {
        var resource = ContentPermissionResource.WithKeys(PublishPermission, (Guid?)null);

        Assert.Multiple(() =>
        {
            Assert.That(resource.CheckRoot, Is.True);
            Assert.That(resource.CheckRecycleBin, Is.False);
            Assert.That(resource.ContentKeys.ToList(), Is.Empty);
        });
    }

    [Test]
    public void WithKeys_Nullable_Key_With_Value_Creates_Key_Resource()
    {
        var resource = ContentPermissionResource.WithKeys(PublishPermission, (Guid?)_contentKey);

        Assert.Multiple(() =>
        {
            Assert.That(resource.CheckRoot, Is.False);
            Assert.That(resource.ContentKeys, Is.EquivalentTo(new[] { _contentKey }));
        });
    }

    [Test]
    public void WithKeys_Nullable_Keys_With_Mix_Sets_Root_And_Keys()
    {
        Guid?[] keys = [null, _contentKey];

        var resource = ContentPermissionResource.WithKeys(PublishPermission, keys);

        Assert.Multiple(() =>
        {
            Assert.That(resource.CheckRoot, Is.True);
            Assert.That(resource.ContentKeys, Is.EquivalentTo(new[] { _contentKey }));
        });
    }

    [Test]
    public void Root_Sets_CheckRoot_True()
    {
        var resource = ContentPermissionResource.Root(PublishPermission);

        Assert.Multiple(() =>
        {
            Assert.That(resource.CheckRoot, Is.True);
            Assert.That(resource.CheckRecycleBin, Is.False);
            Assert.That(resource.ParentKeyForBranch, Is.Null);
            Assert.That(resource.ContentKeys.ToList(), Is.Empty);
            Assert.That(resource.PermissionsToCheck, Is.EquivalentTo(new[] { PublishPermission }));
        });
    }

    [Test]
    public void Root_With_Cultures_Sets_Cultures()
    {
        string[] cultures = ["en-US"];

        var resource = ContentPermissionResource.Root(PublishPermission, cultures);

        Assert.Multiple(() =>
        {
            Assert.That(resource.CheckRoot, Is.True);
            Assert.That(resource.CulturesToCheck!, Is.EquivalentTo(cultures));
        });
    }

    [Test]
    public void Root_With_Multiple_Permissions_Sets_All()
    {
        ISet<string> permissions = new HashSet<string> { PublishPermission, BrowsePermission };

        var resource = ContentPermissionResource.Root(permissions);

        Assert.Multiple(() =>
        {
            Assert.That(resource.CheckRoot, Is.True);
            Assert.That(resource.PermissionsToCheck, Is.EquivalentTo(permissions));
        });
    }

    [Test]
    public void RecycleBin_Sets_CheckRecycleBin_True()
    {
        var resource = ContentPermissionResource.RecycleBin(PublishPermission);

        Assert.Multiple(() =>
        {
            Assert.That(resource.CheckRoot, Is.False);
            Assert.That(resource.CheckRecycleBin, Is.True);
            Assert.That(resource.ParentKeyForBranch, Is.Null);
            Assert.That(resource.ContentKeys.ToList(), Is.Empty);
        });
    }

    [Test]
    public void RecycleBin_With_Multiple_Permissions_Sets_All()
    {
        ISet<string> permissions = new HashSet<string> { PublishPermission, BrowsePermission };

        var resource = ContentPermissionResource.RecycleBin(permissions);

        Assert.Multiple(() =>
        {
            Assert.That(resource.CheckRecycleBin, Is.True);
            Assert.That(resource.PermissionsToCheck, Is.EquivalentTo(permissions));
        });
    }

    [Test]
    public void Branch_Sets_ParentKey_And_Does_Not_Check_RecycleBin()
    {
        var resource = ContentPermissionResource.Branch(PublishPermission, _contentKey);

        Assert.Multiple(() =>
        {
            Assert.That(resource.ParentKeyForBranch, Is.EqualTo(_contentKey));
            Assert.That(resource.CheckRoot, Is.False);
            Assert.That(resource.CheckRecycleBin, Is.False);
            Assert.That(resource.ContentKeys.ToList(), Is.Empty);
            Assert.That(resource.PermissionsToCheck, Is.EquivalentTo(new[] { PublishPermission }));
            Assert.That(resource.CulturesToCheck, Is.Null);
        });
    }

    [Test]
    public void Branch_With_Multiple_Permissions_Sets_All()
    {
        ISet<string> permissions = new HashSet<string> { PublishPermission, BrowsePermission };

        var resource = ContentPermissionResource.Branch(permissions, _contentKey);

        Assert.Multiple(() =>
        {
            Assert.That(resource.ParentKeyForBranch, Is.EqualTo(_contentKey));
            Assert.That(resource.CheckRecycleBin, Is.False);
            Assert.That(resource.PermissionsToCheck, Is.EquivalentTo(permissions));
        });
    }

    [Test]
    public void Branch_With_Cultures_Sets_Cultures()
    {
        string[] cultures = ["en-US", "da-DK"];

        var resource = ContentPermissionResource.Branch(PublishPermission, _contentKey, cultures);

        Assert.Multiple(() =>
        {
            Assert.That(resource.ParentKeyForBranch, Is.EqualTo(_contentKey));
            Assert.That(resource.CheckRecycleBin, Is.False);
            Assert.That(resource.CulturesToCheck!, Is.EquivalentTo(cultures));
        });
    }

    [Test]
    public void Branch_With_Duplicate_Cultures_Deduplicates()
    {
        string[] cultures = ["en-US", "en-US", "da-DK"];

        var resource = ContentPermissionResource.Branch(PublishPermission, _contentKey, cultures);

        Assert.That(resource.CulturesToCheck!, Has.Count.EqualTo(2));
        Assert.That(resource.CulturesToCheck, Is.EquivalentTo(new[] { "en-US", "da-DK" }));
    }
}
