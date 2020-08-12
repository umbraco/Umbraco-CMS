using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Services;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Publishing
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class PublishingStrategyTests : UmbracoIntegrationTest
    {
        private IContent _homePage;

        [NUnit.Framework.Ignore("fixme - ignored test")]
        [Test]
        public void Can_Publish_And_Update_Xml_Cache()
        {
            // TODO: Create new test
        }

        public IEnumerable<IContent> CreateTestData()
        {

            var fileService = GetRequiredService<IFileService>();
            var contentTypeService = GetRequiredService<IContentTypeService>();
            var contentService = GetRequiredService<IContentService>();
            //NOTE Maybe not the best way to create/save test data as we are using the services, which are being tested.

            //Create and Save ContentType "umbTextpage" -> 1045
            ContentType contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage", "Textpage");
            fileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
            contentTypeService.Save(contentType);
            var mandatoryType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            //ServiceContext.FileService.SaveTemplate(mandatoryType.DefaultTemplate); // else, FK violation on contentType!
            contentTypeService.Save(mandatoryType);

            //Create and Save Content "Homepage" based on "umbTextpage" -> 1046
            _homePage = MockedContent.CreateSimpleContent(contentType);
            contentService.Save(_homePage, 0);

            //Create and Save Content "Text Page 1" based on "umbTextpage" -> 1047
            Content subpage = MockedContent.CreateSimpleContent(contentType, "Text Page 1", _homePage.Id);
            contentService.Save(subpage, 0);

            //Create and Save Content "Text Page 2" based on "umbTextpage" -> 1048
            Content subpage2 = MockedContent.CreateSimpleContent(contentType, "Text Page 2", _homePage.Id);
            contentService.Save(subpage2, 0);

            //Create and Save Content "Text Page 3" based on "umbTextpage" -> 1048
            Content subpage3 = MockedContent.CreateSimpleContent(contentType, "Text Page 3", subpage2.Id);
            contentService.Save(subpage3, 0);

            return new[] {_homePage, subpage, subpage2, subpage3};
        }
    }
}
