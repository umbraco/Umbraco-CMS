// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class BlockEditorComponentTests
{
    private const string ContentGuid1 = "709b857e-6f00-45c6-bf65-f7da028c361f";
    private const string ContentGuid2 = "823dc755-28ec-4198-b050-514d91b7994e";
    private const string SettingsGuid1 = "4d2e18fe-f030-4ea9-aed9-10e7aee265fd";
    private const string SubContentGuid1 = "b5698cf9-bf26-4c1c-8b1c-db30a1b5c56a";
    private const string SubContentGuid2 = "68606a64-a03a-4b78-bcb1-39daee0c590d";
    private const string SubSettingsGuid1 = "5ce1b7da-7c9f-491e-9b95-5510fd28c50c";

    private readonly IJsonSerializer _jsonSerializer = new SystemTextJsonSerializer();

    [Test]
    public void Cannot_Have_Null_Udi()
    {
        var component = new BlockListPropertyNotificationHandler(Mock.Of<ILogger<BlockListPropertyNotificationHandler>>());
        var json = GetBlockListJson(null, string.Empty);
        Assert.Throws<FormatException>(() => component.ReplaceBlockEditorKeys(json));
    }

    [Test]
    public void No_Nesting()
    {
        var guidMap = new Dictionary<Guid, Guid>();
        Guid GuidFactory(Guid oldKey)
        {
            guidMap[oldKey] = Guid.NewGuid();
            return guidMap[oldKey];
        }

        var json = GetBlockListJson(null);

        var component = new BlockListPropertyNotificationHandler(Mock.Of<ILogger<BlockListPropertyNotificationHandler>>());
        var result = component.ReplaceBlockEditorKeys(json, GuidFactory);

        Assert.AreEqual(3, guidMap.Count);
        var expected = ReplaceGuids(json, guidMap);
        var expectedJson = _jsonSerializer.Serialize( _jsonSerializer.Deserialize<BlockListValue>(expected));
        var resultJson = _jsonSerializer.Serialize(_jsonSerializer.Deserialize<BlockListValue>(result));
        Assert.IsNotEmpty(resultJson);
        Assert.AreEqual(expectedJson, resultJson);
    }

    [Test]
    public void One_Level_Nesting_Escaped()
    {
        var guidMap = new Dictionary<Guid, Guid>();
        Guid GuidFactory(Guid oldKey)
        {
            guidMap[oldKey] = Guid.NewGuid();
            return guidMap[oldKey];
        }

        var innerJson = GetBlockListJson(null, SubContentGuid1, SubContentGuid2, SubSettingsGuid1);

        // we need to ensure the escaped json is consistent with how it will be re-escaped after parsing
        // and this is how to do that, the result will also include quotes around it.
        var innerJsonEscaped = Escape(innerJson);

        // get the json with the subFeatures as escaped
        var json = GetBlockListJson(innerJsonEscaped);

        var component = new BlockListPropertyNotificationHandler(Mock.Of<ILogger<BlockListPropertyNotificationHandler>>());
        var result = component.ReplaceBlockEditorKeys(json, GuidFactory);

        // the expected result is that the subFeatures data remains escaped
        Assert.AreEqual(6, guidMap.Count);
        var expected = ReplaceGuids(GetBlockListJson(innerJsonEscaped), guidMap);

        var expectedJson = _jsonSerializer.Serialize( _jsonSerializer.Deserialize<BlockListValue>(expected));
        var resultJson = _jsonSerializer.Serialize(_jsonSerializer.Deserialize<BlockListValue>(result));
        Assert.IsNotEmpty(resultJson);
        Assert.AreEqual(expectedJson, resultJson);
    }

    [Test]
    public void One_Level_Nesting_Unescaped()
    {
        var guidMap = new Dictionary<Guid, Guid>();
        Guid GuidFactory(Guid oldKey)
        {
            guidMap[oldKey] = Guid.NewGuid();
            return guidMap[oldKey];
        }

        // nested blocks without property value escaping used in the conversion
        var innerJson = GetBlockListJson(null, SubContentGuid1, SubContentGuid2, SubSettingsGuid1);

        // get the json with the subFeatures as unescaped
        var json = GetBlockListJson(innerJson);

        var component = new BlockListPropertyNotificationHandler(Mock.Of<ILogger<BlockListPropertyNotificationHandler>>());
        var result = component.ReplaceBlockEditorKeys(json, GuidFactory);

        Assert.AreEqual(6, guidMap.Count);
        var expected = ReplaceGuids(GetBlockListJson(innerJson), guidMap);
        var expectedJson = _jsonSerializer.Serialize( _jsonSerializer.Deserialize<BlockListValue>(expected));
        var resultJson = _jsonSerializer.Serialize(_jsonSerializer.Deserialize<BlockListValue>(result));
        Assert.IsNotEmpty(resultJson);
        Assert.AreEqual(expectedJson, resultJson);
    }

    [Test]
    public void Nested_In_Complex_Editor_Escaped()
    {
        var guidMap = new Dictionary<Guid, Guid>();
        Guid GuidFactory(Guid oldKey)
        {
            guidMap[oldKey] = Guid.NewGuid();
            return guidMap[oldKey];
        }

        var innerJson = GetBlockListJson(null, SubContentGuid1, SubContentGuid2, SubSettingsGuid1);

        // we need to ensure the escaped json is consistent with how it will be re-escaped after parsing
        // and this is how to do that, the result will also include quotes around it.
        var innerJsonEscaped = Escape(innerJson);

        // Complex editor such as the grid
        var complexEditorJsonEscaped = GetGridJson(innerJsonEscaped);

        var json = GetBlockListJson(complexEditorJsonEscaped);

        var component = new BlockListPropertyNotificationHandler(Mock.Of<ILogger<BlockListPropertyNotificationHandler>>());
        var result = component.ReplaceBlockEditorKeys(json, GuidFactory);

        // the expected result is that the subFeatures remains escaped
        Assert.AreEqual(6, guidMap.Count);
        var expected = ReplaceGuids(GetBlockListJson(GetGridJson(innerJsonEscaped)), guidMap);

        var expectedJson = _jsonSerializer.Serialize( _jsonSerializer.Deserialize<BlockListValue>(expected));
        var resultJson = _jsonSerializer.Serialize(_jsonSerializer.Deserialize<BlockListValue>(result));
        Assert.IsNotEmpty(resultJson);
        Assert.AreEqual(expectedJson, resultJson);
    }

    [Test]
    public void BlockGrid_With_Nested_BlockList_Escaped()
    {
        var guidMap = new Dictionary<Guid, Guid>();
        Guid GuidFactory(Guid oldKey)
        {
            guidMap[oldKey] = Guid.NewGuid();
            return guidMap[oldKey];
        }

        var innerJson = GetBlockListJson(null, SubContentGuid1, SubContentGuid2, SubSettingsGuid1);

        // we need to ensure the escaped json is consistent with how it will be re-escaped after parsing
        // and this is how to do that, the result will also include quotes around it.
        var innerJsonEscaped = Escape(innerJson);

        var json = GetBlockGridJson(innerJsonEscaped);

        var component = new BlockGridPropertyNotificationHandler(Mock.Of<ILogger<BlockGridPropertyNotificationHandler>>());
        var result = component.ReplaceBlockEditorKeys(json, GuidFactory);

        // the expected result is that the subFeatures remains escaped
        Assert.AreEqual(13, guidMap.Count);
        var expected = ReplaceGuids(GetBlockGridJson(innerJsonEscaped), guidMap);

        var expectedJson = _jsonSerializer.Serialize( _jsonSerializer.Deserialize<BlockGridValue>(expected));
        var resultJson = _jsonSerializer.Serialize(_jsonSerializer.Deserialize<BlockGridValue>(result));
        Assert.IsNotEmpty(resultJson);
        Assert.AreEqual(expectedJson, resultJson);
    }

    [Test]
    public void BlockGrid_With_Nested_BlockList_Unescaped()
    {
        var guidMap = new Dictionary<Guid, Guid>();
        Guid GuidFactory(Guid oldKey)
        {
            guidMap[oldKey] = Guid.NewGuid();
            return guidMap[oldKey];
        }

        var innerJson = GetBlockListJson(null, SubContentGuid1, SubContentGuid2, SubSettingsGuid1);

        var json = GetBlockGridJson(innerJson);

        var component = new BlockGridPropertyNotificationHandler(Mock.Of<ILogger<BlockGridPropertyNotificationHandler>>());
        var result = component.ReplaceBlockEditorKeys(json, GuidFactory);

        // the expected result is that the subFeatures remains unescaped
        Assert.AreEqual(13, guidMap.Count);
        var expected = ReplaceGuids(GetBlockGridJson(innerJson), guidMap);

        var expectedJson = _jsonSerializer.Serialize( _jsonSerializer.Deserialize<BlockGridValue>(expected));
        var resultJson = _jsonSerializer.Serialize(_jsonSerializer.Deserialize<BlockGridValue>(result));
        Assert.IsNotEmpty(resultJson);
        Assert.AreEqual(expectedJson, resultJson);
    }

    [Test]
    public void BlockGrid_With_Nested_Udi_Based_Editor()
    {
        var guidMap = new Dictionary<Guid, Guid>();
        Guid GuidFactory(Guid oldKey)
        {
            guidMap[oldKey] = Guid.NewGuid();
            return guidMap[oldKey];
        }

        var innerJson = @"{
            ""udi"": ""umb://element/eb459ab17259495b90a3d2f6bb299826"",
            ""title"": ""Some title"",
            ""nested"": {
                ""udi"": ""umb://element/7f33e17a00b742cebd1eb7f2af4c56b5""
            }
        }";

        var json = GetBlockGridJson(innerJson);

        var component = new BlockGridPropertyNotificationHandler(Mock.Of<ILogger<BlockGridPropertyNotificationHandler>>());
        var result = component.ReplaceBlockEditorKeys(json, GuidFactory);

        // the expected result is that the subFeatures remains unaltered - the UDIs within should still exist
        Assert.AreEqual(10, guidMap.Count);
        var expected = ReplaceGuids(GetBlockGridJson(innerJson), guidMap);

        var expectedJson = _jsonSerializer.Serialize( _jsonSerializer.Deserialize<BlockGridValue>(expected));
        var resultJson = _jsonSerializer.Serialize(_jsonSerializer.Deserialize<BlockGridValue>(result));
        Assert.IsNotEmpty(resultJson);
        Assert.AreEqual(expectedJson, resultJson);

        Assert.True(result.Contains("umb://element/eb459ab17259495b90a3d2f6bb299826"));
        Assert.True(result.Contains("umb://element/7f33e17a00b742cebd1eb7f2af4c56b5"));
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
                ""contentKey"": """ + (contentGuid1.IsNullOrWhiteSpace() ? string.Empty : contentGuid1) + @"""
            },
            {
                ""contentKey"": """ + contentGuid2 + @""",
                ""settingsKey"": """ + settingsGuid1 + @"""
            }
        ]
    },
    ""contentData"": [
        {
            ""contentTypeKey"": ""d6ce4a86-91a2-45b3-a99c-8691fc1fb020"",
            ""key"": """ + (contentGuid1.IsNullOrWhiteSpace() ? string.Empty : contentGuid1) + @""",
            ""values"": [
                { ""alias"": ""featureName"", ""value"": ""Hello"" },
                { ""alias"": ""featureDetails"", ""value"": ""World"" }
            ]
        },
        {
            ""contentTypeKey"": ""d6ce4a86-91a2-45b3-a99c-8691fc1fb020"",
            ""key"": """ + contentGuid2 + @""",
            ""values"": [
                { ""alias"": ""featureName"", ""value"": ""Another"" },
                { ""alias"": ""featureDetails"", ""value"": ""Feature"" },
                { ""alias"": ""subFeatures"", ""value"": " + subFeatures.OrIfNullOrWhiteSpace(@"""""") + @" }
            ]
        }
    ],
    ""settingsData"": [
        {
            ""contentTypeKey"": ""d6ce4a86-91a2-45b3-a99c-8691fc1fb020"",
            ""key"": """ + settingsGuid1 + @""",
            ""values"": [
                { ""alias"": ""featureName"", ""value"": ""Setting 1"" },
                { ""alias"": ""featureDetails"", ""value"": ""Setting 2"" }
            ]
        }
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

    private string GetBlockGridJson(string subFeatures) =>
        @"{
    ""layout"": {
        ""Umbraco.BlockGrid"": [{
                ""contentKey"": ""fb0595b1-26e7-493f-86c7-bf2c42326850"",
                ""areas"": [{
                        ""key"": ""b17663f0-c1f4-4bee-97cd-290fbc7b9a2c"",
                        ""items"": []
                    }, {
                        ""key"": ""2bdcdadd-f609-4acc-b840-01970b9ced1d"",
                        ""items"": []
                    }
                ],
                ""columnSpan"": 12,
                ""rowSpan"": 1,
                ""settingsKey"": ""0183ae81-2b62-49b5-8ac6-88d66c33068c""
            }, {
                ""contentKey"": ""4852d9ef-ac8d-4d44-87c9-82d282aa0e7f"",
                ""areas"": [{
                        ""key"": ""b17663f0-c1f4-4bee-97cd-290fbc7b9a2c"",
                        ""items"": [{
                                ""contentKey"": ""96a15ca9-3970-4e0a-9c66-18433bc23274"",
                                ""areas"": [],
                                ""columnSpan"": 3,
                                ""rowSpan"": 1
                            }
                        ]
                    }, {
                        ""key"": ""2bdcdadd-f609-4acc-b840-01970b9ced1d"",
                        ""items"": [{
                                ""contentKey"": ""3093f7f1-c931-4325-ba71-638eb2746c8d"",
                                ""areas"": [],
                                ""columnSpan"": 3,
                                ""rowSpan"": 1,
                                ""settingsKey"": ""bef9eb67-56de-4fec-9fbc-1c7c02f5a5a7""
                            }
                        ]
                    }
                ],
                ""columnSpan"": 6,
                ""rowSpan"": 1,
                ""settingsKey"": ""6eed3662-6ad1-4cba-805b-352f28599b0d""
            }, {
                ""contentKey"": ""1f778485-933e-40b4-91e2-9926857a5c81"",
                ""areas"": [],
                ""columnSpan"": 6,
                ""rowSpan"": 1
            }, {
                ""contentKey"": ""2d5c6555-0dd8-4db2-b0c9-2d2eba29026d"",
                ""areas"": [{
                        ""key"": ""b17663f0-c1f4-4bee-97cd-290fbc7b9a2c"",
                        ""items"": []
                    }, {
                        ""key"": ""2bdcdadd-f609-4acc-b840-01970b9ced1d"",
                        ""items"": []
                    }
                ],
                ""columnSpan"": 12,
                ""rowSpan"": 3,
                ""settingsKey"": ""48a7b7da-673f-44d5-8bad-7d71d157fb3e""
            }
        ]
    },
    ""contentData"": [{
            ""contentTypeKey"": ""36ccf44a-aac8-40a6-8685-73ab03bc9709"",
            ""key"": ""fb0595b1-26e7-493f-86c7-bf2c42326850"",
            ""values"": [
                { ""alias"": ""title"", ""value"": ""Element one - 12 cols"" }
            ]
        }, {
            ""contentTypeKey"": ""36ccf44a-aac8-40a6-8685-73ab03bc9709"",
            ""key"": ""4852d9ef-ac8d-4d44-87c9-82d282aa0e7f"",
            ""values"": [
                { ""alias"": ""title"", ""value"": ""Element one - 6 cols, left side"" }
            ]
        }, {
            ""contentTypeKey"": ""5cc488aa-ba24-41f2-a01e-8f2d1982f865"",
            ""key"": ""1f778485-933e-40b4-91e2-9926857a5c81"",
            ""values"": [
                { ""alias"": ""title"", ""value"": ""Element one - 6 cols, right side"" }
            ]
        }, {
            ""contentTypeKey"": ""36ccf44a-aac8-40a6-8685-73ab03bc9709"",
            ""key"": ""2d5c6555-0dd8-4db2-b0c9-2d2eba29026d"",
            ""values"": [
                { ""alias"": ""title"", ""value"": ""One more element one - 12 cols"" },
                { ""alias"": ""subFeatures"", ""value"": " + subFeatures.OrIfNullOrWhiteSpace(@"""""") + @" }
            ]
        }, {
            ""contentTypeKey"": ""5cc488aa-ba24-41f2-a01e-8f2d1982f865"",
            ""key"": ""96a15ca9-3970-4e0a-9c66-18433bc23274"",
            ""values"": [
                { ""alias"": ""title"", ""value"": ""Nested element two - left side"" }
            ]
        }, {
            ""contentTypeKey"": ""36ccf44a-aac8-40a6-8685-73ab03bc9709"",
            ""key"": ""3093f7f1-c931-4325-ba71-638eb2746c8d"",
            ""values"": [
                { ""alias"": ""title"", ""value"": ""Nested element one - right side"" }
            ]
        }
    ],
    ""settingsData"": [{
            ""contentTypeKey"": ""ef150524-7145-469e-8d99-166aad69a7ad"",
            ""key"": ""0183ae81-2b62-49b5-8ac6-88d66c33068c"",
            ""values"": [
                { ""alias"": ""enabled"", ""value"": 1 }
            ]
        }, {
            ""contentTypeKey"": ""ef150524-7145-469e-8d99-166aad69a7ad"",
            ""key"": ""6eed3662-6ad1-4cba-805b-352f28599b0d""
        }, {
            ""contentTypeKey"": ""ef150524-7145-469e-8d99-166aad69a7ad"",
            ""key"": ""bef9eb67-56de-4fec-9fbc-1c7c02f5a5a7""
        }, {
            ""contentTypeKey"": ""ef150524-7145-469e-8d99-166aad69a7ad"",
            ""key"": ""48a7b7da-673f-44d5-8bad-7d71d157fb3e"",
            ""values"": [
                { ""alias"": ""enabled"", ""value"": 1 }
            ]
        }
    ]
}";

    private string ReplaceGuids(string json, Dictionary<Guid, Guid> guidMap)
    {
        foreach ((Guid oldKey, Guid newKey) in guidMap)
        {
            json = json.Replace(oldKey.ToString("D"), newKey.ToString("D"));
        }

        return json;
    }

    private string Escape(string json) => $"\"{System.Web.HttpUtility.JavaScriptStringEncode(json)}\"";
}
