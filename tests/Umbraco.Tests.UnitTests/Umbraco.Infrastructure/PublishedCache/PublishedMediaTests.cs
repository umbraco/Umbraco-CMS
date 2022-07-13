using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PublishedCache;

/// <summary>
///     Tests the typed extension methods on IPublishedContent using the DefaultPublishedMediaStore
/// </summary>
[TestFixture]
public class PublishedMediaTests : PublishedSnapshotServiceTestBase
{
    [SetUp]
    public override void Setup()
    {
        base.Setup();

        var dataTypes = GetDefaultDataTypes().ToList();
        var serializer = new ConfigurationEditorJsonSerializer();
        var rteDataType = new DataType(new VoidEditor("RTE", Mock.Of<IDataValueEditorFactory>()), serializer) { Id = 4 };
        dataTypes.Add(rteDataType);
        _dataTypes = dataTypes.ToArray();

        _propertyDataTypes = new Dictionary<string, IDataType>
        {
            // defaults will just use the first one
            [string.Empty] = _dataTypes[0],

            // content uses the RTE
            ["content"] = _dataTypes[1],
        };
    }

    private Dictionary<string, IDataType> _propertyDataTypes;
    private DataType[] _dataTypes;

    private ContentNodeKit CreateRoot(out MediaType mediaType)
    {
        mediaType = new MediaType(ShortStringHelper, -1);

        var item1Data = new ContentDataBuilder()
            .WithName("Content 1")
            .WithProperties(new PropertyDataBuilder()
                .WithPropertyData("content", "<div>This is some content</div>")
                .Build())

            // build with a dynamically created media type
            .Build(ShortStringHelper, _propertyDataTypes, mediaType, "image2");

        var item1 = ContentNodeKitBuilder.CreateWithContent(
            mediaType.Id,
            1,
            "-1,1",
            draftData: item1Data,
            publishedData: item1Data);

        return item1;
    }

    private IEnumerable<ContentNodeKit> CreateChildren(
        int startId,
        ContentNodeKit parent,
        IMediaType mediaType,
        int count)
    {
        for (var i = 0; i < count; i++)
        {
            var id = startId + i + 1;

            var item1Data = new ContentDataBuilder()
                .WithName("Child " + id)
                .WithProperties(new PropertyDataBuilder()
                    .WithPropertyData("content", "<div>This is some content</div>")
                    .Build())
                .Build();

            var parentPath = parent.Node.Path;

            var item1 = ContentNodeKitBuilder.CreateWithContent(
                mediaType.Id,
                id,
                $"{parentPath},{id}",
                draftData: item1Data,
                publishedData: item1Data);

            yield return item1;
        }
    }

    private void InitializeWithHierarchy(
        out int rootId,
        out IReadOnlyList<ContentNodeKit> firstLevelChildren,
        out IReadOnlyList<ContentNodeKit> secondLevelChildren)
    {
        var cache = new List<ContentNodeKit>();
        var root = CreateRoot(out var mediaType);
        firstLevelChildren = CreateChildren(10, root, mediaType, 3).ToList();
        secondLevelChildren = CreateChildren(20, firstLevelChildren[0], mediaType, 3).ToList();
        cache.Add(root);
        cache.AddRange(firstLevelChildren);
        cache.AddRange(secondLevelChildren);
        InitializedCache(null, null, _dataTypes, cache, new[] { mediaType });
        rootId = root.Node.Id;
    }

    [Test]
    public void Get_Property_Value_Uses_Converter()
    {
        var cache = CreateRoot(out var mediaType);
        InitializedCache(null, null, _dataTypes.ToArray(), new[] { cache }, new[] { mediaType });

        var publishedMedia = GetMedia(1);

        var propVal = publishedMedia.Value(PublishedValueFallback, "content");
        Assert.IsInstanceOf<IHtmlEncodedString>(propVal);
        Assert.AreEqual("<div>This is some content</div>", propVal.ToString());

        var propVal2 = publishedMedia.Value<IHtmlEncodedString>(PublishedValueFallback, "content");
        Assert.IsInstanceOf<IHtmlEncodedString>(propVal2);
        Assert.AreEqual("<div>This is some content</div>", propVal2.ToString());

        var propVal3 = publishedMedia.Value(PublishedValueFallback, "Content");
        Assert.IsInstanceOf<IHtmlEncodedString>(propVal3);
        Assert.AreEqual("<div>This is some content</div>", propVal3.ToString());
    }

