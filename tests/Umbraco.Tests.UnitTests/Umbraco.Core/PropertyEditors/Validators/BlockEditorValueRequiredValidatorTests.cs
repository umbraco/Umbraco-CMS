// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Infrastructure.PropertyEditors.Validators;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class BlockEditorValueRequiredValidatorTests
{
    [TestCase("{ \"contentData\": [], \"settingsData\": [] }")]
    [TestCase("{ \"layout\": { \"Umbraco.BlockList\": [] }, \"contentData\": [], \"settingsData\": [] }")]
    public void Validates_Empty_Block_List_As_Not_Provided(string value)
    {
        var result = Validate<BlockListValue>(value);

        Assert.AreEqual(1, result.Count());
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

        Assert.AreEqual(1, result.Count());
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

        Assert.AreEqual(1, result.Count());
    }

    [Test]
    public void Validates_Populated_Single_Block_As_Provided()
    {
        var result = Validate<SingleBlockValue>("{ \"contentData\": [ {} ], \"settingsData\": [] }");

        Assert.IsEmpty(result);
    }

    private static IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> Validate<TValue>(string value)
        where TValue : BlockValue
    {
        var validator = new BlockEditorValueRequiredValidator<TValue>(new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()));

        return validator.ValidateRequired(JsonNode.Parse(value), ValueTypes.Json);
    }
}
