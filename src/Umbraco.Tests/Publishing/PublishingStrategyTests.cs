using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using System.Linq;
using Umbraco.Core.Services;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Publishing
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class PublishingStrategyTests : TestWithDatabaseBase
    {
        public override void SetUp()
        {
            base.SetUp();

            //LegacyUmbracoSettings.SettingsFilePath = IOHelper.MapPath(SystemDirectories.Config + Path.DirectorySeparatorChar, false);
        }

        private IContent _homePage;

        [NUnit.Framework.Ignore("fixme - ignored test")]
        [Test]
        public void Can_Publish_And_Update_Xml_Cache()
        {
            // TODO: Create new test
        }

        public IEnumerable<IContent> CreateTestData()
        {
            //NOTE Maybe not the best way to create/save test data as we are using the services, which are being tested.

            //Create and Save ContentType "umbTextpage" -> 1045
            ContentType contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage", "Textpage");
            ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
            ServiceContext.ContentTypeService.Save(contentType);
            var mandatoryType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
            //ServiceContext.FileService.SaveTemplate(mandatoryType.DefaultTemplate); // else, FK violation on contentType!
            ServiceContext.ContentTypeService.Save(mandatoryType);

            //Create and Save Content "Homepage" based on "umbTextpage" -> 1046
            _homePage = MockedContent.CreateSimpleContent(contentType);
            ServiceContext.ContentService.Save(_homePage, 0);

            //Create and Save Content "Text Page 1" based on "umbTextpage" -> 1047
            Content subpage = MockedContent.CreateSimpleContent(contentType, "Text Page 1", _homePage.Id);
            ServiceContext.ContentService.Save(subpage, 0);

            //Create and Save Content "Text Page 2" based on "umbTextpage" -> 1048
            Content subpage2 = MockedContent.CreateSimpleContent(contentType, "Text Page 2", _homePage.Id);
            ServiceContext.ContentService.Save(subpage2, 0);

            //Create and Save Content "Text Page 3" based on "umbTextpage" -> 1048
            Content subpage3 = MockedContent.CreateSimpleContent(contentType, "Text Page 3", subpage2.Id);
            ServiceContext.ContentService.Save(subpage3, 0);

            return new[] {_homePage, subpage, subpage2, subpage3};
        }
    }
}
