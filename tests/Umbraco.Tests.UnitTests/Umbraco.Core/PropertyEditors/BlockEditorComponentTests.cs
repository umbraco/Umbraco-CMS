// Copyright (c) Umbraco.
// See LICENSE for more details.

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

    [Test]
    public void Cannot_Have_Null_Udi()
    {
        var component = new BlockListPropertyNotificationHandler(Mock.Of<ILogger<BlockListPropertyNotificationHandler>>());
        var json = GetBlockListJson(null, string.Empty);
        Assert.Throws<FormatException>(() => component.ReplaceBlockEditorUdis(json));
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
        var result = component.ReplaceBlockEditorUdis(json, GuidFactory);

        var expected = ReplaceGuids(json, guidMap);
        var expectedJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(expected, _serializerSettings), _serializerSettings);
        var resultJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(result, _serializerSettings), _serializerSettings);
        Console.WriteLine(expectedJson);
        Console.WriteLine(resultJson);
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
        var innerJsonEscaped = JsonConvert.ToString(innerJson);

        // get the json with the subFeatures as escaped
        var json = GetBlockListJson(innerJsonEscaped);

        var component = new BlockListPropertyNotificationHandler(Mock.Of<ILogger<BlockListPropertyNotificationHandler>>());
        var result = component.ReplaceBlockEditorUdis(json, GuidFactory);

        // the expected result is that the subFeatures data remains escaped
        var expected = ReplaceGuids(GetBlockListJson(innerJsonEscaped), guidMap);

        var expectedJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(expected, _serializerSettings), _serializerSettings);
        var resultJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(result, _serializerSettings), _serializerSettings);
        Console.WriteLine(expectedJson);
        Console.WriteLine(resultJson);
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
        var result = component.ReplaceBlockEditorUdis(json, GuidFactory);

        var expected = ReplaceGuids(GetBlockListJson(innerJson), guidMap);
        var expectedJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(expected, _serializerSettings), _serializerSettings);
        var resultJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(result, _serializerSettings), _serializerSettings);
        Console.WriteLine(expectedJson);
        Console.WriteLine(resultJson);
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
        var innerJsonEscaped = JsonConvert.ToString(innerJson);

        // Complex editor such as the grid
        var complexEditorJsonEscaped = GetGridJson(innerJsonEscaped);

        var json = GetBlockListJson(complexEditorJsonEscaped);

        var component = new BlockListPropertyNotificationHandler(Mock.Of<ILogger<BlockListPropertyNotificationHandler>>());
        var result = component.ReplaceBlockEditorUdis(json, GuidFactory);

        // the expected result is that the subFeatures remains escaped
        Assert.True(guidMap.Any());
        var expected = ReplaceGuids(GetBlockListJson(GetGridJson(innerJsonEscaped)), guidMap);

        var expectedJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(expected, _serializerSettings), _serializerSettings);
        var resultJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(result, _serializerSettings), _serializerSettings);
        Console.WriteLine(expectedJson);
        Console.WriteLine(resultJson);
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
        var innerJsonEscaped = JsonConvert.ToString(innerJson);

        var json = GetBlockGridJson(innerJsonEscaped);

        var component = new BlockGridPropertyNotificationHandler(Mock.Of<ILogger<BlockGridPropertyNotificationHandler>>());
        var result = component.ReplaceBlockEditorUdis(json, GuidFactory);

        // the expected result is that the subFeatures remains escaped
        Assert.True(guidMap.Any());
        var expected = ReplaceGuids(GetBlockGridJson(innerJsonEscaped), guidMap);

        var expectedJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(expected, _serializerSettings), _serializerSettings);
        var resultJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(result, _serializerSettings), _serializerSettings);
        Console.WriteLine(expectedJson);
        Console.WriteLine(resultJson);
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
        var result = component.ReplaceBlockEditorUdis(json, GuidFactory);

        // the expected result is that the subFeatures remains unescaped
        Assert.True(guidMap.Any());
        var expected = ReplaceGuids(GetBlockGridJson(innerJson), guidMap);

        var expectedJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(expected, _serializerSettings), _serializerSettings);
        var resultJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(result, _serializerSettings), _serializerSettings);
        Console.WriteLine(expectedJson);
        Console.WriteLine(resultJson);
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
        var result = component.ReplaceBlockEditorUdis(json, GuidFactory);

        // the expected result is that the subFeatures remains unaltered - the UDIs within should still exist
        Assert.True(guidMap.Any());
        var expected = ReplaceGuids(GetBlockGridJson(innerJson), guidMap);

        var expectedJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(expected, _serializerSettings), _serializerSettings);
        var resultJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(result, _serializerSettings), _serializerSettings);
        Console.WriteLine(expectedJson);
        Console.WriteLine(resultJson);
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

    private string GetBlockGridJson(string subFeatures) =>
        @"{
    ""layout"": {
        ""Umbraco.BlockGrid"": [{
                ""contentUdi"": ""umb://element/d05861169d124582a7c2826e52a51b47"",
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
                ""settingsUdi"": ""umb://element/262d5efd2eeb43ed95e95c094c45ce1c""
            }, {
                ""contentUdi"": ""umb://element/5abad9f1b4e24d7aa269fbd1b50033ac"",
                ""areas"": [{
                        ""key"": ""b17663f0-c1f4-4bee-97cd-290fbc7b9a2c"",
                        ""items"": [{
                                ""contentUdi"": ""umb://element/5fc866c590be4d01a28a979472a1ffee"",
                                ""areas"": [],
                                ""columnSpan"": 3,
                                ""rowSpan"": 1
                            }
                        ]
                    }, {
                        ""key"": ""2bdcdadd-f609-4acc-b840-01970b9ced1d"",
                        ""items"": [{
                                ""contentUdi"": ""umb://element/264536b65b0f4641aa43d4bfb515831d"",
                                ""areas"": [],
                                ""columnSpan"": 3,
                                ""rowSpan"": 1,
                                ""settingsUdi"": ""umb://element/20d735c7c57b40229ed845375cf22d1f""
                            }
                        ]
                    }
                ],
                ""columnSpan"": 6,
                ""rowSpan"": 1,
                ""settingsUdi"": ""umb://element/4d121eaba49c4e09a7460069d1bee600""
            }, {
                ""contentUdi"": ""umb://element/76e24aeb6eeb4370892ca521932a96df"",
                ""areas"": [],
                ""columnSpan"": 6,
                ""rowSpan"": 1
            }, {
                ""contentUdi"": ""umb://element/90549d94555647fdbe4d111c7178ada4"",
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
                ""settingsUdi"": ""umb://element/3dfabc96584c4c35ac2e6bf06ffa20de""
            }
        ]
    },
    ""contentData"": [{
            ""contentTypeKey"": ""36ccf44a-aac8-40a6-8685-73ab03bc9709"",
            ""udi"": ""umb://element/d05861169d124582a7c2826e52a51b47"",
            ""title"": ""Element one - 12 cols""
        }, {
            ""contentTypeKey"": ""36ccf44a-aac8-40a6-8685-73ab03bc9709"",
            ""udi"": ""umb://element/5abad9f1b4e24d7aa269fbd1b50033ac"",
            ""title"": ""Element one - 6 cols, left side""
        }, {
            ""contentTypeKey"": ""5cc488aa-ba24-41f2-a01e-8f2d1982f865"",
            ""udi"": ""umb://element/76e24aeb6eeb4370892ca521932a96df"",
            ""text"": ""Element two - 6 cols, right side""
        }, {
            ""contentTypeKey"": ""36ccf44a-aac8-40a6-8685-73ab03bc9709"",
            ""udi"": ""umb://element/90549d94555647fdbe4d111c7178ada4"",
            ""title"": ""One more element one - 12 cols"",
            ""subFeatures"": " + subFeatures.OrIfNullOrWhiteSpace(@"""""") + @"
        }, {
            ""contentTypeKey"": ""5cc488aa-ba24-41f2-a01e-8f2d1982f865"",
            ""udi"": ""umb://element/5fc866c590be4d01a28a979472a1ffee"",
            ""text"": ""Nested element two - left side""
        }, {
            ""contentTypeKey"": ""36ccf44a-aac8-40a6-8685-73ab03bc9709"",
            ""udi"": ""umb://element/264536b65b0f4641aa43d4bfb515831d"",
            ""title"": ""Nested element one - right side""
        }
    ],
    ""settingsData"": [{
            ""contentTypeKey"": ""ef150524-7145-469e-8d99-166aad69a7ad"",
            ""udi"": ""umb://element/262d5efd2eeb43ed95e95c094c45ce1c"",
            ""enabled"": 1
        }, {
            ""contentTypeKey"": ""ef150524-7145-469e-8d99-166aad69a7ad"",
            ""udi"": ""umb://element/4d121eaba49c4e09a7460069d1bee600""
        }, {
            ""contentTypeKey"": ""ef150524-7145-469e-8d99-166aad69a7ad"",
            ""udi"": ""umb://element/20d735c7c57b40229ed845375cf22d1f""
        }, {
            ""contentTypeKey"": ""ef150524-7145-469e-8d99-166aad69a7ad"",
            ""udi"": ""umb://element/3dfabc96584c4c35ac2e6bf06ffa20de"",
            ""enabled"": 1
        }
    ]
}";

    private string ReplaceGuids(string json, Dictionary<Guid, Guid> guidMap)
    {
        foreach ((Guid oldKey, Guid newKey) in guidMap)
        {
            json = json.Replace(oldKey.ToString("N"), newKey.ToString("N"));
        }

        return json;
    }
}
