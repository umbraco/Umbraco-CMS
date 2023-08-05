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
    protected void IndexContent()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);
        var contentType = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);
        IContent content = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1");
        content.SetValue("author", "John Doe");

            ContentService.SaveAndPublish(content);

    }
        [Test]
        public void Indexing_On_Publish_Works()
        {
            IndexContent();
            var apiIndex = SearchProvider.GetSearcher(Constants
                .UmbracoIndexes
                .DeliveryApiContentIndexName);
            Assert.AreNotEqual(null, apiIndex);
            var apiIndexResult = apiIndex.Search("test", 1, 10);
            Assert.AreEqual(1, apiIndexResult.TotalItemCount);
            var internalIndex = SearchProvider.GetSearcher(Constants
                .UmbracoIndexes
                .InternalIndexName);
            Assert.AreNotEqual(null, internalIndex);
            var internalIndexResult = internalIndex.Search("test", 1, 10);
            Assert.AreEqual(1, internalIndexResult.TotalItemCount);
            var externalIndex = SearchProvider.GetSearcher(Constants
                .UmbracoIndexes
                .DeliveryApiContentIndexName);
            Assert.AreNotEqual(null, externalIndex);

            var externalIndexResult = internalIndex.Search("test", 1, 10);
            Assert.AreEqual(1, externalIndexResult.TotalItemCount);
        }
}
