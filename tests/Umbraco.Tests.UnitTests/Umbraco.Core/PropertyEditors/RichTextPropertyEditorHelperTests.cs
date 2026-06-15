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
        Assert.That(result, Is.True);
        Assert.That(value, Is.Not.Null);
        Assert.That(value.Markup, Is.EqualTo("<p>this is some markup</p>"));
        Assert.That(value.Blocks, Is.Null);
    }

    [Test]
    public void Can_Parse_JObject()
    {
        var input = JsonNode.Parse(""""
                                  {
                                   "markup": "<p>this is some markup</p><umb-rte-block data-content-key=\"36cc710a-d8a6-45d0-a07f-7bbd8742cf02\"><!--Umbraco-Block--></umb-rte-block>",
                                   "blocks": {
                                       "layout": {
                                           "Umbraco.RichText": [{
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
        Assert.That(result, Is.True);
        Assert.That(value, Is.Not.Null);
        Assert.That(value.Markup, Is.EqualTo("<p>this is some markup</p><umb-rte-block data-content-key=\"36cc710a-d8a6-45d0-a07f-7bbd8742cf02\"><!--Umbraco-Block--></umb-rte-block>"));

        Assert.That(value.Blocks, Is.Not.Null);

        Assert.That(value.Blocks.ContentData, Has.Count.EqualTo(1));
        var item = value.Blocks.ContentData.Single();
        var contentTypeGuid = Guid.Parse("b2f0806c-d231-4c78-88b2-3c97d26e1123");
        var itemGuid = Guid.Parse("36cc710a-d8a6-45d0-a07f-7bbd8742cf02");
        Assert.That(item.ContentTypeKey, Is.EqualTo(contentTypeGuid));
        Assert.That(item.Key, Is.EqualTo(itemGuid));
        Assert.That(item.Key, Is.EqualTo(itemGuid));
        var contentProperties = value.Blocks.ContentData.First().Values;
        Assert.That(contentProperties, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(contentProperties.First().Alias, Is.EqualTo("contentPropertyAlias"));
            Assert.That(contentProperties.First().Value, Is.EqualTo("A content property value"));
        });

        Assert.That(value.Blocks.SettingsData, Has.Count.EqualTo(1));
        item = value.Blocks.SettingsData.Single();
        contentTypeGuid = Guid.Parse("e7a9447f-e14d-44dd-9ae8-e68c3c3da598");
        itemGuid = Guid.Parse("d2eeef66-4111-42f4-a164-7a523eaffbc2");
        Assert.That(item.ContentTypeKey, Is.EqualTo(contentTypeGuid));
        Assert.That(item.Key, Is.EqualTo(itemGuid));
        Assert.That(item.Key, Is.EqualTo(itemGuid));
        var settingsProperties = value.Blocks.SettingsData.First().Values;
        Assert.That(settingsProperties, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(settingsProperties.First().Alias, Is.EqualTo("settingsPropertyAlias"));
            Assert.That(settingsProperties.First().Value, Is.EqualTo("A settings property value"));
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
        Assert.That(result, Is.True);
        Assert.That(value, Is.Not.Null);
        Assert.That(value.Markup, Is.EqualTo("<h2>Vælg et af vores mest populære produkter</h2>"));
        Assert.That(value.Blocks, Is.Null);
    }

    [TestCase(Constants.PropertyEditors.Aliases.RichText)]
    [TestCase("Umbraco.TinyMCE")]
    public void Can_Parse_Blocks_With_Both_Content_And_Settings(string propertyEditorAlias)
    {
        string input = """
                             {
                              "markup": "<p>this is some markup</p><umb-rte-block data-content-key=\"36cc710a-d8a6-45d0-a07f-7bbd8742cf02\"><!--Umbraco-Block--></umb-rte-block>",
                              "blocks": {
                                  "layout": {
                                      "[PropertyEditorAlias]": [{
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
        input = input.Replace("[PropertyEditorAlias]", propertyEditorAlias);

        var result = RichTextPropertyEditorHelper.TryParseRichTextEditorValue(input, JsonSerializer(), Logger(), out RichTextEditorValue? value);
        Assert.That(result, Is.True);
        Assert.That(value, Is.Not.Null);
        Assert.That(value.Markup, Is.EqualTo("<p>this is some markup</p><umb-rte-block data-content-key=\"36cc710a-d8a6-45d0-a07f-7bbd8742cf02\"><!--Umbraco-Block--></umb-rte-block>"));

        Assert.That(value.Blocks, Is.Not.Null);

        Assert.That(value.Blocks.ContentData, Has.Count.EqualTo(1));
        var item = value.Blocks.ContentData.Single();
        var contentTypeGuid = Guid.Parse("b2f0806c-d231-4c78-88b2-3c97d26e1123");
        var itemGuid = Guid.Parse("36cc710a-d8a6-45d0-a07f-7bbd8742cf02");
        Assert.That(item.ContentTypeKey, Is.EqualTo(contentTypeGuid));
        Assert.That(item.Key, Is.EqualTo(itemGuid));
        Assert.That(item.Key, Is.EqualTo(itemGuid));
        var contentProperties = value.Blocks.ContentData.First().Values;
        Assert.That(contentProperties, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(contentProperties.First().Alias, Is.EqualTo("contentPropertyAlias"));
            Assert.That(contentProperties.First().Value, Is.EqualTo("A content property value"));
        });

        Assert.That(value.Blocks.SettingsData, Has.Count.EqualTo(1));
        item = value.Blocks.SettingsData.Single();
        contentTypeGuid = Guid.Parse("e7a9447f-e14d-44dd-9ae8-e68c3c3da598");
        itemGuid = Guid.Parse("d2eeef66-4111-42f4-a164-7a523eaffbc2");
        Assert.That(item.ContentTypeKey, Is.EqualTo(contentTypeGuid));
        Assert.That(item.Key, Is.EqualTo(itemGuid));
        Assert.That(item.Key, Is.EqualTo(itemGuid));
        var settingsProperties = value.Blocks.SettingsData.First().Values;
        Assert.That(settingsProperties, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(settingsProperties.First().Alias, Is.EqualTo("settingsPropertyAlias"));
            Assert.That(settingsProperties.First().Value, Is.EqualTo("A settings property value"));
        });

        Assert.That(value.Blocks.Layout.ContainsKey(Constants.PropertyEditors.Aliases.RichText), Is.True);
        var layout = value.Blocks.Layout[Constants.PropertyEditors.Aliases.RichText];
        Assert.That(layout.Count(), Is.EqualTo(1));
        Assert.That(layout.First().ContentKey, Is.EqualTo(Guid.Parse("36cc710a-d8a6-45d0-a07f-7bbd8742cf02")));
        Assert.That(layout.First().SettingsKey, Is.EqualTo(Guid.Parse("d2eeef66-4111-42f4-a164-7a523eaffbc2")));
    }

    [Test]
    public void Can_Parse_Blocks_With_Content_Only()
    {
        const string input = """
                             {
                              "markup": "<p>this is some markup</p><umb-rte-block data-content-key=\"36cc710a-d8a6-45d0-a07f-7bbd8742cf02\"></umb-rte-block>",
                              "blocks": {
                                  "layout": {
                                      "Umbraco.RichText": [{
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
        Assert.That(result, Is.True);
        Assert.That(value, Is.Not.Null);
        Assert.That(value.Markup, Is.EqualTo("<p>this is some markup</p><umb-rte-block data-content-key=\"36cc710a-d8a6-45d0-a07f-7bbd8742cf02\"></umb-rte-block>"));

        Assert.That(value.Blocks, Is.Not.Null);

        Assert.That(value.Blocks.ContentData, Has.Count.EqualTo(1));
        var item = value.Blocks.ContentData.Single();
        var contentTypeGuid = Guid.Parse("b2f0806c-d231-4c78-88b2-3c97d26e1123");
        var itemGuid = Guid.Parse("36cc710a-d8a6-45d0-a07f-7bbd8742cf02");
        Assert.That(item.ContentTypeKey, Is.EqualTo(contentTypeGuid));
        Assert.That(item.Key, Is.EqualTo(itemGuid));
        Assert.That(item.Key, Is.EqualTo(itemGuid));
        var contentProperties = value.Blocks.ContentData.First().Values;
        Assert.That(contentProperties, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(contentProperties.First().Alias, Is.EqualTo("contentPropertyAlias"));
            Assert.That(contentProperties.First().Value, Is.EqualTo("A content property value"));
        });

        Assert.That(value.Blocks.SettingsData.Count, Is.EqualTo(0));
    }

    [Test]
    public void Can_Parse_Mixed_Blocks_And_Inline_Blocks()
    {
        const string input = """
                             {
                              "markup": "<p>this is <umb-rte-block-inline data-content-key=\"36cc710a-d8a6-45d0-a07f-7bbd8742cf03\"></umb-rte-block-inline> some markup</p><umb-rte-block data-content-key=\"36cc710a-d8a6-45d0-a07f-7bbd8742cf02\"></umb-rte-block>",
                              "blocks": {
                                  "layout": {
                                      "Umbraco.RichText": [{
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
        Assert.That(result, Is.True);
        Assert.That(value, Is.Not.Null);
        Assert.That(value.Markup, Is.EqualTo("<p>this is <umb-rte-block-inline data-content-key=\"36cc710a-d8a6-45d0-a07f-7bbd8742cf03\"></umb-rte-block-inline> some markup</p><umb-rte-block data-content-key=\"36cc710a-d8a6-45d0-a07f-7bbd8742cf02\"></umb-rte-block>"));

        Assert.That(value.Blocks, Is.Not.Null);

        Guid[] contentTypeGuids = [Guid.Parse("b2f0806c-d231-4c78-88b2-3c97d26e1123"), Guid.Parse("b2f0806c-d231-4c78-88b2-3c97d26e1124")];
        Guid[] itemGuids = [Guid.Parse("36cc710a-d8a6-45d0-a07f-7bbd8742cf02"), Guid.Parse("36cc710a-d8a6-45d0-a07f-7bbd8742cf03")];

        Assert.That(value.Blocks.ContentData, Has.Count.EqualTo(2));
        for (var i = 0; i < value.Blocks.ContentData.Count; i++)
        {
            var item = value.Blocks.ContentData[i];
            Assert.That(item.ContentTypeKey, Is.EqualTo(contentTypeGuids[i]));
            Assert.That(item.Key, Is.EqualTo(itemGuids[i]));
        }

        Assert.That(value.Blocks.SettingsData.Count, Is.EqualTo(0));
    }

    private IJsonSerializer JsonSerializer() => new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());

    private ILogger Logger() => Mock.Of<ILogger>();
}