    [Test]
    public void Children()
    {
        InitializeWithHierarchy(
            out var rootId,
            out var firstLevelChildren,
            out var secondLevelChildren);

        var publishedMedia = GetMedia(rootId);

        var rootChildren = publishedMedia.Children(VariationContextAccessor);
        Assert.IsTrue(rootChildren.Select(x => x.Id).ContainsAll(firstLevelChildren.Select(x => x.Node.Id)));

        var publishedChild1 = GetMedia(firstLevelChildren[0].Node.Id);
        var subChildren = publishedChild1.Children(VariationContextAccessor);
        Assert.IsTrue(subChildren.Select(x => x.Id).ContainsAll(secondLevelChildren.Select(x => x.Node.Id)));
    }

    [Test]
    public void Descendants()
    {
        InitializeWithHierarchy(
            out var rootId,
            out var firstLevelChildren,
            out var secondLevelChildren);

        var publishedMedia = GetMedia(rootId);
        var rootDescendants = publishedMedia.Descendants(VariationContextAccessor);

        var descendentIds =
            firstLevelChildren.Select(x => x.Node.Id).Concat(secondLevelChildren.Select(x => x.Node.Id));

        Assert.IsTrue(rootDescendants.Select(x => x.Id).ContainsAll(descendentIds));

        var publishedChild1 = GetMedia(firstLevelChildren[0].Node.Id);
        var subDescendants = publishedChild1.Descendants(VariationContextAccessor);
        Assert.IsTrue(subDescendants.Select(x => x.Id).ContainsAll(secondLevelChildren.Select(x => x.Node.Id)));
    }

    [Test]
    public void DescendantsOrSelf()
    {
        InitializeWithHierarchy(
            out var rootId,
            out var firstLevelChildren,
            out var secondLevelChildren);

        var publishedMedia = GetMedia(rootId);
        var rootDescendantsOrSelf = publishedMedia.DescendantsOrSelf(VariationContextAccessor);
        var descendentAndSelfIds = firstLevelChildren.Select(x => x.Node.Id)
            .Concat(secondLevelChildren.Select(x => x.Node.Id))
            .Append(rootId);

        Assert.IsTrue(rootDescendantsOrSelf.Select(x => x.Id).ContainsAll(descendentAndSelfIds));

        var publishedChild1 = GetMedia(firstLevelChildren[0].Node.Id);
        var subDescendantsOrSelf = publishedChild1.DescendantsOrSelf(VariationContextAccessor);
        Assert.IsTrue(subDescendantsOrSelf.Select(x => x.Id).ContainsAll(
            secondLevelChildren.Select(x => x.Node.Id).Append(firstLevelChildren[0].Node.Id)));
    }

    [Test]
    public void Parent()
    {
        InitializeWithHierarchy(
            out var rootId,
            out var firstLevelChildren,
            out var secondLevelChildren);

        var publishedMedia = GetMedia(rootId);
        Assert.AreEqual(null, publishedMedia.Parent);

        var publishedChild1 = GetMedia(firstLevelChildren[0].Node.Id);
        Assert.AreEqual(publishedMedia.Id, publishedChild1.Parent.Id);

        var publishedSubChild1 = GetMedia(secondLevelChildren[0].Node.Id);
        Assert.AreEqual(firstLevelChildren[0].Node.Id, publishedSubChild1.Parent.Id);
    }

    [Test]
    public void Ancestors()
    {
        InitializeWithHierarchy(
            out var rootId,
            out var firstLevelChildren,
            out var secondLevelChildren);

        var publishedSubChild1 = GetMedia(secondLevelChildren[0].Node.Id);
        Assert.IsTrue(publishedSubChild1.Ancestors().Select(x => x.Id)
            .ContainsAll(new[] { firstLevelChildren[0].Node.Id, rootId }));
    }

    [Test]
    public void AncestorsOrSelf()
    {
        InitializeWithHierarchy(
            out var rootId,
            out var firstLevelChildren,
            out var secondLevelChildren);

        var publishedSubChild1 = GetMedia(secondLevelChildren[0].Node.Id);
        Assert.IsTrue(publishedSubChild1.AncestorsOrSelf().Select(x => x.Id)
            .ContainsAll(new[] { secondLevelChildren[0].Node.Id, firstLevelChildren[0].Node.Id, rootId }));
    }
}
