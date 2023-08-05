// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Search.ValueSet;
using Umbraco.Search.ValueSet.Validators;
using Umbraco.Search.ValueSet.ValueSetBuilders;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Examine;

[TestFixture]
public class UmbracoContentValueSetValidatorTests
{
    [Test]
    public void Invalid_Category()
    {
        var validator = new ContentValueSetValidator(
            false,
            true,
            Mock.Of<IPublicAccessService>(),
            Mock.Of<IScopeProvider>());

        var result =
            validator.Validate(UmbracoValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Valid, result.Status);

        result = validator.Validate(
            UmbracoValueSet.FromObject("777", IndexTypes.Media, new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Valid, result.Status);

        result = validator.Validate(UmbracoValueSet.FromObject(
            "555",
            "invalid-category",
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Failed, result.Status);
    }

    [Test]
    public void Must_Have_Path()
    {
        var validator = new ContentValueSetValidator(
            false,
            true,
            Mock.Of<IPublicAccessService>(),
            Mock.Of<IScopeProvider>());

        var result = validator.Validate(UmbracoValueSet.FromObject("555", IndexTypes.Content, new { hello = "world" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Failed, result.Status);

        result = validator.Validate(UmbracoValueSet.FromObject(
            "555",
            IndexTypes.Content,
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Valid, result.Status);
    }

    [Test]
    public void Parent_Id()
    {
        var validator = new ContentValueSetValidator(
            false,
            true,
            Mock.Of<IPublicAccessService>(),
            Mock.Of<IScopeProvider>(),
            555);

        var result =
            validator.Validate(UmbracoValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Filtered, result.Status);

        result = validator.Validate(UmbracoValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,444" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Filtered, result.Status);

        result = validator.Validate(UmbracoValueSet.FromObject(
            "555",
            IndexTypes.Content,
            new { hello = "world", path = "-1,555,777" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Valid, result.Status);

        result = validator.Validate(UmbracoValueSet.FromObject(
            "555",
            IndexTypes.Content,
            new { hello = "world", path = "-1,555,777,999" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Valid, result.Status);
    }

    [Test]
    public void Inclusion_Field_List()
    {
        var validator = new UmbracoValueSetValidator(
            null,
            null,
            new[] { "hello", "world" },
            null);

        var UmbracoValueSet = Search.ValueSet.UmbracoValueSet.FromObject(
            "555",
            IndexTypes.Content,
            "test-content",
            new { hello = "world", path = "-1,555", world = "your oyster" });
        var result = validator.Validate(UmbracoValueSet);

        // Note - Result is still valid, excluded is not the same as filtered.
        Assert.AreEqual(UmbracoValueSetValidationStatus.Valid, result.Status);

        Assert.IsFalse(result.ValueSet.Values.ContainsKey("path"));
        Assert.IsTrue(result.ValueSet.Values.ContainsKey("hello"));
        Assert.IsTrue(result.ValueSet.Values.ContainsKey("world"));
    }

    [Test]
    public void Exclusion_Field_List()
    {
        var validator = new UmbracoValueSetValidator(
            null,
            null,
            null,
            new[] { "hello", "world" });

        var UmbracoValueSet = Search.ValueSet.UmbracoValueSet.FromObject(
            "555",
            IndexTypes.Content,
            "test-content",
            new { hello = "world", path = "-1,555", world = "your oyster" });
        var result = validator.Validate(UmbracoValueSet);

        // Note - Result is still valid, excluded is not the same as filtered.
        Assert.AreEqual(UmbracoValueSetValidationStatus.Valid, result.Status);

        Assert.IsTrue(result.ValueSet.Values.ContainsKey("path"));
        Assert.IsFalse(result.ValueSet.Values.ContainsKey("hello"));
        Assert.IsFalse(result.ValueSet.Values.ContainsKey("world"));
    }

    [Test]
    public void Inclusion_Exclusion_Field_List()
    {
        var validator = new UmbracoValueSetValidator(
            null,
            null,
            new[] { "hello", "world" },
            new[] { "world" });

        var UmbracoValueSet = Search.ValueSet.UmbracoValueSet.FromObject(
            "555",
            IndexTypes.Content,
            "test-content",
            new { hello = "world", path = "-1,555", world = "your oyster" });
        var result = validator.Validate(UmbracoValueSet);

        // Note - Result is still valid, excluded is not the same as filtered.
        Assert.AreEqual(UmbracoValueSetValidationStatus.Valid, result.Status);

        Assert.IsFalse(result.ValueSet.Values.ContainsKey("path"));
        Assert.IsTrue(result.ValueSet.Values.ContainsKey("hello"));
        Assert.IsFalse(result.ValueSet.Values.ContainsKey("world"));
    }

    [Test]
    public void Inclusion_Type_List()
    {
        var validator = new ContentValueSetValidator(
            false,
            true,
            Mock.Of<IPublicAccessService>(),
            Mock.Of<IScopeProvider>(),
            includeItemTypes: new List<string> { "include-content" });

        var result = validator.Validate(UmbracoValueSet.FromObject(
            "555",
            IndexTypes.Content,
            "test-content",
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Failed, result.Status);

        result = validator.Validate(UmbracoValueSet.FromObject(
            "555",
            IndexTypes.Content,
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Failed, result.Status);

        result = validator.Validate(UmbracoValueSet.FromObject(
            "555",
            IndexTypes.Content,
            "include-content",
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Valid, result.Status);
    }

    [Test]
    public void Exclusion_Type_List()
    {
        var validator = new ContentValueSetValidator(
            false,
            true,
            Mock.Of<IPublicAccessService>(),
            Mock.Of<IScopeProvider>(),
            excludeItemTypes: new List<string> { "exclude-content" });

        var result = validator.Validate(UmbracoValueSet.FromObject(
            "555",
            IndexTypes.Content,
            "test-content",
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Valid, result.Status);

        result = validator.Validate(UmbracoValueSet.FromObject(
            "555",
            IndexTypes.Content,
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Valid, result.Status);

        result = validator.Validate(UmbracoValueSet.FromObject(
            "555",
            IndexTypes.Content,
            "exclude-content",
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Failed, result.Status);
    }

    [Test]
    public void Inclusion_Exclusion_Type_List()
    {
        var validator = new ContentValueSetValidator(
            false,
            true,
            Mock.Of<IPublicAccessService>(),
            Mock.Of<IScopeProvider>(),
            includeItemTypes: new List<string> { "include-content", "exclude-content" },
            excludeItemTypes: new List<string> { "exclude-content" });

        var result = validator.Validate(UmbracoValueSet.FromObject(
            "555",
            IndexTypes.Content,
            "test-content",
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Failed, result.Status);

        result = validator.Validate(UmbracoValueSet.FromObject(
            "555",
            IndexTypes.Content,
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Failed, result.Status);

        result = validator.Validate(UmbracoValueSet.FromObject(
            "555",
            IndexTypes.Content,
            "exclude-content",
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Failed, result.Status);

        result = validator.Validate(UmbracoValueSet.FromObject(
            "555",
            IndexTypes.Content,
            "include-content",
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Valid, result.Status);
    }

    [Test]
    public void Recycle_Bin_Content()
    {
        var validator = new ContentValueSetValidator(
            true,
            false,
            Mock.Of<IPublicAccessService>(),
            Mock.Of<IScopeProvider>());

        var result =
            validator.Validate(UmbracoValueSet.FromObject(
                "555",
                IndexTypes.Content,
                new { hello = "world", path = "-1,-20,555" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Failed, result.Status);

        result = validator.Validate(UmbracoValueSet.FromObject(
            "555",
            IndexTypes.Content,
            new { hello = "world", path = "-1,-20,555,777" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Failed, result.Status);

        result = validator.Validate(UmbracoValueSet.FromObject(
            "555",
            IndexTypes.Content,
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Failed, result.Status);

        result = validator.Validate(new UmbracoValueSet(
            "555",
            IndexTypes.Content,
            new Dictionary<string, object>
            {
                ["hello"] = "world",
                ["path"] = "-1,555",
                [UmbracoSearchFieldNames.PublishedFieldName] = "y",
            }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Valid, result.Status);
    }

    [Test]
    public void Recycle_Bin_Media()
    {
        var validator = new ContentValueSetValidator(
            true,
            false,
            Mock.Of<IPublicAccessService>(),
            Mock.Of<IScopeProvider>());

        var result =
            validator.Validate(UmbracoValueSet.FromObject(
                "555",
                IndexTypes.Media,
                new { hello = "world", path = "-1,-21,555" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Filtered, result.Status);

        result = validator.Validate(UmbracoValueSet.FromObject(
            "555",
            IndexTypes.Media,
            new { hello = "world", path = "-1,-21,555,777" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Filtered, result.Status);

        result = validator.Validate(
            UmbracoValueSet.FromObject("555", IndexTypes.Media, new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Valid, result.Status);
    }

    [Test]
    public void Published_Only()
    {
        var validator = new ContentValueSetValidator(
            true,
            true,
            Mock.Of<IPublicAccessService>(),
            Mock.Of<IScopeProvider>());

        var result =
            validator.Validate(UmbracoValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Failed, result.Status);

        result = validator.Validate(new UmbracoValueSet(
            "555",
            IndexTypes.Content,
            new Dictionary<string, object>
            {
                ["hello"] = "world",
                ["path"] = "-1,555",
                [UmbracoSearchFieldNames.PublishedFieldName] = "n",
            }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Failed, result.Status);

        result = validator.Validate(new UmbracoValueSet(
            "555",
            IndexTypes.Content,
            new Dictionary<string, object>
            {
                ["hello"] = "world",
                ["path"] = "-1,555",
                [UmbracoSearchFieldNames.PublishedFieldName] = "y",
            }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Valid, result.Status);
    }

    [Test]
    public void Published_Only_With_Variants()
    {
        var validator = new ContentValueSetValidator(
            true,
            true,
            Mock.Of<IPublicAccessService>(),
            Mock.Of<IScopeProvider>());

        var result = validator.Validate(new UmbracoValueSet(
            "555",
            IndexTypes.Content,
            new Dictionary<string, object>
            {
                ["hello"] = "world",
                ["path"] = "-1,555",
                [UmbracoSearchFieldNames.VariesByCultureFieldName] = "y",
                [UmbracoSearchFieldNames.PublishedFieldName] = "n",
            }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Failed, result.Status);

        result = validator.Validate(new UmbracoValueSet(
            "555",
            IndexTypes.Content,
            new Dictionary<string, object>
            {
                ["hello"] = "world",
                ["path"] = "-1,555",
                [UmbracoSearchFieldNames.VariesByCultureFieldName] = "y",
                [UmbracoSearchFieldNames.PublishedFieldName] = "y",
            }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Valid, result.Status);

        var UmbracoValueSet = new UmbracoValueSet(
            "555",
            IndexTypes.Content,
            new Dictionary<string, object>
            {
                ["hello"] = "world",
                ["path"] = "-1,555",
                [UmbracoSearchFieldNames.VariesByCultureFieldName] = "y",
                [$"{UmbracoSearchFieldNames.PublishedFieldName}_en-us"] = "y",
                ["hello_en-us"] = "world",
                ["title_en-us"] = "my title",
                [$"{UmbracoSearchFieldNames.PublishedFieldName}_es-es"] = "n",
                ["hello_es-ES"] = "world",
                ["title_es-ES"] = "my title",
                [UmbracoSearchFieldNames.PublishedFieldName] = "y",
            });
        Assert.AreEqual(10, UmbracoValueSet.Values.Count());
        Assert.IsTrue(UmbracoValueSet.Values.ContainsKey($"{UmbracoSearchFieldNames.PublishedFieldName}_es-es"));
        Assert.IsTrue(UmbracoValueSet.Values.ContainsKey("hello_es-ES"));
        Assert.IsTrue(UmbracoValueSet.Values.ContainsKey("title_es-ES"));

        result = validator.Validate(UmbracoValueSet);

        // Note - Result is still valid, excluded is not the same as filtered.
        Assert.AreEqual(UmbracoValueSetValidationStatus.Valid, result.Status);

        Assert.AreEqual(7, result.ValueSet.Values.Count()); // filtered to 7 values (removes es-es values)
        Assert.IsFalse(result.ValueSet.Values.ContainsKey($"{UmbracoSearchFieldNames.PublishedFieldName}_es-es"));
        Assert.IsFalse(result.ValueSet.Values.ContainsKey("hello_es-ES"));
        Assert.IsFalse(result.ValueSet.Values.ContainsKey("title_es-ES"));
    }

    [Test]
    public void Non_Protected()
    {
        var publicAccessService = new Mock<IPublicAccessService>();
        publicAccessService.Setup(x => x.IsProtected("-1,555"))
            .Returns(Attempt.Succeed(new PublicAccessEntry(Guid.NewGuid(), 555, 444, 333, Enumerable.Empty<PublicAccessRule>())));
        publicAccessService.Setup(x => x.IsProtected("-1,777"))
            .Returns(Attempt.Fail<PublicAccessEntry?>());
        var validator = new ContentValueSetValidator(
            false,
            false,
            publicAccessService.Object,
            Mock.Of<IScopeProvider>());

        var result =
            validator.Validate(UmbracoValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Filtered, result.Status);

        result = validator.Validate(UmbracoValueSet.FromObject(
            "777",
            IndexTypes.Content,
            new { hello = "world", path = "-1,777" }));
        Assert.AreEqual(UmbracoValueSetValidationStatus.Valid, result.Status);
    }
}
