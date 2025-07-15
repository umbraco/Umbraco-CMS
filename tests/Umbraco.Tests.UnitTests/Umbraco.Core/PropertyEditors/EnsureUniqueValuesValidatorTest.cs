// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class EnsureUniqueValuesValidatorTest
{
    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer()
        => new SystemTextConfigurationEditorJsonSerializer();

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
