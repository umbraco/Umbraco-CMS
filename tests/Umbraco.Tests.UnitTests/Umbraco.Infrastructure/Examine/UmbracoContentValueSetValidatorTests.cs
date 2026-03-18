// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Examine;

/// <summary>
/// Tests for the <see cref="UmbracoContentValueSetValidator"/> class.
/// </summary>
[TestFixture]
public class UmbracoContentValueSetValidatorTests
{
    /// <summary>
    /// Verifies that the <see cref="ContentValueSetValidator"/> correctly fails validation
    /// when a value set with an invalid category is provided, and passes for valid categories
    /// such as <c>Content</c> and <c>Media</c>.
    /// </summary>
    [Test]
    public void Invalid_Category()
    {
        var validator = new ContentValueSetValidator(
            false,
            true,
            Mock.Of<IPublicAccessService>(),
            Mock.Of<IScopeProvider>(),
            null,
            null,
            null);

        var result =
            validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(ValueSetValidationStatus.Valid, result.Status);

        result = validator.Validate(
            ValueSet.FromObject("777", IndexTypes.Media, new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(ValueSetValidationStatus.Valid, result.Status);

        result = validator.Validate(ValueSet.FromObject(
            "555",
            "invalid-category",
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(ValueSetValidationStatus.Failed, result.Status);
    }

    /// <summary>
    /// Tests that the ContentValueSetValidator requires a valid path to consider the value set valid.
    /// </summary>
    [Test]
    public void Must_Have_Path()
    {
        var validator = new ContentValueSetValidator(
            false,
            true,
            Mock.Of<IPublicAccessService>(),
            Mock.Of<IScopeProvider>(),
            null,
            null,
            null);

        var result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, new { hello = "world" }));
        Assert.AreEqual(ValueSetValidationStatus.Failed, result.Status);

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(ValueSetValidationStatus.Valid, result.Status);
    }

    /// <summary>
    /// Verifies that <see cref="ContentValueSetValidator"/> correctly validates content items based on their parent ID path.
    /// Tests that value sets with a path ending at the specified parent ID are filtered, while those with additional descendants are considered valid.
    /// This ensures the validator's logic for handling parent-child relationships in content indexing is correct.
    /// </summary>
    [Test]
    public void Parent_Id()
    {
        var validator = new ContentValueSetValidator(
            false,
            true,
            Mock.Of<IPublicAccessService>(),
            Mock.Of<IScopeProvider>(),
            555,
            null,
            null);

        var result =
            validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(ValueSetValidationStatus.Filtered, result.Status);

        result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,444" }));
        Assert.AreEqual(ValueSetValidationStatus.Filtered, result.Status);

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            new { hello = "world", path = "-1,555,777" }));
        Assert.AreEqual(ValueSetValidationStatus.Valid, result.Status);

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            new { hello = "world", path = "-1,555,777,999" }));
        Assert.AreEqual(ValueSetValidationStatus.Valid, result.Status);
    }

    /// <summary>
    /// Tests that the ValueSetValidator correctly includes only the specified fields in the validated value set.
    /// </summary>
    [Test]
    public void Inclusion_Field_List()
    {
        var validator = new ValueSetValidator(
            null,
            null,
            new[] { "hello", "world" },
            null);

        var valueSet = ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            "test-content",
            new { hello = "world", path = "-1,555", world = "your oyster" });
        var result = validator.Validate(valueSet);

        // Note - Result is still valid, excluded is not the same as filtered.
        Assert.AreEqual(ValueSetValidationStatus.Valid, result.Status);

        Assert.IsFalse(result.ValueSet.Values.ContainsKey("path"));
        Assert.IsTrue(result.ValueSet.Values.ContainsKey("hello"));
        Assert.IsTrue(result.ValueSet.Values.ContainsKey("world"));
    }

    /// <summary>
    /// Tests that fields specified in the exclusion list are removed from the value set during validation.
    /// </summary>
    [Test]
    public void Exclusion_Field_List()
    {
        var validator = new ValueSetValidator(
            null,
            null,
            null,
            new[] { "hello", "world" });

        var valueSet = ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            "test-content",
            new { hello = "world", path = "-1,555", world = "your oyster" });
        var result = validator.Validate(valueSet);

        // Note - Result is still valid, excluded is not the same as filtered.
        Assert.AreEqual(ValueSetValidationStatus.Valid, result.Status);

        Assert.IsTrue(result.ValueSet.Values.ContainsKey("path"));
        Assert.IsFalse(result.ValueSet.Values.ContainsKey("hello"));
        Assert.IsFalse(result.ValueSet.Values.ContainsKey("world"));
    }

    /// <summary>
    /// Tests the inclusion and exclusion of fields in the ValueSetValidator.
    /// Ensures that included fields are present and excluded fields are removed from the validated ValueSet.
    /// </summary>
    [Test]
    public void Inclusion_Exclusion_Field_List()
    {
        var validator = new ValueSetValidator(
            null,
            null,
            new[] { "hello", "world" },
            new[] { "world" });

        var valueSet = ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            "test-content",
            new { hello = "world", path = "-1,555", world = "your oyster" });
        var result = validator.Validate(valueSet);

        // Note - Result is still valid, excluded is not the same as filtered.
        Assert.AreEqual(ValueSetValidationStatus.Valid, result.Status);

        Assert.IsFalse(result.ValueSet.Values.ContainsKey("path"));
        Assert.IsTrue(result.ValueSet.Values.ContainsKey("hello"));
        Assert.IsFalse(result.ValueSet.Values.ContainsKey("world"));
    }

    /// <summary>
    /// Tests that the <see cref="ContentValueSetValidator"/> correctly validates content based on an inclusion type list.
    /// Ensures that only content with a type present in the inclusion list is considered valid, and others are rejected.
    /// </summary>
    [Test]
    public void Inclusion_Type_List()
    {
        var validator = new ContentValueSetValidator(
            false,
            true,
            Mock.Of<IPublicAccessService>(),
            Mock.Of<IScopeProvider>(),
            null,
            new List<string> { "include-content" },
            null);

        var result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            "test-content",
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(ValueSetValidationStatus.Failed, result.Status);

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(ValueSetValidationStatus.Failed, result.Status);

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            "include-content",
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(ValueSetValidationStatus.Valid, result.Status);
    }

    /// <summary>
    /// Tests the exclusion type list functionality of the ContentValueSetValidator.
    /// Validates that value sets with types in the exclusion list are marked as failed,
    /// while others are valid.
    /// </summary>
    [Test]
    public void Exclusion_Type_List()
    {
        var validator = new ContentValueSetValidator(
            false,
            true,
            Mock.Of<IPublicAccessService>(),
            Mock.Of<IScopeProvider>(),
            null,
            null,
            new List<string> { "exclude-content" });

        var result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            "test-content",
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(ValueSetValidationStatus.Valid, result.Status);

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(ValueSetValidationStatus.Valid, result.Status);

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            "exclude-content",
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(ValueSetValidationStatus.Failed, result.Status);
    }

    /// <summary>
    /// Tests the inclusion and exclusion logic of the ContentValueSetValidator
    /// using a list of types to include and exclude.
    /// </summary>
    [Test]
    public void Inclusion_Exclusion_Type_List()
    {
        var validator = new ContentValueSetValidator(
            false,
            true,
            Mock.Of<IPublicAccessService>(),
            Mock.Of<IScopeProvider>(),
            null,
            new List<string> { "include-content", "exclude-content" },
            new List<string> { "exclude-content" });

        var result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            "test-content",
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(ValueSetValidationStatus.Failed, result.Status);

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(ValueSetValidationStatus.Failed, result.Status);

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            "exclude-content",
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(ValueSetValidationStatus.Failed, result.Status);

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            "include-content",
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(ValueSetValidationStatus.Valid, result.Status);
    }

    /// <summary>
    /// Tests validation of content items located in the recycle bin.
    /// </summary>
    [Test]
    public void Recycle_Bin_Content()
    {
        var validator = new ContentValueSetValidator(
            true,
            false,
            Mock.Of<IPublicAccessService>(),
            Mock.Of<IScopeProvider>(),
            null,
            null,
            null);

        var result =
            validator.Validate(ValueSet.FromObject(
                "555",
                IndexTypes.Content,
                new { hello = "world", path = "-1,-20,555" }));
        Assert.AreEqual(ValueSetValidationStatus.Failed, result.Status);

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            new { hello = "world", path = "-1,-20,555,777" }));
        Assert.AreEqual(ValueSetValidationStatus.Failed, result.Status);

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(ValueSetValidationStatus.Failed, result.Status);

        result = validator.Validate(new ValueSet(
            "555",
            IndexTypes.Content,
            new Dictionary<string, object>
            {
                ["hello"] = "world",
                ["path"] = "-1,555",
                [UmbracoExamineFieldNames.PublishedFieldName] = "y",
            }));
        Assert.AreEqual(ValueSetValidationStatus.Valid, result.Status);
    }

    /// <summary>
    /// Verifies that media items located in the recycle bin are correctly filtered by the <see cref="ContentValueSetValidator"/>,
    /// and that only media items not in the recycle bin are considered valid.
    /// </summary>
    [Test]
    public void Recycle_Bin_Media()
    {
        var validator = new ContentValueSetValidator(
            true,
            false,
            Mock.Of<IPublicAccessService>(),
            Mock.Of<IScopeProvider>(),
            null,
            null,
            null);

        var result =
            validator.Validate(ValueSet.FromObject(
                "555",
                IndexTypes.Media,
                new { hello = "world", path = "-1,-21,555" }));
        Assert.AreEqual(ValueSetValidationStatus.Filtered, result.Status);

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Media,
            new { hello = "world", path = "-1,-21,555,777" }));
        Assert.AreEqual(ValueSetValidationStatus.Filtered, result.Status);

        result = validator.Validate(
            ValueSet.FromObject("555", IndexTypes.Media, new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(ValueSetValidationStatus.Valid, result.Status);
    }

    /// <summary>
    /// Verifies that the <see cref="ContentValueSetValidator"/> only considers content value sets as valid if they are marked as published,
    /// and rejects those that are unpublished.
    /// </summary>
    [Test]
    public void Published_Only()
    {
        var validator = new ContentValueSetValidator(
            true,
            true,
            Mock.Of<IPublicAccessService>(),
            Mock.Of<IScopeProvider>(),
            null,
            null,
            null);

        var result =
            validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(ValueSetValidationStatus.Failed, result.Status);

        result = validator.Validate(new ValueSet(
            "555",
            IndexTypes.Content,
            new Dictionary<string, object>
            {
                ["hello"] = "world",
                ["path"] = "-1,555",
                [UmbracoExamineFieldNames.PublishedFieldName] = "n",
            }));
        Assert.AreEqual(ValueSetValidationStatus.Failed, result.Status);

        result = validator.Validate(new ValueSet(
            "555",
            IndexTypes.Content,
            new Dictionary<string, object>
            {
                ["hello"] = "world",
                ["path"] = "-1,555",
                [UmbracoExamineFieldNames.PublishedFieldName] = "y",
            }));
        Assert.AreEqual(ValueSetValidationStatus.Valid, result.Status);
    }

    /// <summary>
    /// Tests that the <see cref="ContentValueSetValidator"/> correctly validates content with culture variants.
    /// Ensures that only published variants are included in the validated <see cref="ValueSet"/>,
    /// and that any unpublished variants (e.g., specific cultures marked as unpublished) are excluded from the result.
    /// Also verifies that the overall validation status is correct depending on the published state of the variants.
    /// </summary>
    [Test]
    public void Published_Only_With_Variants()
    {
        var validator = new ContentValueSetValidator(
            true,
            true,
            Mock.Of<IPublicAccessService>(),
            Mock.Of<IScopeProvider>(),
            null,
            null,
            null);

        var result = validator.Validate(new ValueSet(
            "555",
            IndexTypes.Content,
            new Dictionary<string, object>
            {
                ["hello"] = "world",
                ["path"] = "-1,555",
                [UmbracoExamineFieldNames.VariesByCultureFieldName] = "y",
                [UmbracoExamineFieldNames.PublishedFieldName] = "n",
            }));
        Assert.AreEqual(ValueSetValidationStatus.Failed, result.Status);

        result = validator.Validate(new ValueSet(
            "555",
            IndexTypes.Content,
            new Dictionary<string, object>
            {
                ["hello"] = "world",
                ["path"] = "-1,555",
                [UmbracoExamineFieldNames.VariesByCultureFieldName] = "y",
                [UmbracoExamineFieldNames.PublishedFieldName] = "y",
            }));
        Assert.AreEqual(ValueSetValidationStatus.Valid, result.Status);

        var valueSet = new ValueSet(
            "555",
            IndexTypes.Content,
            new Dictionary<string, object>
            {
                ["hello"] = "world",
                ["path"] = "-1,555",
                [UmbracoExamineFieldNames.VariesByCultureFieldName] = "y",
                [$"{UmbracoExamineFieldNames.PublishedFieldName}_en-us"] = "y",
                ["hello_en-us"] = "world",
                ["title_en-us"] = "my title",
                [$"{UmbracoExamineFieldNames.PublishedFieldName}_es-es"] = "n",
                ["hello_es-ES"] = "world",
                ["title_es-ES"] = "my title",
                [UmbracoExamineFieldNames.PublishedFieldName] = "y",
            });
        Assert.AreEqual(10, valueSet.Values.Count());
        Assert.IsTrue(valueSet.Values.ContainsKey($"{UmbracoExamineFieldNames.PublishedFieldName}_es-es"));
        Assert.IsTrue(valueSet.Values.ContainsKey("hello_es-ES"));
        Assert.IsTrue(valueSet.Values.ContainsKey("title_es-ES"));

        result = validator.Validate(valueSet);

        // Note - Result is still valid, excluded is not the same as filtered.
        Assert.AreEqual(ValueSetValidationStatus.Valid, result.Status);

        Assert.AreEqual(7, result.ValueSet.Values.Count()); // filtered to 7 values (removes es-es values)
        Assert.IsFalse(result.ValueSet.Values.ContainsKey($"{UmbracoExamineFieldNames.PublishedFieldName}_es-es"));
        Assert.IsFalse(result.ValueSet.Values.ContainsKey("hello_es-ES"));
        Assert.IsFalse(result.ValueSet.Values.ContainsKey("title_es-ES"));
    }

    /// <summary>
    /// Verifies that the <see cref="ContentValueSetValidator"/> correctly filters or validates content value sets
    /// based on whether the content is protected or not, using mocked public access service responses.
    /// Specifically, ensures protected content is filtered and non-protected content is valid.
    /// </summary>
    [Test]
    public void Non_Protected()
    {
        var publicAccessService = new Mock<IPublicAccessService>();
        publicAccessService.Setup(x => x.IsProtected("-1,555"))
            .Returns(Attempt.Succeed(new PublicAccessEntry(Guid.NewGuid(), 555, 444, 333, Enumerable.Empty<PublicAccessRule>())));
        publicAccessService.Setup(x => x.IsProtected("-1,777"))
            .Returns(Attempt.Fail<PublicAccessEntry>());
        var validator = new ContentValueSetValidator(
            false,
            false,
            publicAccessService.Object,
            Mock.Of<IScopeProvider>(),
            null,
            null,
            null);

        var result =
            validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
        Assert.AreEqual(ValueSetValidationStatus.Filtered, result.Status);

        result = validator.Validate(ValueSet.FromObject(
            "777",
            IndexTypes.Content,
            new { hello = "world", path = "-1,777" }));
        Assert.AreEqual(ValueSetValidationStatus.Valid, result.Status);
    }
}
