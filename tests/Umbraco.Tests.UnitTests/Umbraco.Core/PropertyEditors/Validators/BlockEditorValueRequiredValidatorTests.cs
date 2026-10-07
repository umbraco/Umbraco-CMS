// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Infrastructure.PropertyEditors.Validators;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class BlockEditorValueRequiredValidatorTests
{
    [Test]
    public void Validates_Missing_Block_Value_As_Not_Provided()
    {
        var result = Validate<BlockListValue>(null);

        AssertValidationFailed(result, expectedMessage: Constants.Validation.ErrorMessages.Properties.Missing);
    }

    [TestCase("{ \"contentData\": [], \"settingsData\": [] }")]
    [TestCase("{ \"layout\": { \"Umbraco.BlockList\": [] }, \"contentData\": [], \"settingsData\": [] }")]
    public void Validates_Empty_Block_List_As_Not_Provided(string value)
    {
        var result = Validate<BlockListValue>(value);

        AssertValidationFailed(result);
    }

    [Test]
    public void Validates_Populated_Block_List_As_Provided()
    {
        var result = Validate<BlockListValue>("{ \"contentData\": [ {} ], \"settingsData\": [] }");

        Assert.IsEmpty(result);
    }

    [TestCase("{ \"contentData\": [], \"settingsData\": [] }")]
    [TestCase("{ \"layout\": { \"Umbraco.BlockGrid\": [] }, \"contentData\": [], \"settingsData\": [] }")]
    public void Validates_Empty_Block_Grid_As_Not_Provided(string value)
    {
        var result = Validate<BlockGridValue>(value);

        AssertValidationFailed(result);
    }

    [Test]
    public void Validates_Populated_Block_Grid_As_Provided()
    {
        var result = Validate<BlockGridValue>("{ \"contentData\": [ {} ], \"settingsData\": [] }");

        Assert.IsEmpty(result);
    }

    [TestCase("{ \"contentData\": [], \"settingsData\": [] }")]
    [TestCase("{ \"layout\": { \"Umbraco.SingleBlock\": [] }, \"contentData\": [], \"settingsData\": [] }")]
    public void Validates_Empty_Single_Block_As_Not_Provided(string value)
    {
        var result = Validate<SingleBlockValue>(value);

        AssertValidationFailed(result);
    }

    [Test]
    public void Validates_Populated_Single_Block_As_Provided()
    {
        var result = Validate<SingleBlockValue>("{ \"contentData\": [ {} ], \"settingsData\": [] }");

        Assert.IsEmpty(result);
    }

    private static IEnumerable<ValidationResult> Validate<TValue>(string? value)
        where TValue : BlockValue
    {
        var validator = new BlockEditorValueRequiredValidator<TValue>(new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()));

        return validator.ValidateRequired(value is null ? null : JsonNode.Parse(value), ValueTypes.Json);
    }

    private static void AssertValidationFailed(IEnumerable<ValidationResult> result, string expectedMessage = Constants.Validation.ErrorMessages.Properties.Empty)
    {
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(expectedMessage, result.First().ErrorMessage);
    }
}
