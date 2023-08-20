using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.baseTests;

[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class UmbracoSearchBaseTests : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IFileService FileService => GetRequiredService<IFileService>();


    private IContentService ContentService => GetRequiredService<IContentService>();

    protected IEnumerable<IContent> CreateContent()
    {
        var list = new List<IContent>();
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);
        var contentType = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);
        IContent content = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1");
        content.SetValue("author", "John Test Doe");
        content.Id = 1;
        list.Add(content);
        IContent content2 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 2");
        content2.SetValue("author", "John Test 2 Doe");
        content.Id = 2;
        list.Add(content2);
        return list;
    }

    [Test]
    public void CanIndexSingle()
    {
     var content =CreateContent();
        var apiIndex = SearchProvider.GetIndex<IContentBase>(Constants
            .UmbracoIndexes
            .DeliveryApiContentIndexName);
        apiIndex.IndexItems(content.First().AsEnumerableOfOne().ToArray());

        var apiSearch = SearchProvider.GetSearcher(Constants
            .UmbracoIndexes
            .DeliveryApiContentIndexName);
        Assert.AreNotEqual(null, apiSearch);
        var apiIndexResult = apiSearch.Search("test", 1, 10);
        Assert.AreEqual(1, apiIndexResult.TotalItemCount);
    }
    [Test]
    public void CanIndexMultiple()
    {
        var content =CreateContent();
        var apiIndex = SearchProvider.GetIndex<IContentBase>(Constants
            .UmbracoIndexes
            .DeliveryApiContentIndexName);
        apiIndex.IndexItems(content.ToArray());

        var apiSearch = SearchProvider.GetSearcher(Constants
            .UmbracoIndexes
            .DeliveryApiContentIndexName);
        Assert.AreNotEqual(null, apiSearch);
        var apiIndexResult = apiSearch.Search("test", 1, 10);
        Assert.AreEqual(2, apiIndexResult.TotalItemCount);
    }
}
