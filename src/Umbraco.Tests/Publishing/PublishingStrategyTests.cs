using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using System.Linq;
using Umbraco.Core.Services;

namespace Umbraco.Tests.Publishing
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture]
    public class PublishingStrategyTests : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            //LegacyUmbracoSettings.SettingsFilePath = IOHelper.MapPath(SystemDirectories.Config + Path.DirectorySeparatorChar, false);              
        }

        [TearDown]
        public override void TearDown()
        {
			base.TearDown();
            
            //ensure event handler is gone
            ContentService.Publishing -= StrategyPublishing;            
        }

        private IContent _homePage;

        /// <summary>
        /// in these tests we have a heirarchy of 
        /// - home
        /// -- text page 1
        /// -- text page 2
        /// --- text page 3
        /// 
        /// For this test, none of them are published, then we bulk publish them all, however one of the nodes will fail publishing
        /// because it is not valid, then it's children won't be published either because it's never been published.
        /// </summary>
        [Test]
        public void Publishes_Many_Ignores_Invalid_Items_And_Children()
        {
            var testData = CreateTestData();
            //Create some other data which are descendants of Text Page 2
            var mandatorContent = MockedContent.CreateSimpleContent(
                ServiceContext.ContentTypeService.Get("umbMandatory"), "Invalid Content", testData.Single(x => x.Name == "Text Page 2").Id);
            mandatorContent.SetValue("author", string.Empty);
            ServiceContext.ContentService.Save(mandatorContent, 0);
            var subContent = MockedContent.CreateSimpleContent(
                ServiceContext.ContentTypeService.Get("umbTextpage"), "Sub Sub Sub", mandatorContent.Id);
            ServiceContext.ContentService.Save(subContent, 0);
            
            //publish root and nodes at it's children level
            var listToPublish = ServiceContext.ContentService.GetDescendants(_homePage.Id).Concat(new[] { _homePage });
            var evtMsgs = new EventMessages();
            var result = ((ContentService)ServiceContext.ContentService).StrategyPublishWithChildren(listToPublish, null, 0, evtMsgs, true);

            Assert.AreEqual(listToPublish.Count() - 2, result.Count(x => x.Success));
            Assert.IsTrue(result.Where(x => x.Success).Select(x => x.Result.ContentItem.Id)
                                .ContainsAll(listToPublish.Where(x => x.Name != "Invalid Content" && x.Name != "Sub Sub Sub").Select(x => x.Id)));
        }

        /// <summary>
        /// in these tests we have a heirarchy of 
        /// - home
        /// -- text page 1
        /// -- text page 2
        /// --- text page 3
        /// 
        /// For this test, none of them are published, then we bulk publish them all, however we cancel the publishing for
        /// "text page 2". This internally will ensure that text page 3 doesn't get published either because text page 2 has 
        /// never been published.
        /// </summary>
        [Test]
        public void Publishes_Many_Ignores_Cancelled_Items_And_Children()
        {
            CreateTestData();

            ContentService.Publishing += StrategyPublishing;

            //publish root and nodes at it's children level
            var listToPublish = ServiceContext.ContentService.GetDescendants(_homePage.Id).Concat(new[] {_homePage});
            var evtMsgs = new EventMessages();
            var result = ((ContentService)ServiceContext.ContentService).StrategyPublishWithChildren(listToPublish, null, 0, evtMsgs);
            
            Assert.AreEqual(listToPublish.Count() - 2, result.Count(x => x.Success));
            Assert.IsTrue(result.Where(x => x.Success).Select(x => x.Result.ContentItem.Id)
                                .ContainsAll(listToPublish.Where(x => x.Name != "Text Page 2" && x.Name != "Text Page 3").Select(x => x.Id)));
        }

        static void StrategyPublishing(IContentService sender, PublishEventArgs<IContent> e)
        {
            if (e.PublishedEntities.Any(x => x.Name == "Text Page 2"))
                e.Cancel = true;
        }

        [Test]
        public void Publishes_Many_Ignores_Unpublished_Items()
        {
            CreateTestData();

            var evtMsgs = new EventMessages();

            //publish root and nodes at it's children level
            var result1 = ((ContentService)ServiceContext.ContentService).StrategyPublish(_homePage, false, 0, evtMsgs);
            Assert.IsTrue(result1);
            Assert.IsTrue(_homePage.Published);
            foreach (var c in ServiceContext.ContentService.GetChildren(_homePage.Id))
            {
                var r = ((ContentService)ServiceContext.ContentService).StrategyPublish(c, false, 0, evtMsgs);    
                Assert.IsTrue(r);
                Assert.IsTrue(c.Published);
            }

            //ok, all are published except the deepest descendant, we will pass in a flag to not include it to 
            //be published - beware that StrategyPublishWithChildren expects content to be ordered by level
            var content = new[] {_homePage}.Union(ServiceContext.ContentService.GetDescendants(_homePage));
            var result = ((ContentService)ServiceContext.ContentService).StrategyPublishWithChildren(content, null, 0, evtMsgs, false);
            //all of them will be SuccessAlreadyPublished unless the unpublished one gets included, in that case
            //we'll have a 'Success' result which we don't want.
            // NOTE - not true, we'll have Success for _homePage because we DO want to publish it
            Assert.AreEqual(1, result.Count(x => x.Result.StatusType == PublishStatusType.Success));
        }

        [Test]
        public void Publishes_Many_Does_Not_Ignore_Unpublished_Items()
        {
            CreateTestData();

            var evtMsgs = new EventMessages();

            //publish root and nodes at it's children level
            var result1 = ((ContentService)ServiceContext.ContentService).StrategyPublish(_homePage, false, 0, evtMsgs);
            Assert.IsTrue(result1);
            Assert.IsTrue(_homePage.Published);
            
            //NOTE (MCH) This isn't persisted, so not really a good test as it will look like the result should be something else.
            foreach (var c in ServiceContext.ContentService.GetChildren(_homePage.Id))
            {
                var r = ((ContentService)ServiceContext.ContentService).StrategyPublish(c, false, 0, evtMsgs);
                Assert.IsTrue(r);
                Assert.IsTrue(c.Published);
            }

            //NOTE (MCH) when doing the test like this the Publish status will not actually have been persisted
            //since its only updating a property. The actual persistence and publishing is done through the ContentService.
            //So when descendants are fetched from the ContentService the Publish status will be "reset", which
            //means the result will be 1 'SuccessAlreadyPublished' and 3 'Success' because the Homepage is
            //inserted in the list and since that item has the status of already being Published it will be the one item
            //with 'SuccessAlreadyPublished'
            //
            // NOTE - this is WRONG, the first one (_homepage) we really want to publish!
            // not sure this test makes sense at all...

            var contents = new[] {_homePage} // top-level MUST be the first one here
                .Concat(ServiceContext.ContentService.GetDescendants(_homePage));
            var result = ((ContentService)ServiceContext.ContentService).StrategyPublishWithChildren(contents, null, 0, evtMsgs, true);
            
            Assert.AreEqual(4, result.Count(x => x.Result.StatusType == PublishStatusType.Success));
            Assert.AreEqual(0, result.Count(x => x.Result.StatusType == PublishStatusType.SuccessAlreadyPublished));
            Assert.IsTrue(result.First(x => x.Result.StatusType == PublishStatusType.Success).Success);
            Assert.IsTrue(result.First(x => x.Result.StatusType == PublishStatusType.Success).Result.ContentItem.Published);
        }

        [NUnit.Framework.Ignore]
        [Test]
        public void Can_Publish_And_Update_Xml_Cache()
        {
            //TODO Create new test
        }

        public IEnumerable<IContent> CreateTestData()
        {
            //NOTE Maybe not the best way to create/save test data as we are using the services, which are being tested.

            //Create and Save ContentType "umbTextpage" -> 1045
            ContentType contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage", "Textpage");
            ServiceContext.ContentTypeService.Save(contentType);
            var mandatoryType = MockedContentTypes.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type", true);
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