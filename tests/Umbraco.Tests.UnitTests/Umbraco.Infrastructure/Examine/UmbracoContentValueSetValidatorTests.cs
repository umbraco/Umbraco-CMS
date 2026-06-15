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
            Mock.Of<IScopeProvider>(),
            null,
            null,
            null);

        var result =
            validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,555" }));
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Valid));

        result = validator.Validate(
            ValueSet.FromObject("777", IndexTypes.Media, new { hello = "world", path = "-1,555" }));
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Valid));

        result = validator.Validate(ValueSet.FromObject(
            "555",
            "invalid-category",
            new { hello = "world", path = "-1,555" }));
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Failed));
    }

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
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Failed));

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            new { hello = "world", path = "-1,555" }));
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Valid));
    }

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
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Filtered));

        result = validator.Validate(ValueSet.FromObject("555", IndexTypes.Content, new { hello = "world", path = "-1,444" }));
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Filtered));

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            new { hello = "world", path = "-1,555,777" }));
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Valid));

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            new { hello = "world", path = "-1,555,777,999" }));
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Valid));
    }

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
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Valid));

        Assert.That(result.ValueSet.Values.ContainsKey("path"), Is.False);
        Assert.That(result.ValueSet.Values.ContainsKey("hello"), Is.True);
        Assert.That(result.ValueSet.Values.ContainsKey("world"), Is.True);
    }

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
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Valid));

        Assert.That(result.ValueSet.Values.ContainsKey("path"), Is.True);
        Assert.That(result.ValueSet.Values.ContainsKey("hello"), Is.False);
        Assert.That(result.ValueSet.Values.ContainsKey("world"), Is.False);
    }

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
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Valid));

        Assert.That(result.ValueSet.Values.ContainsKey("path"), Is.False);
        Assert.That(result.ValueSet.Values.ContainsKey("hello"), Is.True);
        Assert.That(result.ValueSet.Values.ContainsKey("world"), Is.False);
    }

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
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Failed));

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            new { hello = "world", path = "-1,555" }));
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Failed));

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            "include-content",
            new { hello = "world", path = "-1,555" }));
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Valid));
    }

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
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Valid));

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            new { hello = "world", path = "-1,555" }));
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Valid));

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            "exclude-content",
            new { hello = "world", path = "-1,555" }));
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Failed));
    }

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
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Failed));

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            new { hello = "world", path = "-1,555" }));
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Failed));

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            "exclude-content",
            new { hello = "world", path = "-1,555" }));
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Failed));

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            "include-content",
            new { hello = "world", path = "-1,555" }));
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Valid));
    }

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
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Failed));

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            new { hello = "world", path = "-1,-20,555,777" }));
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Failed));

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Content,
            new { hello = "world", path = "-1,555" }));
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Failed));

        result = validator.Validate(new ValueSet(
            "555",
            IndexTypes.Content,
            new Dictionary<string, object>
            {
                ["hello"] = "world",
                ["path"] = "-1,555",
                [UmbracoExamineFieldNames.PublishedFieldName] = "y",
            }));
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Valid));
    }

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
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Filtered));

        result = validator.Validate(ValueSet.FromObject(
            "555",
            IndexTypes.Media,
            new { hello = "world", path = "-1,-21,555,777" }));
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Filtered));

        result = validator.Validate(
            ValueSet.FromObject("555", IndexTypes.Media, new { hello = "world", path = "-1,555" }));
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Valid));
    }

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
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Failed));

        result = validator.Validate(new ValueSet(
            "555",
            IndexTypes.Content,
            new Dictionary<string, object>
            {
                ["hello"] = "world",
                ["path"] = "-1,555",
                [UmbracoExamineFieldNames.PublishedFieldName] = "n",
            }));
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Failed));

        result = validator.Validate(new ValueSet(
            "555",
            IndexTypes.Content,
            new Dictionary<string, object>
            {
                ["hello"] = "world",
                ["path"] = "-1,555",
                [UmbracoExamineFieldNames.PublishedFieldName] = "y",
            }));
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Valid));
    }

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
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Failed));

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
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Valid));

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
        Assert.That(valueSet.Values.Count(), Is.EqualTo(10));
        Assert.That(valueSet.Values.ContainsKey($"{UmbracoExamineFieldNames.PublishedFieldName}_es-es"), Is.True);
        Assert.That(valueSet.Values.ContainsKey("hello_es-ES"), Is.True);
        Assert.That(valueSet.Values.ContainsKey("title_es-ES"), Is.True);

        result = validator.Validate(valueSet);

        // Note - Result is still valid, excluded is not the same as filtered.
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Valid));

        Assert.That(result.ValueSet.Values.Count(), Is.EqualTo(7)); // filtered to 7 values (removes es-es values)
        Assert.That(result.ValueSet.Values.ContainsKey($"{UmbracoExamineFieldNames.PublishedFieldName}_es-es"), Is.False);
        Assert.That(result.ValueSet.Values.ContainsKey("hello_es-ES"), Is.False);
        Assert.That(result.ValueSet.Values.ContainsKey("title_es-ES"), Is.False);
    }

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
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Filtered));

        result = validator.Validate(ValueSet.FromObject(
            "777",
            IndexTypes.Content,
            new { hello = "world", path = "-1,777" }));
        Assert.That(result.Status, Is.EqualTo(ValueSetValidationStatus.Valid));
    }
}
