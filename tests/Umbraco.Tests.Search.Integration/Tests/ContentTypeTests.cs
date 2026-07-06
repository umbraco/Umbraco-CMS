using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Tests.Search.Integration.Services;

namespace Umbraco.Tests.Search.Integration.Tests;

public class ContentTypeTests : ContentBaseTestBase
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentType _contentType1 = null!;

    private IContentType _contentType2 = null!;

    private IContentType _contentType3 = null!;

    private readonly Guid _contentType1RootContentKey = Guid.NewGuid();

    private readonly Guid _contentType1ChildContentKey = Guid.NewGuid();

    private readonly Guid _contentType2RootContentKey = Guid.NewGuid();

    private readonly Guid _contentType2ChildContentKey = Guid.NewGuid();

    private readonly Guid _contentType3RootContentKey = Guid.NewGuid();

    private readonly Guid _contentType3ChildContentKey = Guid.NewGuid();

    [SetUp]
    public async Task SetupTest()
    {
        IndexerAndSearcher.Reset();

        _contentType1 = await CreateContentType();
        _contentType2 = await CreateContentType();
        _contentType3 = await CreateContentType();

        CreateContentStructure(_contentType1, _contentType1RootContentKey, _contentType1ChildContentKey);
        CreateContentStructure(_contentType2, _contentType2RootContentKey, _contentType2ChildContentKey);
        CreateContentStructure(_contentType3, _contentType3RootContentKey, _contentType3ChildContentKey);

        return;

        async Task<IContentType> CreateContentType()
        {
            IContentType contentType = new ContentTypeBuilder().Build();
            await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
            contentType.AllowedAsRoot = true;
            contentType.AllowedContentTypes = [new ContentTypeSort(contentType.Key, 0, contentType.Alias)];
            await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

            return contentType;
        }

        void CreateContentStructure(IContentType contentType, Guid rootContentKey, Guid childContentKey)
        {
            Content root = new ContentBuilder()
                .WithKey(rootContentKey)
                .WithContentType(contentType)
                .Build();
            ContentService.Save(root);

            Content child = new ContentBuilder()
                .WithKey(childContentKey)
                .WithContentType(contentType)
                .WithParent(root)
                .Build();
            ContentService.Save(child);

            ContentService.PublishBranch(root, PublishBranchFilter.IncludeUnpublished, ["*"]);
        }
    }

    [Test]
    public async Task DeleteContentType1()
    {
        IReadOnlyList<TestIndexDocument> draftDocuments = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(draftDocuments, Has.Count.EqualTo(6));

        IReadOnlyList<TestIndexDocument> publishedDocuments = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(publishedDocuments, Has.Count.EqualTo(6));

        await ContentTypeService.DeleteAsync(_contentType1.Key, Constants.Security.SuperUserKey);

        draftDocuments = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(draftDocuments, Has.Count.EqualTo(4));

        publishedDocuments = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(publishedDocuments, Has.Count.EqualTo(4));

        Assert.Multiple(() =>
        {
            Assert.That(draftDocuments[0].Id, Is.EqualTo(_contentType2RootContentKey));
            Assert.That(draftDocuments[1].Id, Is.EqualTo(_contentType2ChildContentKey));
            Assert.That(draftDocuments[2].Id, Is.EqualTo(_contentType3RootContentKey));
            Assert.That(draftDocuments[3].Id, Is.EqualTo(_contentType3ChildContentKey));

            Assert.That(publishedDocuments[0].Id, Is.EqualTo(_contentType2RootContentKey));
            Assert.That(publishedDocuments[1].Id, Is.EqualTo(_contentType2ChildContentKey));
            Assert.That(publishedDocuments[2].Id, Is.EqualTo(_contentType3RootContentKey));
            Assert.That(publishedDocuments[3].Id, Is.EqualTo(_contentType3ChildContentKey));
        });
    }

    [Test]
    public async Task DeleteContentType2And3()
    {
        IReadOnlyList<TestIndexDocument> draftDocuments = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(draftDocuments, Has.Count.EqualTo(6));

        IReadOnlyList<TestIndexDocument> publishedDocuments = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(publishedDocuments, Has.Count.EqualTo(6));

        await ContentTypeService.DeleteAsync(_contentType2.Key, Constants.Security.SuperUserKey);
        await ContentTypeService.DeleteAsync(_contentType3.Key, Constants.Security.SuperUserKey);

        draftDocuments = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(draftDocuments, Has.Count.EqualTo(2));

        publishedDocuments = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(publishedDocuments, Has.Count.EqualTo(2));

        Assert.Multiple(() =>
        {
            Assert.That(draftDocuments[0].Id, Is.EqualTo(_contentType1RootContentKey));
            Assert.That(draftDocuments[1].Id, Is.EqualTo(_contentType1ChildContentKey));

            Assert.That(publishedDocuments[0].Id, Is.EqualTo(_contentType1RootContentKey));
            Assert.That(publishedDocuments[1].Id, Is.EqualTo(_contentType1ChildContentKey));
        });
    }

    [Test]
    public async Task UpdateComposedContentType_ReindexesComposingTypeContent()
    {
        // Create a content type that will be used as a composition
        IContentType compositionType = new ContentTypeBuilder()
            .WithAlias("composition")
            .AddPropertyType()
                .WithAlias("originalProp")
                .WithDataTypeId(Constants.DataTypes.Textbox)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                .Done()
            .Build();
        await ContentTypeService.CreateAsync(compositionType, Constants.Security.SuperUserKey);
        compositionType.AllowedAsRoot = true;
        await ContentTypeService.UpdateAsync(compositionType, Constants.Security.SuperUserKey);

        // Create a content type that inherits from (composes) the first
        IContentType composingType = new ContentTypeBuilder()
            .WithAlias("composing")
            .Build();
        composingType.AddContentType(compositionType);
        await ContentTypeService.CreateAsync(composingType, Constants.Security.SuperUserKey);
        composingType.AllowedAsRoot = true;
        await ContentTypeService.UpdateAsync(composingType, Constants.Security.SuperUserKey);

        // Create content of the composition type
        var compositionContentKey = Guid.NewGuid();
        Content compositionContent = new ContentBuilder()
            .WithKey(compositionContentKey)
            .WithContentType(compositionType)
            .Build();
        ContentService.Save(compositionContent);

        // Create content of the composing type
        var composingContentKey = Guid.NewGuid();
        Content composingContent = new ContentBuilder()
            .WithKey(composingContentKey)
            .WithContentType(composingType)
            .Build();
        ContentService.Save(composingContent);

        // Verify initial state: both new content items indexed in draft
        // (plus 6 from the base SetUp = 8 total)
        IReadOnlyList<TestIndexDocument> draftDocuments = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(draftDocuments, Has.Count.EqualTo(8));

        // Clear the index to track what gets re-indexed by the content type change
        IndexerAndSearcher.Reset();
        draftDocuments = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(draftDocuments, Is.Empty);

        // Act: make a structural change to the composition type (change the alias)
        compositionType.Alias += "_updated";
        await ContentTypeService.UpdateAsync(compositionType, Constants.Security.SuperUserKey);

        // Assert: BOTH content items should have been re-indexed,
        // not just the composition type's content
        draftDocuments = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(draftDocuments, Has.Count.EqualTo(2));

        Assert.Multiple(() =>
        {
            Assert.That(draftDocuments.Select(d => d.Id), Contains.Item(compositionContentKey));
            Assert.That(draftDocuments.Select(d => d.Id), Contains.Item(composingContentKey));
        });
    }

    [Test]
    public async Task UpdateDeeplyNestedComposition_ReindexesAllDependentContent()
    {
        // Create a 3-level composition chain: grandchild composes child, child composes root
        IContentType rootType = new ContentTypeBuilder()
            .WithAlias("root_comp")
            .AddPropertyType()
                .WithAlias("rootProp")
                .WithDataTypeId(Constants.DataTypes.Textbox)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                .Done()
            .Build();
        await ContentTypeService.CreateAsync(rootType, Constants.Security.SuperUserKey);
        rootType.AllowedAsRoot = true;
        await ContentTypeService.UpdateAsync(rootType, Constants.Security.SuperUserKey);

        IContentType childType = new ContentTypeBuilder()
            .WithAlias("child_comp")
            .Build();
        childType.AddContentType(rootType);
        await ContentTypeService.CreateAsync(childType, Constants.Security.SuperUserKey);
        childType.AllowedAsRoot = true;
        await ContentTypeService.UpdateAsync(childType, Constants.Security.SuperUserKey);

        IContentType grandchildType = new ContentTypeBuilder()
            .WithAlias("grandchild_comp")
            .Build();
        grandchildType.AddContentType(childType);
        await ContentTypeService.CreateAsync(grandchildType, Constants.Security.SuperUserKey);
        grandchildType.AllowedAsRoot = true;
        await ContentTypeService.UpdateAsync(grandchildType, Constants.Security.SuperUserKey);

        // Create content of each type
        var rootContentKey = Guid.NewGuid();
        Content rootContent = new ContentBuilder()
            .WithKey(rootContentKey)
            .WithContentType(rootType)
            .Build();
        ContentService.Save(rootContent);

        var childContentKey = Guid.NewGuid();
        Content childContent = new ContentBuilder()
            .WithKey(childContentKey)
            .WithContentType(childType)
            .Build();
        ContentService.Save(childContent);

        var grandchildContentKey = Guid.NewGuid();
        Content grandchildContent = new ContentBuilder()
            .WithKey(grandchildContentKey)
            .WithContentType(grandchildType)
            .Build();
        ContentService.Save(grandchildContent);

        // Verify initial state (6 from SetUp + 3 new = 9)
        IReadOnlyList<TestIndexDocument> draftDocuments = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(draftDocuments, Has.Count.EqualTo(9));

        // Clear the index to track what gets re-indexed
        IndexerAndSearcher.Reset();

        // Act: make a structural change to the root composition type (change the alias)
        rootType.Alias += "_updated";
        await ContentTypeService.UpdateAsync(rootType, Constants.Security.SuperUserKey);

        // Assert: ALL 3 content items should be re-indexed
        draftDocuments = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(draftDocuments, Has.Count.EqualTo(3));

        Assert.Multiple(() =>
        {
            Assert.That(draftDocuments.Select(d => d.Id), Contains.Item(rootContentKey));
            Assert.That(draftDocuments.Select(d => d.Id), Contains.Item(childContentKey));
            Assert.That(draftDocuments.Select(d => d.Id), Contains.Item(grandchildContentKey));
        });
    }

    [Test]
    public async Task DeleteComposedContentType_RemovesBothTypesContent()
    {
        // Create a content type that will be used as a composition
        IContentType compositionType = new ContentTypeBuilder()
            .WithAlias("composition")
            .AddPropertyType()
                .WithAlias("originalProp")
                .WithDataTypeId(Constants.DataTypes.Textbox)
                .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                .Done()
            .Build();
        await ContentTypeService.CreateAsync(compositionType, Constants.Security.SuperUserKey);
        compositionType.AllowedAsRoot = true;
        await ContentTypeService.UpdateAsync(compositionType, Constants.Security.SuperUserKey);

        // Create a content type that inherits from (composes) the first
        IContentType composingType = new ContentTypeBuilder()
            .WithAlias("composing")
            .Build();
        composingType.AddContentType(compositionType);
        await ContentTypeService.CreateAsync(composingType, Constants.Security.SuperUserKey);
        composingType.AllowedAsRoot = true;
        await ContentTypeService.UpdateAsync(composingType, Constants.Security.SuperUserKey);

        // Create content of the composition type
        var compositionContentKey = Guid.NewGuid();
        Content compositionContent = new ContentBuilder()
            .WithKey(compositionContentKey)
            .WithContentType(compositionType)
            .Build();
        ContentService.Save(compositionContent);
        ContentService.PublishBranch(compositionContent, PublishBranchFilter.IncludeUnpublished, ["*"]);

        // Create content of the composing type
        var composingContentKey = Guid.NewGuid();
        Content composingContent = new ContentBuilder()
            .WithKey(composingContentKey)
            .WithContentType(composingType)
            .Build();
        ContentService.Save(composingContent);
        ContentService.PublishBranch(composingContent, PublishBranchFilter.IncludeUnpublished, ["*"]);

        // Verify initial state (6 from SetUp + 2 new = 8)
        IReadOnlyList<TestIndexDocument> draftDocuments = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(draftDocuments, Has.Count.EqualTo(8));

        IReadOnlyList<TestIndexDocument> publishedDocuments = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(publishedDocuments, Has.Count.EqualTo(8));

        // Act: remove the composition from the composing type, then delete the composition type
        composingType.RemoveContentType(compositionType.Alias);
        await ContentTypeService.UpdateAsync(composingType, Constants.Security.SuperUserKey);
        await ContentTypeService.DeleteAsync(compositionType.Key, Constants.Security.SuperUserKey);

        // Assert: composition type's content is removed, composing type's content is re-indexed
        draftDocuments = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(draftDocuments, Has.Count.EqualTo(7));
        Assert.That(draftDocuments.Select(d => d.Id), Does.Not.Contain(compositionContentKey));
        Assert.That(draftDocuments.Select(d => d.Id), Contains.Item(composingContentKey));

        publishedDocuments = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(publishedDocuments, Has.Count.EqualTo(7));
        Assert.That(publishedDocuments.Select(d => d.Id), Does.Not.Contain(compositionContentKey));
        Assert.That(publishedDocuments.Select(d => d.Id), Contains.Item(composingContentKey));
    }

    [Test]
    public async Task DeleteAllContentTypes()
    {
        IReadOnlyList<TestIndexDocument> draftDocuments = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(draftDocuments, Has.Count.EqualTo(6));

        IReadOnlyList<TestIndexDocument> publishedDocuments = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(publishedDocuments, Has.Count.EqualTo(6));

        await ContentTypeService.DeleteAsync(_contentType1.Key, Constants.Security.SuperUserKey);
        await ContentTypeService.DeleteAsync(_contentType2.Key, Constants.Security.SuperUserKey);
        await ContentTypeService.DeleteAsync(_contentType3.Key, Constants.Security.SuperUserKey);

        draftDocuments = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(draftDocuments, Is.Empty);

        publishedDocuments = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(publishedDocuments, Is.Empty);
    }
}
