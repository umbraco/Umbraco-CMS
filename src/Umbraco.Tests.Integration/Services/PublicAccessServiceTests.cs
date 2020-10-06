using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Integration.Services
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class PublicAccessServiceTests : UmbracoIntegrationTest
    {
        private IContentService _contentService => GetRequiredService<IContentService>();
        private IContentTypeService _contentTypeService => GetRequiredService<IContentTypeService>();
        private IFileService _fileService => GetRequiredService<IFileService>();
        private IPublicAccessService _publicAccessService => GetRequiredService<IPublicAccessService>();

        [Test]
        public void Can_Add_New_Entry()
        {
            // Arrange
            var ct = MockedContentTypes.CreateSimpleContentType("blah", "Blah");
            _fileService.SaveTemplate(ct.DefaultTemplate);
            _contentTypeService.Save(ct);
            var c = MockedContent.CreateSimpleContent(ct, "Test", -1);
            _contentService.Save(c);

            // Act
            var entry = new PublicAccessEntry(c, c, c, new[]
            {
                new PublicAccessRule()
                {
                    RuleType = "TestType",
                    RuleValue = "TestVal"
                },
            });
            var result = _publicAccessService.Save(entry);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(OperationResultType.Success, result.Result.Result);
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
            var ct = MockedContentTypes.CreateSimpleContentType("blah", "Blah");
            _fileService.SaveTemplate(ct.DefaultTemplate);
            _contentTypeService.Save(ct);
            var c = MockedContent.CreateSimpleContent(ct, "Test", -1);
            _contentService.Save(c);
            var entry = new PublicAccessEntry(c, c, c, new[]
            {
                new PublicAccessRule()
                {
                    RuleType = "TestType",
                    RuleValue = "TestVal"
                },
            });
            _publicAccessService.Save(entry);

            // Act
            var updated = _publicAccessService.AddRule(c, "TestType2", "AnotherVal");
            //re-get
            entry = _publicAccessService.GetEntryForContent(c);

            // Assert
            Assert.IsTrue(updated.Success);
            Assert.AreEqual(OperationResultType.Success, updated.Result.Result);
            Assert.AreEqual(2, entry.Rules.Count());
        }

        [Test]
        public void Can_Add_Multiple_Value_For_Same_Rule_Type()
        {
            // Arrange
            var ct = MockedContentTypes.CreateSimpleContentType("blah", "Blah");
            _fileService.SaveTemplate(ct.DefaultTemplate);
            _contentTypeService.Save(ct);
            var c = MockedContent.CreateSimpleContent(ct, "Test", -1);
            _contentService.Save(c);
            var entry = new PublicAccessEntry(c, c, c, new[]
            {
                new PublicAccessRule()
                {
                    RuleType = "TestType",
                    RuleValue = "TestVal"
                },
            });
            _publicAccessService.Save(entry);

            // Act
            var updated1 = _publicAccessService.AddRule(c, "TestType", "AnotherVal1");
            var updated2 = _publicAccessService.AddRule(c, "TestType", "AnotherVal2");

            //re-get
            entry = _publicAccessService.GetEntryForContent(c);

            // Assert
            Assert.IsTrue(updated1.Success);
            Assert.IsTrue(updated2.Success);
            Assert.AreEqual(OperationResultType.Success, updated1.Result.Result);
            Assert.AreEqual(OperationResultType.Success, updated2.Result.Result);
            Assert.AreEqual(3, entry.Rules.Count());
        }

        [Test]
        public void Can_Remove_Rule()
        {
            // Arrange
            var ct = MockedContentTypes.CreateSimpleContentType("blah", "Blah");
            _fileService.SaveTemplate(ct.DefaultTemplate);
            _contentTypeService.Save(ct);
            var c = MockedContent.CreateSimpleContent(ct, "Test", -1);
            _contentService.Save(c);
            var entry = new PublicAccessEntry(c, c, c, new[]
            {
                new PublicAccessRule()
                {
                    RuleType = "TestType",
                    RuleValue = "TestValue1"
                },
                new PublicAccessRule()
                {
                    RuleType = "TestType",
                    RuleValue = "TestValue2"
                },
            });
            _publicAccessService.Save(entry);

            // Act
            var removed = _publicAccessService.RemoveRule(c, "TestType", "TestValue1");
            //re-get
            entry = _publicAccessService.GetEntryForContent(c);

            // Assert
            Assert.IsTrue(removed.Success);
            Assert.AreEqual(OperationResultType.Success, removed.Result.Result);
            Assert.AreEqual(1, entry.Rules.Count());
            Assert.AreEqual("TestValue2", entry.Rules.ElementAt(0).RuleValue);
        }
    }
}
