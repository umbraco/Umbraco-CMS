// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

/// <summary>
/// Contains unit tests that verify the behavior of the <see cref="EnsureUniqueValuesValidator"/> class.
/// </summary>
[TestFixture]
public class EnsureUniqueValuesValidatorTest
{
    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer()
        => new SystemTextConfigurationEditorJsonSerializer(new DefaultJsonSerializerEncoderFactory());

    /// <summary>
    /// Tests that the validator expects an array of strings rather than a single string.
    /// </summary>
    [Test]
    public void Expects_Array_Of_String_Not_Single_String()
    {
        var validator = new ValueListUniqueValueValidator(ConfigurationEditorJsonSerializer());
        var result = validator.Validate(
            "hello",
            null,
            null,
            PropertyValidationContext.Empty());
        Assert.AreEqual(1, result.Count());
    }

    /// <summary>
    /// Verifies that the <see cref="ValueListUniqueValueValidator"/> expects an array of strings as input
    /// and validates that all values are unique without errors.
    /// </summary>
    [Test]
    public void Expects_Array_Of_String()
    {
        var validator = new ValueListUniqueValueValidator(ConfigurationEditorJsonSerializer());
        var result =
            validator.Validate(
                new JsonArray("hello", "world"),
                null,
                null,
                PropertyValidationContext.Empty());
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that the validator allows unique values without errors.
    /// </summary>
    [Test]
    public void Allows_Unique_Values()
    {
        var validator = new ValueListUniqueValueValidator(ConfigurationEditorJsonSerializer());
        var result =
            validator.Validate(
                new JsonArray("one", "two", "three"),
                null,
                null,
                PropertyValidationContext.Empty());
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that the validator does not allow multiple identical values.
    /// </summary>
    [Test]
    public void Does_Not_Allow_Multiple_Values()
    {
        var validator = new ValueListUniqueValueValidator(ConfigurationEditorJsonSerializer());
        var result =
            validator.Validate(
                new JsonArray("one", "one"),
                null,
                null,
                PropertyValidationContext.Empty());
        Assert.AreEqual(1, result.Count());
    }

    /// <summary>
    /// Tests that the validator correctly identifies multiple duplicate values in a list.
    /// </summary>
    [Test]
    public void Validates_Multiple_Duplicate_Values()
    {
        var validator = new ValueListUniqueValueValidator(ConfigurationEditorJsonSerializer());
        var result =
            validator.Validate(
                new JsonArray("one", "two", "three", "one", "two"),
                null,
                null,
                PropertyValidationContext.Empty());
        Assert.AreEqual(2, result.Count());
    }

    /// <summary>
    /// Tests that the validator correctly handles a null input value.
    /// </summary>
    [Test]
    public void Handles_Null()
    {
        var validator = new ValueListUniqueValueValidator(ConfigurationEditorJsonSerializer());
        var result =
            validator.Validate(
                null,
                null,
                null,
                PropertyValidationContext.Empty());
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that the validator correctly handles an IEnumerable of string values.
    /// </summary>
    [Test]
    public void Handles_IEnumerable_Of_String()
    {
        var validator = new ValueListUniqueValueValidator(ConfigurationEditorJsonSerializer());
        IEnumerable<string> value = new[] { "one", "two", "three" };
        var result =
            validator.Validate(
                value,
                null,
                null,
                PropertyValidationContext.Empty());
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that the validator correctly handles an array of strings and validates uniqueness.
    /// </summary>
    [Test]
    public void Handles_Array_Of_String()
    {
        var validator = new ValueListUniqueValueValidator(ConfigurationEditorJsonSerializer());
        string[] value = { "one", "two", "three" };
        var result =
            validator.Validate(
                value,
                null,
                null,
                PropertyValidationContext.Empty());
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    /// Tests that the ValueListUniqueValueValidator correctly handles a list of strings and validates uniqueness.
    /// </summary>
    [Test]
    public void Handles_List_Of_String()
    {
        var validator = new ValueListUniqueValueValidator(ConfigurationEditorJsonSerializer());
        var value = new List<string> { "one", "two", "three" };
        var result =
            validator.Validate(
                value,
                null,
                null,
                PropertyValidationContext.Empty());
        Assert.AreEqual(0, result.Count());
    }
}
