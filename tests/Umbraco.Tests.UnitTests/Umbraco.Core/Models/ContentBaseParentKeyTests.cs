// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class ContentBaseParentKeyTests
{
    [Test]
    public void ParentKey_For_Root_Content_Returns_Null()
    {
        var contentType = new ContentTypeBuilder().Build();
        IContent content = new Content("content", Constants.System.Root, contentType);

        Guid? result = content.ParentKey;

        Assert.That(result, Is.Null);
    }

    [Test]
    public void ParentKey_For_Trashed_Content_Returns_RecycleBin_Sentinel_Key()
    {
        var contentType = new ContentTypeBuilder().Build();
        IContent content = new Content("content", Constants.System.RecycleBinContent, contentType);

        Guid? result = content.ParentKey;

        Assert.That(result, Is.EqualTo(Constants.System.RecycleBinContentKey));
    }

    [Test]
    public void ParentKey_For_Non_Root_Content_With_Unresolved_Real_Parent_Throws()
    {
        var contentType = new ContentTypeBuilder().Build();
        IContent content = new Content("content", 5, contentType);

        Assert.Throws<NotSupportedException>(
            () => _ = content.ParentKey,
            "a real, unpopulated parent id cannot be resolved from ParentId alone - the caller must " +
            "populate ParentKey via the repository read path or SetParent before reading it");
    }

    [Test]
    public void SetParent_Captures_Parent_Key_Eagerly()
    {
        var contentType = new ContentTypeBuilder().Build();
        var parent = new Content("parent", Constants.System.Root, contentType);
        var child = new Content("child", parent, contentType);

        Guid? result = child.ParentKey;

        Assert.That(result, Is.EqualTo(parent.Key));
    }

    [Test]
    public void Direct_ParentId_Assignment_Invalidates_Previously_Set_ParentKey()
    {
        var contentType = new ContentTypeBuilder().Build();
        var parent = new Content("parent", Constants.System.Root, contentType);
        var child = new Content("child", parent, contentType);

        Assert.That(child.ParentKey, Is.EqualTo(parent.Key)); // sanity check before the reassignment

        child.ParentId = 99;

        Assert.Throws<NotSupportedException>(
            () => _ = child.ParentKey,
            "reassigning ParentId to an unpopulated real parent must invalidate the old key, not leave it stale");
    }

    [Test]
    public void ParentKey_Public_Setter_Is_Read_Back()
    {
        var contentType = new ContentTypeBuilder().Build();
        var content = new Content("content", 5, contentType);
        var explicitKey = Guid.NewGuid();

        content.ParentKey = explicitKey;

        Assert.That(content.ParentKey, Is.EqualTo(explicitKey));
    }

    [Test]
    public void SetParent_With_Non_ContentBase_Parent_Leaves_ParentKey_Unresolved()
    {
        var contentType = new ContentTypeBuilder().Build();
        var content = new Content("content", Constants.System.Root, contentType);
        var nonContentParent = new EntityContainer(999, Guid.NewGuid(), -1, "-1,999", 1, 0, Constants.ObjectTypes.DataType, "Container", -1);

        content.SetParent(nonContentParent);

        Assert.That(content.ParentId, Is.EqualTo(999)); // sanity check: ParentId did resolve from the non-IContentBase parent
        Assert.Throws<NotSupportedException>(
            () => _ = content.ParentKey,
            "a parent that isn't an IContentBase carries no key to capture - it must not be silently treated as root");
    }

    [Test]
    public void DeepClone_Then_Reassign_ParentId_Invalidates_The_Clones_Copied_ParentKey()
    {
        var contentType = new ContentTypeBuilder().Build();
        var parent = new Content("parent", Constants.System.Root, contentType);
        var original = new Content("original", parent, contentType);
        Assert.That(original.ParentKey, Is.EqualTo(parent.Key)); // sanity check before cloning

        var clone = (Content)original.DeepClone();

        clone.ParentId = 123;

        Assert.Throws<NotSupportedException>(
            () => _ = clone.ParentKey,
            "the clone's ParentKey must not still carry the original parent's key after being retargeted");
    }
}
