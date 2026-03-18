// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Infrastructure.PropertyEditors.Validators;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

/// <summary>
/// Contains unit tests for the <see cref="BlockListValueRequiredValidator"/> class, verifying its validation logic and behavior.
/// </summary>
[TestFixture]
public class BlockListValueRequiredValidatorTests
{
    /// <summary>
    /// Validates that an empty block list is considered as not provided.
    /// </summary>
    [Test]
    public void Validates_Empty_Block_List_As_Not_Provided()
    {
        var validator = new BlockListValueRequiredValidator(new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()));

        var value = JsonNode.Parse("{ \"contentData\": [], \"settingsData\": [] }");
        var result = validator.ValidateRequired(value, ValueTypes.Json);
        Assert.AreEqual(1, result.Count());
    }

    /// <summary>
    /// Tests that the <see cref="BlockListValueRequiredValidator"/> correctly accepts a block list value that contains content data as valid.
    /// Ensures that no validation errors are returned for a populated block list.
    /// </summary>
    [Test]
    public void Validates_Populated_Block_List_As_Provided()
    {
        var validator = new BlockListValueRequiredValidator(new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()));

        var value = JsonNode.Parse("{ \"contentData\": [ {} ], \"settingsData\": [] }");
        var result = validator.ValidateRequired(value, ValueTypes.Json);
        Assert.IsEmpty(result);
    }
}
