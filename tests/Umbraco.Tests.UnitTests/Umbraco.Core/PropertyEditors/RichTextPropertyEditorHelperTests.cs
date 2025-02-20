using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class RichTextPropertyEditorHelperTests
{
    [Test]
    public void Can_Parse_Pure_Markup_String()
    {
        var result = RichTextPropertyEditorHelper.TryParseRichTextEditorValue("<p>this is some markup</p>", JsonSerializer(), Logger(), out RichTextEditorValue? value);
        Assert.IsTrue(result);
        Assert.IsNotNull(value);
        Assert.AreEqual("<p>this is some markup</p>", value.Markup);
        Assert.IsNull(value.Blocks);
    }

    [Test]
    public void Can_Parse_JObject()
    {
        var input = JsonNode.Parse(""""
                                  {
                                   "markup": "<p>this is some markup</p><umb-rte-block data-content-key=\"36cc710a-d8a6-45d0-a07f-7bbd8742cf02\"><!--Umbraco-Block--></umb-rte-block>",
                                   "blocks": {
                                       "layout": {
                                           "Umbraco.TinyMCE": [{
                                                   "contentKey": "36cc710a-d8a6-45d0-a07f-7bbd8742cf02",
                                                   "settingsKey": "d2eeef66-4111-42f4-a164-7a523eaffbc2"
                                               }
                                           ]
                                       },
                                       "contentData": [{
                                               "contentTypeKey": "b2f0806c-d231-4c78-88b2-3c97d26e1123",
                                               "key": "36cc710a-d8a6-45d0-a07f-7bbd8742cf02",
                                               "values": [
                                                   { "alias": "contentPropertyAlias", "value": "A content property value" }
                                               ]
                                             }
                                         ],
                                         "settingsData": [{
                                                 "contentTypeKey": "e7a9447f-e14d-44dd-9ae8-e68c3c3da598",
                                                 "key": "d2eeef66-4111-42f4-a164-7a523eaffbc2",
                                                 "values": [
                                                     { "alias": "settingsPropertyAlias", "value": "A settings property value" }
                                                 ]
                                             }
                                         ]
                                     }
                                  }
                                  """");

        var result = RichTextPropertyEditorHelper.TryParseRichTextEditorValue(input, JsonSerializer(), Logger(), out RichTextEditorValue? value);
        Assert.IsTrue(result);
        Assert.IsNotNull(value);
        Assert.AreEqual("<p>this is some markup</p><umb-rte-block data-content-key=\"36cc710a-d8a6-45d0-a07f-7bbd8742cf02\"><!--Umbraco-Block--></umb-rte-block>", value.Markup);

        Assert.IsNotNull(value.Blocks);

        Assert.AreEqual(1, value.Blocks.ContentData.Count);
        var item = value.Blocks.ContentData.Single();
        var contentTypeGuid = Guid.Parse("b2f0806c-d231-4c78-88b2-3c97d26e1123");
        var itemGuid = Guid.Parse("36cc710a-d8a6-45d0-a07f-7bbd8742cf02");
        Assert.AreEqual(contentTypeGuid, item.ContentTypeKey);
        Assert.AreEqual(itemGuid, item.Key);
        Assert.AreEqual(itemGuid, item.Key);
        var contentProperties = value.Blocks.ContentData.First().Values;
        Assert.AreEqual(1, contentProperties.Count);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("contentPropertyAlias", contentProperties.First().Alias);
            Assert.AreEqual("A content property value", contentProperties.First().Value);
        });

        Assert.AreEqual(1, value.Blocks.SettingsData.Count);
        item = value.Blocks.SettingsData.Single();
        contentTypeGuid = Guid.Parse("e7a9447f-e14d-44dd-9ae8-e68c3c3da598");
        itemGuid = Guid.Parse("d2eeef66-4111-42f4-a164-7a523eaffbc2");
        Assert.AreEqual(contentTypeGuid, item.ContentTypeKey);
        Assert.AreEqual(itemGuid, item.Key);
        Assert.AreEqual(itemGuid, item.Key);
        var settingsProperties = value.Blocks.SettingsData.First().Values;
        Assert.AreEqual(1, settingsProperties.Count);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("settingsPropertyAlias", settingsProperties.First().Alias);
            Assert.AreEqual("A settings property value", settingsProperties.First().Value);
        });
    }

    [Test]
    public void Can_Parse_JObject_With_Missing_Blocks()
    {
        var input = JsonNode.Parse(""""
                                   {
                                    "markup": "<h2>Vælg et af vores mest populære produkter</h2>"
                                   }
                                   """");

        var result = RichTextPropertyEditorHelper.TryParseRichTextEditorValue(input, JsonSerializer(), Logger(), out RichTextEditorValue? value);
        Assert.IsTrue(result);
        Assert.IsNotNull(value);
        Assert.AreEqual("<h2>Vælg et af vores mest populære produkter</h2>", value.Markup);
        Assert.IsNull(value.Blocks);
    }

    [Test]
    public void Can_Parse_Blocks_With_Both_Content_And_Settings()
    {
        const string input = """
                             {
                              "markup": "<p>this is some markup</p><umb-rte-block data-content-key=\"36cc710a-d8a6-45d0-a07f-7bbd8742cf02\"><!--Umbraco-Block--></umb-rte-block>",
                              "blocks": {
                                  "layout": {
                                      "Umbraco.TinyMCE": [{
                                              "contentKey": "36cc710a-d8a6-45d0-a07f-7bbd8742cf02",
                                              "settingsKey": "d2eeef66-4111-42f4-a164-7a523eaffbc2"
                                          }
                                      ]
                                  },
                                  "contentData": [{
                                          "contentTypeKey": "b2f0806c-d231-4c78-88b2-3c97d26e1123",
                                          "key": "36cc710a-d8a6-45d0-a07f-7bbd8742cf02",
                                          "values": [
                                              { "alias": "contentPropertyAlias", "value": "A content property value" }
                                          ]
                                        }
                                    ],
                                    "settingsData": [{
                                            "contentTypeKey": "e7a9447f-e14d-44dd-9ae8-e68c3c3da598",
                                            "key": "d2eeef66-4111-42f4-a164-7a523eaffbc2",
                                            "values": [
                                                { "alias": "settingsPropertyAlias", "value": "A settings property value" }
                                            ]
                                        }
                                    ]
                                }
                             }
                             """;

        var result = RichTextPropertyEditorHelper.TryParseRichTextEditorValue(input, JsonSerializer(), Logger(), out RichTextEditorValue? value);
        Assert.IsTrue(result);
        Assert.IsNotNull(value);
        Assert.AreEqual("<p>this is some markup</p><umb-rte-block data-content-key=\"36cc710a-d8a6-45d0-a07f-7bbd8742cf02\"><!--Umbraco-Block--></umb-rte-block>", value.Markup);

        Assert.IsNotNull(value.Blocks);

        Assert.AreEqual(1, value.Blocks.ContentData.Count);
        var item = value.Blocks.ContentData.Single();
        var contentTypeGuid = Guid.Parse("b2f0806c-d231-4c78-88b2-3c97d26e1123");
        var itemGuid = Guid.Parse("36cc710a-d8a6-45d0-a07f-7bbd8742cf02");
        Assert.AreEqual(contentTypeGuid, item.ContentTypeKey);
        Assert.AreEqual(itemGuid, item.Key);
        Assert.AreEqual(itemGuid, item.Key);
        var contentProperties = value.Blocks.ContentData.First().Values;
        Assert.AreEqual(1, contentProperties.Count);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("contentPropertyAlias", contentProperties.First().Alias);
            Assert.AreEqual("A content property value", contentProperties.First().Value);
        });

        Assert.AreEqual(1, value.Blocks.SettingsData.Count);
        item = value.Blocks.SettingsData.Single();
        contentTypeGuid = Guid.Parse("e7a9447f-e14d-44dd-9ae8-e68c3c3da598");
        itemGuid = Guid.Parse("d2eeef66-4111-42f4-a164-7a523eaffbc2");
        Assert.AreEqual(contentTypeGuid, item.ContentTypeKey);
        Assert.AreEqual(itemGuid, item.Key);
        Assert.AreEqual(itemGuid, item.Key);
        var settingsProperties = value.Blocks.SettingsData.First().Values;
        Assert.AreEqual(1, settingsProperties.Count);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("settingsPropertyAlias", settingsProperties.First().Alias);
            Assert.AreEqual("A settings property value", settingsProperties.First().Value);
        });
    }

    [Test]
    public void Can_Parse_Blocks_With_Content_Only()
    {
        const string input = """
                             {
                              "markup": "<p>this is some markup</p><umb-rte-block data-content-key=\"36cc710a-d8a6-45d0-a07f-7bbd8742cf02\"></umb-rte-block>",
                              "blocks": {
                                  "layout": {
                                      "Umbraco.TinyMCE": [{
                                              "contentKey": "36cc710a-d8a6-45d0-a07f-7bbd8742cf02"
                                          }
                                      ]
                                  },
                                  "contentData": [{
                                          "contentTypeKey": "b2f0806c-d231-4c78-88b2-3c97d26e1123",
                                          "key": "36cc710a-d8a6-45d0-a07f-7bbd8742cf02",
                                          "values": [
                                              { "alias": "contentPropertyAlias", "value": "A content property value" }
                                          ]
                                        }
                                    ],
                                    "settingsData": []
                                }
                             }
                             """;

        var result = RichTextPropertyEditorHelper.TryParseRichTextEditorValue(input, JsonSerializer(), Logger(), out RichTextEditorValue? value);
        Assert.IsTrue(result);
        Assert.IsNotNull(value);
        Assert.AreEqual("<p>this is some markup</p><umb-rte-block data-content-key=\"36cc710a-d8a6-45d0-a07f-7bbd8742cf02\"></umb-rte-block>", value.Markup);

        Assert.IsNotNull(value.Blocks);

        Assert.AreEqual(1, value.Blocks.ContentData.Count);
        var item = value.Blocks.ContentData.Single();
        var contentTypeGuid = Guid.Parse("b2f0806c-d231-4c78-88b2-3c97d26e1123");
        var itemGuid = Guid.Parse("36cc710a-d8a6-45d0-a07f-7bbd8742cf02");
        Assert.AreEqual(contentTypeGuid, item.ContentTypeKey);
        Assert.AreEqual(itemGuid, item.Key);
        Assert.AreEqual(itemGuid, item.Key);
        var contentProperties = value.Blocks.ContentData.First().Values;
        Assert.AreEqual(1, contentProperties.Count);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("contentPropertyAlias", contentProperties.First().Alias);
            Assert.AreEqual("A content property value", contentProperties.First().Value);
        });

        Assert.AreEqual(0, value.Blocks.SettingsData.Count);
    }

    [Test]
    public void Can_Parse_Mixed_Blocks_And_Inline_Blocks()
    {
        const string input = """
                             {
                              "markup": "<p>this is <umb-rte-block-inline data-content-key=\"36cc710a-d8a6-45d0-a07f-7bbd8742cf03\"></umb-rte-block-inline> some markup</p><umb-rte-block data-content-key=\"36cc710a-d8a6-45d0-a07f-7bbd8742cf02\"></umb-rte-block>",
                              "blocks": {
                                  "layout": {
                                      "Umbraco.TinyMCE": [{
                                              "contentKey": "36cc710a-d8a6-45d0-a07f-7bbd8742cf02"
                                          }, {
                                              "contentKey": "36cc710a-d8a6-45d0-a07f-7bbd8742cf03"
                                          }
                                      ]
                                  },
                                  "contentData": [{
                                          "contentTypeKey": "b2f0806c-d231-4c78-88b2-3c97d26e1123",
                                          "key": "36cc710a-d8a6-45d0-a07f-7bbd8742cf02",
                                          "contentPropertyAlias": "A content property value"
                                        }, {
                                          "contentTypeKey": "b2f0806c-d231-4c78-88b2-3c97d26e1124",
                                          "key": "36cc710a-d8a6-45d0-a07f-7bbd8742cf03",
                                          "contentPropertyAlias": "A content property value"
                                        }
                                    ],
                                    "settingsData": []
                                }
                             }
                             """;

        var result = RichTextPropertyEditorHelper.TryParseRichTextEditorValue(input, JsonSerializer(), Logger(), out RichTextEditorValue? value);
        Assert.IsTrue(result);
        Assert.IsNotNull(value);
        Assert.AreEqual("<p>this is <umb-rte-block-inline data-content-key=\"36cc710a-d8a6-45d0-a07f-7bbd8742cf03\"></umb-rte-block-inline> some markup</p><umb-rte-block data-content-key=\"36cc710a-d8a6-45d0-a07f-7bbd8742cf02\"></umb-rte-block>", value.Markup);

        Assert.IsNotNull(value.Blocks);

        Guid[] contentTypeGuids = [Guid.Parse("b2f0806c-d231-4c78-88b2-3c97d26e1123"), Guid.Parse("b2f0806c-d231-4c78-88b2-3c97d26e1124")];
        Guid[] itemGuids = [Guid.Parse("36cc710a-d8a6-45d0-a07f-7bbd8742cf02"), Guid.Parse("36cc710a-d8a6-45d0-a07f-7bbd8742cf03")];

        Assert.AreEqual(2, value.Blocks.ContentData.Count);
        for (var i = 0; i < value.Blocks.ContentData.Count; i++)
        {
            var item = value.Blocks.ContentData[i];
            Assert.AreEqual(contentTypeGuids[i], item.ContentTypeKey);
            Assert.AreEqual(itemGuids[i], item.Key);
        }

        Assert.AreEqual(0, value.Blocks.SettingsData.Count);
    }

    private IJsonSerializer JsonSerializer() => new SystemTextJsonSerializer();

    private ILogger Logger() => Mock.Of<ILogger>();
}
