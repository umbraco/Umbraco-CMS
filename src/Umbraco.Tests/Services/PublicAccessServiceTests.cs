using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Services
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture, RequiresSTA]
    public class PublicAccessServiceTests : BaseServiceTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void Can_Add_New_Entry()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var ct = MockedContentTypes.CreateSimpleContentType("blah", "Blah");
            contentTypeService.Save(ct);
            var c = MockedContent.CreateSimpleContent(ct, "Test", -1);
            contentService.Save(c);
            var publicAccessService = ServiceContext.PublicAccessService;


            // Act
            var entry = new PublicAccessEntry(c, c, c, new[]
           {
                new PublicAccessRule()
                {
                    RuleType = "TestType",
                    RuleValue = "TestVal"
                },
            });
            var result = publicAccessService.Save(entry);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(OperationStatusType.Success, result.Result.StatusType);
            Assert.IsTrue(entry.HasIdentity);
            Assert.AreNotEqual(entry.Key, Guid.Empty);
            Assert.AreEqual(c.Id, entry.LoginNodeId);
            Assert.AreEqual(c.Id, entry.NoAccessNodeId);
            Assert.AreEqual(c.Id, entry.ProtectedNodeId);
        }

        [Test]
        public void Can_Add_Rule()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var ct = MockedContentTypes.CreateSimpleContentType("blah", "Blah");
            contentTypeService.Save(ct);
            var c = MockedContent.CreateSimpleContent(ct, "Test", -1);
            contentService.Save(c);
            var publicAccessService = ServiceContext.PublicAccessService;
            var entry = new PublicAccessEntry(c, c, c, new[]
           {
                new PublicAccessRule()
                {
                    RuleType = "TestType",
                    RuleValue = "TestVal"
                },
            });
            publicAccessService.Save(entry);

            // Act
            var updated = publicAccessService.AddOrUpdateRule(c, "TestType2", "AnotherVal");
            //re-get
            entry = publicAccessService.GetEntryForContent(c);

            // Assert           
            Assert.IsTrue(updated.Success);
            Assert.AreEqual(OperationStatusType.Success, updated.Result.StatusType);
            Assert.AreEqual(2, entry.Rules.Count());
        }

        [Test]
        public void Can_Update_Rule()
        {
            // Arrange
            var contentService = ServiceContext.ContentService;
            var contentTypeService = ServiceContext.ContentTypeService;
            var ct = MockedContentTypes.CreateSimpleContentType("blah", "Blah");
            contentTypeService.Save(ct);
            var c = MockedContent.CreateSimpleContent(ct, "Test", -1);
            contentService.Save(c);
            var publicAccessService = ServiceContext.PublicAccessService;
            var entry = new PublicAccessEntry(c, c, c, new[]
           {
                new PublicAccessRule()
                {
                    RuleType = "TestType",
                    RuleValue = "TestValue"
                },
            });
            publicAccessService.Save(entry);

            // Act
            var updated = publicAccessService.AddOrUpdateRule(c, "TestType", "AnotherVal");
            //re-get
            entry = publicAccessService.GetEntryForContent(c);

            // Assert           
            Assert.IsTrue(updated.Success);
            Assert.AreEqual(OperationStatusType.Success, updated.Result.StatusType);
            Assert.AreEqual(1, entry.Rules.Count());
            Assert.AreEqual("AnotherVal", entry.Rules.ElementAt(0).RuleValue);
        }
    }
}