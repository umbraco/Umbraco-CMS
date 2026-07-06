using Examine;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Tests.Search.Examine.Integration.Tests.ContentTests.IndexService;

public partial class InvariantDocumentTreeTests
{
    [Test]
    public async Task DraftStructure_YieldsAllDocuments()
    {
        await CreateInvariantDocumentTree(false);
        IIndex index = GetIndex(Cms.Search.Core.Constants.IndexAliases.DraftContent);

        ISearchResult[] results = index.Searcher.CreateQuery().All().Execute().ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(results.Length, Is.EqualTo(3));
            Assert.That(results[0].Id, Is.EqualTo(RootKey.ToString()));
            Assert.That(results[1].Id, Is.EqualTo(ChildKey.ToString()));
            Assert.That(results[2].Id, Is.EqualTo(GrandchildKey.ToString()));
        });
    }

    [Test]
    public async Task DraftStructure_YieldsNoPublishedDocuments()
    {
        await CreateInvariantDocumentTree(false);
        IIndex index = GetIndex(Cms.Search.Core.Constants.IndexAliases.PublishedContent);

        ISearchResults results = index.Searcher.CreateQuery().All().Execute();
        Assert.That(results.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task DraftStructure_WithRootInRecycleBin_YieldsAllDocuments()
    {
        await CreateInvariantDocumentTree(false);

        await WaitForIndexing(Cms.Search.Core.Constants.IndexAliases.DraftContent, () =>
        {
            IContent root = ContentService.GetById(RootKey)!;
            ContentService.MoveToRecycleBin(root);
            return Task.CompletedTask;
        });

        IIndex index = GetIndex(Cms.Search.Core.Constants.IndexAliases.DraftContent);
        ISearchResult[] results = index.Searcher.CreateQuery().All().Execute().ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(results.Length, Is.EqualTo(3));
            Assert.That(results[0].Id, Is.EqualTo(RootKey.ToString()));
            Assert.That(results[1].Id, Is.EqualTo(ChildKey.ToString()));
            Assert.That(results[2].Id, Is.EqualTo(GrandchildKey.ToString()));
        });
    }


    [Test]
    public async Task DraftStructure_WithChildDeleted_YieldsNothingBelowRoot()
    {
        await CreateInvariantDocumentTree(false);
        await WaitForIndexing(Cms.Search.Core.Constants.IndexAliases.DraftContent, () =>
        {
            IContent child = ContentService.GetById(ChildKey)!;
            ContentService.Delete(child);
            return Task.CompletedTask;
        });

        IIndex index = GetIndex(Cms.Search.Core.Constants.IndexAliases.DraftContent);
        ISearchResult[] results = index.Searcher.CreateQuery().All().Execute().ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(results.Length, Is.EqualTo(1));
            Assert.That(results[0].Id, Is.EqualTo(RootKey.ToString()));
        });
    }

    [Test]
    public async Task DraftStructure_WithGrandchildDeleted_YieldsNothingBelowChild()
    {
        await CreateInvariantDocumentTree(false);
        await WaitForIndexing(Cms.Search.Core.Constants.IndexAliases.DraftContent, () =>
        {
            IContent grandchild = ContentService.GetById(GrandchildKey)!;
            ContentService.Delete(grandchild);
            return Task.CompletedTask;
        });


        IIndex index = GetIndex(Cms.Search.Core.Constants.IndexAliases.DraftContent);
        ISearchResult[] results = index.Searcher.CreateQuery().All().Execute().ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(results.Length, Is.EqualTo(2));
            Assert.That(results[0].Id, Is.EqualTo(RootKey.ToString()));
            Assert.That(results[1].Id, Is.EqualTo(ChildKey.ToString()));
        });
    }

    private async Task CreateInvariantDocumentTree(bool publish)
    {
        DataType dataType = new DataTypeBuilder()
            .WithId(0)
            .WithoutIdentity()
            .WithDatabaseType(ValueStorageType.Decimal)
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.Decimal)
            .Done()
            .Build();

        await DataTypeService.CreateAsync(dataType, Umbraco.Cms.Core.Constants.Security.SuperUserKey);
        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("invariant")
            .AddPropertyType()
            .WithAlias("title")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .AddPropertyType()
            .WithAlias("count")
            .WithDataTypeId(-51)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Integer)
            .Done()
            .AddPropertyType()
            .WithAlias("datetime")
            .WithDataTypeId(Constants.DataTypes.DateTime)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.DateTime)
            .Done()
            .AddPropertyType()
            .WithAlias("decimalproperty")
            .WithDataTypeId(dataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Decimal)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(contentType, Umbraco.Cms.Core.Constants.Security.SuperUserKey);
        contentType.AllowedContentTypes = [new ContentTypeSort(contentType.Key, 0, contentType.Alias)];
        await ContentTypeService.CreateAsync(contentType, Umbraco.Cms.Core.Constants.Security.SuperUserKey);

        Content root = new ContentBuilder()
            .WithKey(RootKey)
            .WithContentType(contentType)
            .WithName("Root")
            .WithPropertyValues(
                new
                {
                    title = "The root title",
                    count = 12,
                    datetime = CurrentDateTimeOffset.DateTime,
                    decimalproperty = DecimalValue,
                })
            .Build();

        await WaitForIndexing(publish ? Cms.Search.Core.Constants.IndexAliases.PublishedContent : Cms.Search.Core.Constants.IndexAliases.DraftContent, () =>
        {
            if (publish)
            {
                SaveAndPublish(root);
            }
            else
            {
                ContentService.Save(root);
            }


            Content child = new ContentBuilder()
                .WithKey(ChildKey)
                .WithContentType(contentType)
                .WithName("Child")
                .WithParent(root)
                .WithPropertyValues(
                    new
                    {
                        title = "The child title",
                        count = 12,
                        datetime = CurrentDateTimeOffset.DateTime,
                        decimalproperty = DecimalValue,
                    })
                .Build();

            if (publish)
            {
                SaveAndPublish(child);
            }
            else
            {
                ContentService.Save(child);
            }

            Content grandchild = new ContentBuilder()
                .WithKey(GrandchildKey)
                .WithContentType(contentType)
                .WithName("Grandchild")
                .WithParent(child)
                .WithPropertyValues(
                    new
                    {
                        title = "The grandchild title",
                        count = 12,
                        datetime = CurrentDateTimeOffset.DateTime,
                        decimalproperty = DecimalValue,
                    })
                .Build();

            if (publish)
            {
                SaveAndPublish(grandchild);
            }
            else
            {
                ContentService.Save(grandchild);
            }

            return Task.CompletedTask;
        });


    }
}
