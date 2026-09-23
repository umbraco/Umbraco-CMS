// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
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
    public void ParentKey_For_Non_Root_Content_With_Unresolved_Real_Parent_Is_Null_And_Not_Known()
    {
        var contentType = new ContentTypeBuilder().Build();
        IContent content = new Content("content", 5, contentType);

        Assert.Multiple(() =>
        {
            Assert.That(content.ParentKey, Is.Null);
            Assert.That(
                content.TryGetParentKey(out Guid? parentKey),
                Is.False,
                "a real, unpopulated parent id cannot be resolved from ParentId alone, so it must be " +
                "reported as unknown rather than confused with the root");
            Assert.That(parentKey, Is.Null);
        });
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

        Assert.Multiple(() =>
        {
            Assert.That(
                child.TryGetParentKey(out Guid? parentKey),
                Is.False,
                "reassigning ParentId to an unpopulated real parent must invalidate the old key, not leave it stale");
            Assert.That(parentKey, Is.Null);
        });
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
    public void SetParent_With_Non_ContentBase_Parent_Captures_Its_Key()
    {
        var contentType = new ContentTypeBuilder().Build();
        var content = new Content("content", Constants.System.Root, contentType);
        var parentKey = Guid.NewGuid();
        var nonContentParent = new EntityContainer(999, parentKey, -1, "-1,999", 1, 0, Constants.ObjectTypes.DataType, "Container", -1);

        content.SetParent(nonContentParent);

        Assert.Multiple(() =>
        {
            Assert.That(content.ParentId, Is.EqualTo(999));
            Assert.That(
                content.ParentKey,
                Is.EqualTo(parentKey),
                "every tree entity carries a key, so it is captured whether or not the parent is an IContentBase");
            Assert.That(content.TryGetParentKey(out Guid? captured), Is.True);
            Assert.That(captured, Is.EqualTo(parentKey));
        });
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

        Assert.Multiple(() =>
        {
            Assert.That(
                clone.TryGetParentKey(out Guid? parentKey),
                Is.False,
                "the clone's ParentKey must not still carry the original parent's key after being retargeted");
            Assert.That(parentKey, Is.Null);
        });
    }

    [Test]
    public void Serializing_An_Int_Constructed_Content_Does_Not_Throw()
    {
        var contentType = new ContentTypeBuilder().Build();
        IContent content = new Content("content", 5, contentType);

        Assert.DoesNotThrow(
            () => JsonSerializer.Serialize(content),
            "reading ParentKey must never throw - webhook payloads and similar consumers serialise entities " +
            "that were constructed from a raw parent id");
    }
}
