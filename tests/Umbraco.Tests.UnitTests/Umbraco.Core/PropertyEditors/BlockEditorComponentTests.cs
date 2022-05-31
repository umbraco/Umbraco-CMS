// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class BlockEditorComponentTests
{
    private readonly JsonSerializerSettings _serializerSettings = new()
    {
        Formatting = Formatting.None,
        NullValueHandling = NullValueHandling.Ignore,
    };

    private const string ContentGuid1 = "036ce82586a64dfba2d523a99ed80f58";
    private const string ContentGuid2 = "48288c21a38a40ef82deb3eda90a58f6";
    private const string SettingsGuid1 = "ffd35c4e2eea4900abfa5611b67b2492";
    private const string SubContentGuid1 = "4c44ce6b3a5c4f5f8f15e3dc24819a9e";
    private const string SubContentGuid2 = "a062c06d6b0b44ac892b35d90309c7f8";
    private const string SubSettingsGuid1 = "4d998d980ffa4eee8afdc23c4abd6d29";

    private static readonly ILogger<BlockEditorPropertyHandler> s_logger =
        Mock.Of<ILogger<BlockEditorPropertyHandler>>();

    [Test]
    public void Cannot_Have_Null_Udi()
    {
        var component = new BlockEditorPropertyHandler(s_logger);
        var json = GetBlockListJson(null, string.Empty);
        Assert.Throws<FormatException>(() => component.ReplaceBlockListUdis(json));
    }

    [Test]
    public void No_Nesting()
    {
        var guids = Enumerable.Range(0, 3).Select(x => Guid.NewGuid()).ToList();
        var guidCounter = 0;

        Guid GuidFactory()
        {
            return guids[guidCounter++];
        }

        var json = GetBlockListJson(null);

        var expected = ReplaceGuids(json, guids, ContentGuid1, ContentGuid2, SettingsGuid1);

        var component = new BlockEditorPropertyHandler(s_logger);
        var result = component.ReplaceBlockListUdis(json, GuidFactory);

        var expectedJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(expected, _serializerSettings), _serializerSettings);
        var resultJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(result, _serializerSettings), _serializerSettings);
        Console.WriteLine(expectedJson);
        Console.WriteLine(resultJson);
        Assert.AreEqual(expectedJson, resultJson);
    }

    [Test]
    public void One_Level_Nesting_Escaped()
    {
        var guids = Enumerable.Range(0, 6).Select(x => Guid.NewGuid()).ToList();

        var guidCounter = 0;

        Guid GuidFactory()
        {
            return guids[guidCounter++];
        }

        var innerJson = GetBlockListJson(null, SubContentGuid1, SubContentGuid2, SubSettingsGuid1);

        // we need to ensure the escaped json is consistent with how it will be re-escaped after parsing
        // and this is how to do that, the result will also include quotes around it.
        var innerJsonEscaped = JsonConvert.ToString(innerJson);

        // get the json with the subFeatures as escaped
        var json = GetBlockListJson(innerJsonEscaped);

        var component = new BlockEditorPropertyHandler(s_logger);
        var result = component.ReplaceBlockListUdis(json, GuidFactory);

        // the expected result is that the subFeatures data is no longer escaped
        var expected = ReplaceGuids(
            GetBlockListJson(innerJson),
            guids,
            ContentGuid1,
            ContentGuid2,
            SettingsGuid1,
            SubContentGuid1,
            SubContentGuid2,
            SubSettingsGuid1);

        var expectedJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(expected, _serializerSettings), _serializerSettings);
        var resultJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(result, _serializerSettings), _serializerSettings);
        Console.WriteLine(expectedJson);
        Console.WriteLine(resultJson);
        Assert.AreEqual(expectedJson, resultJson);
    }

    [Test]
    public void One_Level_Nesting_Unescaped()
    {
        var guids = Enumerable.Range(0, 6).Select(x => Guid.NewGuid()).ToList();
        var guidCounter = 0;

        Guid GuidFactory()
        {
            return guids[guidCounter++];
        }

        // nested blocks without property value escaping used in the conversion
        var innerJson = GetBlockListJson(null, SubContentGuid1, SubContentGuid2, SubSettingsGuid1);

        // get the json with the subFeatures as unescaped
        var json = GetBlockListJson(innerJson);

        var expected = ReplaceGuids(
            GetBlockListJson(innerJson),
            guids,
            ContentGuid1,
            ContentGuid2,
            SettingsGuid1,
            SubContentGuid1,
            SubContentGuid2,
            SubSettingsGuid1);

        var component = new BlockEditorPropertyHandler(s_logger);
        var result = component.ReplaceBlockListUdis(json, GuidFactory);

        var expectedJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(expected, _serializerSettings), _serializerSettings);
        var resultJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(result, _serializerSettings), _serializerSettings);
        Console.WriteLine(expectedJson);
        Console.WriteLine(resultJson);
        Assert.AreEqual(expectedJson, resultJson);
    }

    [Test]
    public void Nested_In_Complex_Editor_Escaped()
    {
        var guids = Enumerable.Range(0, 6).Select(x => Guid.NewGuid()).ToList();
        var guidCounter = 0;

        Guid GuidFactory()
        {
            return guids[guidCounter++];
        }

        var innerJson = GetBlockListJson(null, SubContentGuid1, SubContentGuid2, SubSettingsGuid1);

        // we need to ensure the escaped json is consistent with how it will be re-escaped after parsing
        // and this is how to do that, the result will also include quotes around it.
        var innerJsonEscaped = JsonConvert.ToString(innerJson);

        // Complex editor such as the grid
        var complexEditorJsonEscaped = GetGridJson(innerJsonEscaped);

        var json = GetBlockListJson(complexEditorJsonEscaped);

        var component = new BlockEditorPropertyHandler(s_logger);
        var result = component.ReplaceBlockListUdis(json, GuidFactory);

        // the expected result is that the subFeatures data is no longer escaped
        var expected = ReplaceGuids(
            GetBlockListJson(GetGridJson(innerJson)),
            guids,
            ContentGuid1,
            ContentGuid2,
            SettingsGuid1,
            SubContentGuid1,
            SubContentGuid2,
            SubSettingsGuid1);

        var expectedJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(expected, _serializerSettings), _serializerSettings);
        var resultJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(result, _serializerSettings), _serializerSettings);
        Console.WriteLine(expectedJson);
        Console.WriteLine(resultJson);
        Assert.AreEqual(expectedJson, resultJson);
    }

    private string GetBlockListJson(
        string subFeatures,
        string contentGuid1 = ContentGuid1,
        string contentGuid2 = ContentGuid2,
        string settingsGuid1 = SettingsGuid1) =>
        @"{
    ""layout"":
    {
        ""Umbraco.BlockList"": [
            {
                ""contentUdi"": """ + (contentGuid1.IsNullOrWhiteSpace()
            ? string.Empty
            : Udi.Create(Constants.UdiEntityType.Element, Guid.Parse(contentGuid1)).ToString()) + @"""
            },
            {
                ""contentUdi"": ""umb://element/" + contentGuid2 + @""",
                ""settingsUdi"": ""umb://element/" + settingsGuid1 + @"""
            }
        ]
    },
    ""contentData"": [
        {
            ""contentTypeKey"": ""d6ce4a86-91a2-45b3-a99c-8691fc1fb020"",
            ""udi"": """ + (contentGuid1.IsNullOrWhiteSpace()
            ? string.Empty
            : Udi.Create(Constants.UdiEntityType.Element, Guid.Parse(contentGuid1)).ToString()) + @""",
            ""featureName"": ""Hello"",
            ""featureDetails"": ""World""
        },
        {
            ""contentTypeKey"": ""d6ce4a86-91a2-45b3-a99c-8691fc1fb020"",
            ""udi"": ""umb://element/" + contentGuid2 + @""",
            ""featureName"": ""Another"",
            ""featureDetails"": ""Feature""" +
        (subFeatures == null ? string.Empty : @", ""subFeatures"": " + subFeatures) + @"
        }
    ],
    ""settingsData"": [
        {
            ""contentTypeKey"": ""d6ce4a86-91a2-45b3-a99c-8691fc1fb020"",
            ""udi"": ""umb://element/" + settingsGuid1 + @""",
            ""featureName"": ""Setting 1"",
            ""featureDetails"": ""Setting 2""
        },
    ]
}";

    private string GetGridJson(string subBlockList) =>
        @"{
  ""name"": ""1 column layout"",
  ""sections"": [
    {
        ""grid"": ""12"",
        ""rows"": [
        {
            ""name"": ""Article"",
            ""id"": ""b4f6f651-0de3-ef46-e66a-464f4aaa9c57"",
            ""areas"": [
            {
                ""grid"": ""4"",
                ""controls"": [
                {
                    ""value"": ""I am quote"",
                    ""editor"": {
                        ""alias"": ""quote"",
                        ""view"": ""textstring""
                    },
                    ""styles"": null,
                    ""config"": null
                }],
                ""styles"": null,
                ""config"": null
            },
            {
                ""grid"": ""8"",
                ""controls"": [
                {
                    ""value"": ""Header"",
                    ""editor"": {
                        ""alias"": ""headline"",
                        ""view"": ""textstring""
                    },
                    ""styles"": null,
                    ""config"": null
                },
                {
                    ""value"": " + subBlockList + @",
                    ""editor"": {
                        ""alias"": ""madeUpNestedContent"",
                        ""view"": ""madeUpNestedContentInGrid""
                    },
                    ""styles"": null,
                    ""config"": null
                }],
                ""styles"": null,
                ""config"": null
            }],
            ""styles"": null,
            ""config"": null
        }]
    }]
}";

    private string ReplaceGuids(string json, List<Guid> newGuids, params string[] oldGuids)
    {
        for (var i = 0; i < oldGuids.Length; i++)
        {
            var old = oldGuids[i];
            json = json.Replace(old, newGuids[i].ToString("N"));
        }

        return json;
    }
}
