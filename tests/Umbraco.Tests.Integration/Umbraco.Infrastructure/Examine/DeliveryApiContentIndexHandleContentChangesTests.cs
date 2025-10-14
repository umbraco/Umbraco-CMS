using Examine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Examine.Lucene.UmbracoExamine;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Examine;

[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
[TestFixture]
public class DeliveryApiContentIndexHandleContentChangesTests : ExamineBaseTest
{

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder.AddDeliveryApi();
        builder.Services.Configure<DeliveryApiSettings>(settings => settings.Enabled = true);
    }

    /// <summary>
    /// Test for https://github.com/umbraco/Umbraco-CMS/issues/20370
    /// </summary>
    [Test]
    public async Task BranchContentIsIndexed()
    {
        // setup
        // setup doc structure without publishing
        var doctype = await CreateNestedTextPageContentType();
        var documents = CreateDocumentStructure(doctype);
        // Publish branch root
        ContentService.Publish(documents[0], []);

        // act
        await ExecuteAndWaitForIndexing(
            () => ContentService.PublishBranch(documents[0], PublishBranchFilter.IncludeUnpublished, []),
            Constants.UmbracoIndexes.DeliveryApiContentIndexName);

        // assert
        ExamineManager.TryGetIndex(Constants.UmbracoIndexes.DeliveryApiContentIndexName, out IIndex index);

        if (index is null)
        {
            throw new Exception("DeliveryApiContentIndex not found");
        }

        foreach (var document in documents)
        {
            var searchResult = index.Searcher.Search(document.Key.ToString());
        }

        // all descendants are indexed and marked as published
    }

    private async Task<IContentType> CreateNestedTextPageContentType()
    {
        var nestedTextPageContentTypekey = Guid.NewGuid();
        var nestedTextPageContentType = new ContentTypeBuilder()
            .WithAlias("textPage")
            .WithName("Text Page")
            .WithKey(nestedTextPageContentTypekey)
            .AddAllowedContentType()
            .WithKey(nestedTextPageContentTypekey)
            .Done()
            .AddPropertyGroup()
            .WithAlias("content")
            .WithName("Content")
            .WithSupportsPublishing(true)
            .AddPropertyType()
            .WithAlias("title")
            .WithName("Title")
            .Done()
            .Done()
            .WithAllowAsRoot(true)
            .Build();

        var result = await ContentTypeService.CreateAsync(nestedTextPageContentType, Constants.Security.SuperUserKey);
        if (result.Success is false)
        {
            throw new Exception("Failed to create content type");
        }

        return nestedTextPageContentType;
    }

    private IContent[] CreateDocumentStructure(IContentType doctype)
    {
        var rootContent = new ContentBuilder()
            .WithContentType(doctype)
            .WithName("root")
            .WithKey(Guid.NewGuid())
            .Build();
        ContentService.Save(rootContent);

        var root_1Content = new ContentBuilder()
            .WithContentType(doctype)
            .WithName("root_1")
            .WithParentId(rootContent.Id)
            .WithKey(Guid.NewGuid())
            .Build();
        ContentService.Save(root_1Content);

        var root_1_1Content = new ContentBuilder()
            .WithContentType(doctype)
            .WithName("root_1_1")
            .WithParentId(root_1Content.Id)
            .WithKey(Guid.NewGuid())
            .Build();
        ContentService.Save(root_1_1Content);

        var root_1_2Content = new ContentBuilder()
            .WithContentType(doctype)
            .WithName("root_1_2")
            .WithParentId(root_1Content.Id)
            .WithKey(Guid.NewGuid())
            .Build();
        ContentService.Save(root_1_2Content);

        var root_2Content = new ContentBuilder()
            .WithContentType(doctype)
            .WithName("root_2")
            .WithParentId(rootContent.Id)
            .WithKey(Guid.NewGuid())
            .Build();
        ContentService.Save(root_2Content);

        var root_2_1Content = new ContentBuilder()
            .WithContentType(doctype)
            .WithName("root_2_1")
            .WithParentId(root_2Content.Id)
            .WithKey(Guid.NewGuid())
            .Build();
        ContentService.Save(root_2_1Content);

        var root_2_2Content = new ContentBuilder()
            .WithContentType(doctype)
            .WithName("root_2_2")
            .WithParentId(root_2Content.Id)
            .WithKey(Guid.NewGuid())
            .Build();
        ContentService.Save(root_2_2Content);

        return
        [
            rootContent,
            root_1Content,
            root_1_1Content,
            root_1_2Content,
            root_2Content,
            root_2_1Content,
            root_2_2Content
        ];
    }
}
