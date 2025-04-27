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
public class BlockListValueRequiredValidatorTests
{
    [Test]
    public void Validates_Empty_Block_List_As_Not_Provided()
    {
        var validator = new BlockListValueRequiredValidator(new SystemTextJsonSerializer());

        var value = JsonNode.Parse("{ \"contentData\": [], \"settingsData\": [] }");
        var result = validator.ValidateRequired(value, ValueTypes.Json);
        Assert.AreEqual(1, result.Count());
    }

    [Test]
    public void Validates_Populated_Block_List_As_Provided()
    {
        var validator = new BlockListValueRequiredValidator(new SystemTextJsonSerializer());

        var value = JsonNode.Parse("{ \"contentData\": [ {} ], \"settingsData\": [] }");
        var result = validator.ValidateRequired(value, ValueTypes.Json);
        Assert.IsEmpty(result);
    }
}
