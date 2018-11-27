using Examine;
using NUnit.Framework;
using Umbraco.Examine;
using Moq;
using Umbraco.Core.Services;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models;
using System;
using System.Linq;

namespace Umbraco.Tests.UmbracoExamine
{
    [TestFixture]
    public class UmbracoContentValueSetValidatorTests
    {
        [Test]
        public void Must_Have_Path()
        {
            var validator = new UmbracoContentValueSetValidator(
                new UmbracoContentIndexerOptions(true, true, null),
                Mock.Of<IPublicAccessService>());

            var result = validator.Validate(new ValueSet("555", IndexTypes.Content, new { hello = "world" }));
            Assert.IsFalse(result);

            result = validator.Validate(new ValueSet("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
            Assert.IsTrue(result);
        }

        [Test]
        public void Parent_Id()
        {
            var validator = new UmbracoContentValueSetValidator(
                new UmbracoContentIndexerOptions(true, true, 555),
                Mock.Of<IPublicAccessService>());

            var result = validator.Validate(new ValueSet("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
            Assert.IsFalse(result);

            result = validator.Validate(new ValueSet("555", IndexTypes.Content, new { hello = "world", path = "-1,444" }));
            Assert.IsFalse(result);

            result = validator.Validate(new ValueSet("555", IndexTypes.Content, new { hello = "world", path = "-1,555,777" }));
            Assert.IsTrue(result);

            result = validator.Validate(new ValueSet("555", IndexTypes.Content, new { hello = "world", path = "-1,555,777,999" }));
            Assert.IsTrue(result);
        }

        [Test]
        public void Inclusion_List()
        {
            var validator = new UmbracoContentValueSetValidator(
                new UmbracoContentIndexerOptions(true, true, includeContentTypes: new List<string> { "include-content" }),
                Mock.Of<IPublicAccessService>());

            var result = validator.Validate(new ValueSet("555", IndexTypes.Content, "test-content",  new { hello = "world", path = "-1,555" }));
            Assert.IsFalse(result);

            result = validator.Validate(new ValueSet("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
            Assert.IsFalse(result);

            result = validator.Validate(new ValueSet("555", IndexTypes.Content, "include-content", new { hello = "world", path = "-1,555" }));
            Assert.IsTrue(result);
        }

        [Test]
        public void Exclusion_List()
        {
            var validator = new UmbracoContentValueSetValidator(
                new UmbracoContentIndexerOptions(true, true, excludeContentTypes: new List<string> { "exclude-content" }),
                Mock.Of<IPublicAccessService>());

            var result = validator.Validate(new ValueSet("555", IndexTypes.Content, "test-content", new { hello = "world", path = "-1,555" }));
            Assert.IsTrue(result);

            result = validator.Validate(new ValueSet("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
            Assert.IsTrue(result);

            result = validator.Validate(new ValueSet("555", IndexTypes.Content, "exclude-content", new { hello = "world", path = "-1,555" }));
            Assert.IsFalse(result);
        }

        [Test]
        public void Inclusion_Exclusion_List()
        {
            var validator = new UmbracoContentValueSetValidator(
                new UmbracoContentIndexerOptions(true, true, 
                    includeContentTypes: new List<string> { "include-content", "exclude-content" },
                    excludeContentTypes: new List<string> { "exclude-content" }),
                Mock.Of<IPublicAccessService>());

            var result = validator.Validate(new ValueSet("555", IndexTypes.Content, "test-content", new { hello = "world", path = "-1,555" }));
            Assert.IsFalse(result);

            result = validator.Validate(new ValueSet("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
            Assert.IsFalse(result);

            result = validator.Validate(new ValueSet("555", IndexTypes.Content, "exclude-content", new { hello = "world", path = "-1,555" }));
            Assert.IsFalse(result);

            result = validator.Validate(new ValueSet("555", IndexTypes.Content, "include-content", new { hello = "world", path = "-1,555" }));
            Assert.IsTrue(result);
        }

        [Test]
        public void Recycle_Bin()
        {
            var validator = new UmbracoContentValueSetValidator(
                new UmbracoContentIndexerOptions(false, true, null),
                Mock.Of<IPublicAccessService>());

            var result = validator.Validate(new ValueSet("555", IndexTypes.Content, new { hello = "world", path = "-1,-20,555" }));
            Assert.IsFalse(result);

            result = validator.Validate(new ValueSet("555", IndexTypes.Content, new { hello = "world", path = "-1,-20,555,777" }));
            Assert.IsFalse(result);

            result = validator.Validate(new ValueSet("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
            Assert.IsFalse(result);

            result = validator.Validate(new ValueSet("555", IndexTypes.Content,
                new Dictionary<string, object>
                {
                    ["hello"] = "world",
                    ["path"] = "-1,555",
                    [UmbracoExamineIndexer.PublishedFieldName] = 1
                }));
            Assert.IsTrue(result);
        }

        [Test]
        public void Published_Only()
        {
            var validator = new UmbracoContentValueSetValidator(
                new UmbracoContentIndexerOptions(false, true, null),
                Mock.Of<IPublicAccessService>());

            var result = validator.Validate(new ValueSet("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
            Assert.IsFalse(result);

            result = validator.Validate(new ValueSet("555", IndexTypes.Content,
                new Dictionary<string, object>
                {
                    ["hello"] = "world",
                    ["path"] = "-1,555",
                    [UmbracoExamineIndexer.PublishedFieldName] = 0
                }));
            Assert.IsFalse(result);

            result = validator.Validate(new ValueSet("555", IndexTypes.Content,
                new Dictionary<string, object>
                {
                    ["hello"] = "world",
                    ["path"] = "-1,555",
                    [UmbracoExamineIndexer.PublishedFieldName] = 1
                }));
            Assert.IsTrue(result);
        }

        [Test]
        public void Published_Only_With_Variants()
        {
            var validator = new UmbracoContentValueSetValidator(
                new UmbracoContentIndexerOptions(false, true, null),
                Mock.Of<IPublicAccessService>());

            var result = validator.Validate(new ValueSet("555", IndexTypes.Content,
                new Dictionary<string, object>
                {
                    ["hello"] = "world",
                    ["path"] = "-1,555",
                    [UmbracoContentIndexer.VariesByCultureFieldName] = 1,
                    [UmbracoExamineIndexer.PublishedFieldName] = 0
                }));
            Assert.IsFalse(result);

            result = validator.Validate(new ValueSet("555", IndexTypes.Content,
                new Dictionary<string, object>
                {
                    ["hello"] = "world",
                    ["path"] = "-1,555",
                    [UmbracoContentIndexer.VariesByCultureFieldName] = 1,
                    [UmbracoExamineIndexer.PublishedFieldName] = 1
                }));
            Assert.IsTrue(result);

            var valueSet = new ValueSet("555", IndexTypes.Content,
                new Dictionary<string, object>
                {
                    ["hello"] = "world",
                    ["path"] = "-1,555",
                    [UmbracoContentIndexer.VariesByCultureFieldName] = 1,
                    [$"{UmbracoExamineIndexer.PublishedFieldName}_en-us"] = 1,
                    ["hello_en-us"] = "world",
                    ["title_en-us"] = "my title",
                    [$"{UmbracoExamineIndexer.PublishedFieldName}_es-es"] = 0,
                    ["hello_es-ES"] = "world",
                    ["title_es-ES"] = "my title",
                    [UmbracoExamineIndexer.PublishedFieldName] = 1
                });
            Assert.AreEqual(10, valueSet.Values.Count()); 
            Assert.IsTrue(valueSet.Values.ContainsKey($"{UmbracoExamineIndexer.PublishedFieldName}_es-es"));
            Assert.IsTrue(valueSet.Values.ContainsKey("hello_es-ES"));
            Assert.IsTrue(valueSet.Values.ContainsKey("title_es-ES"));

            result = validator.Validate(valueSet);
            Assert.IsTrue(result);

            Assert.AreEqual(7, valueSet.Values.Count()); //filtered to 7 values (removes es-es values)
            Assert.IsFalse(valueSet.Values.ContainsKey($"{UmbracoExamineIndexer.PublishedFieldName}_es-es"));
            Assert.IsFalse(valueSet.Values.ContainsKey("hello_es-ES"));
            Assert.IsFalse(valueSet.Values.ContainsKey("title_es-ES"));
        }

        [Test]
        public void Non_Protected()
        {
            var publicAccessService = new Mock<IPublicAccessService>();
            publicAccessService.Setup(x => x.IsProtected("-1,555"))
                .Returns(Attempt.Succeed(new PublicAccessEntry(Guid.NewGuid(), 555, 444, 333, Enumerable.Empty<PublicAccessRule>())));
            publicAccessService.Setup(x => x.IsProtected("-1,777"))
                .Returns(Attempt.Fail<PublicAccessEntry>());
            var validator = new UmbracoContentValueSetValidator(
                new UmbracoContentIndexerOptions(true, false, null),
                publicAccessService.Object);

            var result = validator.Validate(new ValueSet("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
            Assert.IsFalse(result);

            result = validator.Validate(new ValueSet("777", IndexTypes.Content, new { hello = "world", path = "-1,777" }));
            Assert.IsTrue(result);
        }
    }
}
