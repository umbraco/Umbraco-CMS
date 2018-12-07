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
        public void Invalid_Category()
        {
            var validator = new ContentValueSetValidator(false, true, Mock.Of<IPublicAccessService>());

            var result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
            Assert.AreEqual(ValueSetValidationResult.Valid, result);            

            result = validator.Validate(ValueSet.FromObject("777", IndexTypes.Media, new { hello = "world", path = "-1,555" }));
            Assert.AreEqual(ValueSetValidationResult.Valid, result);

            result = validator.Validate(ValueSet.FromObject("555", "invalid-category", new { hello = "world", path = "-1,555" }));
            Assert.AreEqual(ValueSetValidationResult.Failed, result);

        }

        [Test]
        public void Must_Have_Path()
        {
            var validator = new ContentValueSetValidator(false, true, Mock.Of<IPublicAccessService>());

            var result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, new { hello = "world" }));
            Assert.AreEqual(ValueSetValidationResult.Failed, result);

            result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
            Assert.AreEqual(ValueSetValidationResult.Valid, result);
        }

        [Test]
        public void Parent_Id()
        {
            var validator = new ContentValueSetValidator(false, true, Mock.Of<IPublicAccessService>(), 555);

            var result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
            Assert.AreEqual(ValueSetValidationResult.Filtered, result);

            result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,444" }));
            Assert.AreEqual(ValueSetValidationResult.Filtered, result);

            result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,555,777" }));
            Assert.AreEqual(ValueSetValidationResult.Valid, result);

            result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,555,777,999" }));
            Assert.AreEqual(ValueSetValidationResult.Valid, result);
        }

        [Test]
        public void Inclusion_Field_List()
        {
            var validator = new ValueSetValidator(null, null,
                new[] { "hello", "world" },
                null);

            var valueSet = ValueSet.FromObject("555", IndexTypes.Content, "test-content", new { hello = "world", path = "-1,555", world = "your oyster" });
            var result = validator.Validate(valueSet);
            Assert.AreEqual(ValueSetValidationResult.Filtered, result);

            Assert.IsFalse(valueSet.Values.ContainsKey("path"));
            Assert.IsTrue(valueSet.Values.ContainsKey("hello"));
            Assert.IsTrue(valueSet.Values.ContainsKey("world"));
        }

        [Test]
        public void Exclusion_Field_List()
        {
            var validator = new ValueSetValidator(null, null,
                null,
                new[] { "hello", "world" });

            var valueSet = ValueSet.FromObject("555", IndexTypes.Content, "test-content", new { hello = "world", path = "-1,555", world = "your oyster" });
            var result = validator.Validate(valueSet);
            Assert.AreEqual(ValueSetValidationResult.Filtered, result);

            Assert.IsTrue(valueSet.Values.ContainsKey("path"));
            Assert.IsFalse(valueSet.Values.ContainsKey("hello"));
            Assert.IsFalse(valueSet.Values.ContainsKey("world"));
        }

        [Test]
        public void Inclusion_Exclusion_Field_List()
        {
            var validator = new ValueSetValidator(null, null,
                new[] { "hello", "world" },
                new[] { "world" });

            var valueSet = ValueSet.FromObject("555", IndexTypes.Content, "test-content", new { hello = "world", path = "-1,555", world = "your oyster" });
            var result = validator.Validate(valueSet);
            Assert.AreEqual(ValueSetValidationResult.Filtered, result);

            Assert.IsFalse(valueSet.Values.ContainsKey("path"));
            Assert.IsTrue(valueSet.Values.ContainsKey("hello"));
            Assert.IsFalse(valueSet.Values.ContainsKey("world"));
        }

        [Test]
        public void Inclusion_Type_List()
        {
            var validator = new ContentValueSetValidator(false, true, Mock.Of<IPublicAccessService>(),
                includeItemTypes: new List<string> { "include-content" });

            var result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, "test-content", new { hello = "world", path = "-1,555" }));
            Assert.AreEqual(ValueSetValidationResult.Failed, result);

            result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
            Assert.AreEqual(ValueSetValidationResult.Failed, result);

            result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, "include-content", new { hello = "world", path = "-1,555" }));
            Assert.AreEqual(ValueSetValidationResult.Valid, result);
        }

        [Test]
        public void Exclusion_Type_List()
        {
            var validator = new ContentValueSetValidator(false, true, Mock.Of<IPublicAccessService>(),
                excludeItemTypes: new List<string> { "exclude-content" });

            var result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, "test-content", new { hello = "world", path = "-1,555" }));
            Assert.AreEqual(ValueSetValidationResult.Valid, result);

            result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
            Assert.AreEqual(ValueSetValidationResult.Valid, result);

            result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, "exclude-content", new { hello = "world", path = "-1,555" }));
            Assert.AreEqual(ValueSetValidationResult.Failed, result);
        }

        [Test]
        public void Inclusion_Exclusion_Type_List()
        {
            var validator = new ContentValueSetValidator(false, true, Mock.Of<IPublicAccessService>(),
                includeItemTypes: new List<string> { "include-content", "exclude-content" },
                excludeItemTypes: new List<string> { "exclude-content" });

            var result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, "test-content", new { hello = "world", path = "-1,555" }));
            Assert.AreEqual(ValueSetValidationResult.Failed, result);

            result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
            Assert.AreEqual(ValueSetValidationResult.Failed, result);

            result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, "exclude-content", new { hello = "world", path = "-1,555" }));
            Assert.AreEqual(ValueSetValidationResult.Failed, result);

            result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, "include-content", new { hello = "world", path = "-1,555" }));
            Assert.AreEqual(ValueSetValidationResult.Valid, result);
        }

        [Test]
        public void Recycle_Bin_Content()
        {
            var validator = new ContentValueSetValidator(true, false, Mock.Of<IPublicAccessService>());

            var result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,-20,555" }));
            Assert.AreEqual(ValueSetValidationResult.Failed, result);

            result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,-20,555,777" }));
            Assert.AreEqual(ValueSetValidationResult.Failed, result);

            result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
            Assert.AreEqual(ValueSetValidationResult.Failed, result);

            result = validator.Validate(new ValueSet("555", IndexTypes.Content,
                new Dictionary<string, object>
                {
                    ["hello"] = "world",
                    ["path"] = "-1,555",
                    [UmbracoExamineIndexer.PublishedFieldName] = 1
                }));
            Assert.AreEqual(ValueSetValidationResult.Valid, result);
        }

        [Test]
        public void Recycle_Bin_Media()
        {
            var validator = new ContentValueSetValidator(true, false, Mock.Of<IPublicAccessService>());

            var result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Media, new { hello = "world", path = "-1,-21,555" }));
            Assert.AreEqual(ValueSetValidationResult.Filtered, result);

            result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Media, new { hello = "world", path = "-1,-21,555,777" }));
            Assert.AreEqual(ValueSetValidationResult.Filtered, result);

            result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Media, new { hello = "world", path = "-1,555" }));
            Assert.AreEqual(ValueSetValidationResult.Valid, result);

        }

        [Test]
        public void Published_Only()
        {
            var validator = new ContentValueSetValidator(true, true, Mock.Of<IPublicAccessService>());

            var result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
            Assert.AreEqual(ValueSetValidationResult.Failed, result);

            result = validator.Validate(new ValueSet("555", IndexTypes.Content,
                new Dictionary<string, object>
                {
                    ["hello"] = "world",
                    ["path"] = "-1,555",
                    [UmbracoExamineIndexer.PublishedFieldName] = 0
                }));
            Assert.AreEqual(ValueSetValidationResult.Failed, result);

            result = validator.Validate(new ValueSet("555", IndexTypes.Content,
                new Dictionary<string, object>
                {
                    ["hello"] = "world",
                    ["path"] = "-1,555",
                    [UmbracoExamineIndexer.PublishedFieldName] = 1
                }));
            Assert.AreEqual(ValueSetValidationResult.Valid, result);
        }

        [Test]
        public void Published_Only_With_Variants()
        {
            var validator = new ContentValueSetValidator(true, true, Mock.Of<IPublicAccessService>());

            var result = validator.Validate(new ValueSet("555", IndexTypes.Content,
                new Dictionary<string, object>
                {
                    ["hello"] = "world",
                    ["path"] = "-1,555",
                    [UmbracoContentIndexer.VariesByCultureFieldName] = 1,
                    [UmbracoExamineIndexer.PublishedFieldName] = 0
                }));
            Assert.AreEqual(ValueSetValidationResult.Failed, result);

            result = validator.Validate(new ValueSet("555", IndexTypes.Content,
                new Dictionary<string, object>
                {
                    ["hello"] = "world",
                    ["path"] = "-1,555",
                    [UmbracoContentIndexer.VariesByCultureFieldName] = 1,
                    [UmbracoExamineIndexer.PublishedFieldName] = 1
                }));
            Assert.AreEqual(ValueSetValidationResult.Valid, result);

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
            Assert.AreEqual(ValueSetValidationResult.Filtered, result);

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
            var validator = new ContentValueSetValidator(false, false, publicAccessService.Object);

            var result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
            Assert.AreEqual(ValueSetValidationResult.Filtered, result);

            result = validator.Validate(ValueSet.FromObject("777", IndexTypes.Content, new { hello = "world", path = "-1,777" }));
            Assert.AreEqual(ValueSetValidationResult.Valid, result);
        }
    }
}
