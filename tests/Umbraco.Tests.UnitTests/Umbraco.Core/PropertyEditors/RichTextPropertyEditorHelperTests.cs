using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
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
        var input = JObject.Parse(""""
                                  {
                                   "markup": "<p>this is some markup</p><umb-rte-block data-content-udi=\"umb://element/36cc710ad8a645d0a07f7bbd8742cf02\"><!--Umbraco-Block--></umb-rte-block>",
                                   "blocks": {
                                       "layout": {
                                           "Umbraco.TinyMCE": [{
                                                   "contentUdi": "umb://element/36cc710ad8a645d0a07f7bbd8742cf02",
                                                   "settingsUdi": "umb://element/d2eeef66411142f4a1647a523eaffbc2",
                                               }
                                           ]
                                       },
                                       "contentData": [{
                                               "contentTypeKey": "b2f0806c-d231-4c78-88b2-3c97d26e1123",
                                               "udi": "umb://element/36cc710ad8a645d0a07f7bbd8742cf02",
                                               "contentPropertyAlias": "A content property value"
                                             }
                                         ],
                                         "settingsData": [{
                                                 "contentTypeKey": "e7a9447f-e14d-44dd-9ae8-e68c3c3da598",
                                                 "udi": "umb://element/d2eeef66411142f4a1647a523eaffbc2",
                                                 "settingsPropertyAlias": "A settings property value"
                                             }
                                         ]
                                     }
                                  }
                                  """");

        var result = RichTextPropertyEditorHelper.TryParseRichTextEditorValue(input, JsonSerializer(), Logger(), out RichTextEditorValue? value);
        Assert.IsTrue(result);
        Assert.IsNotNull(value);
        Assert.AreEqual("<p>this is some markup</p><umb-rte-block data-content-udi=\"umb://element/36cc710ad8a645d0a07f7bbd8742cf02\"><!--Umbraco-Block--></umb-rte-block>", value.Markup);

        Assert.IsNotNull(value.Blocks);

        Assert.AreEqual(1, value.Blocks.ContentData.Count);
        var item = value.Blocks.ContentData.Single();
        var contentTypeGuid = Guid.Parse("b2f0806c-d231-4c78-88b2-3c97d26e1123");
        var itemGuid = Guid.Parse("36cc710a-d8a6-45d0-a07f-7bbd8742cf02");
        Assert.AreEqual(contentTypeGuid, item.ContentTypeKey);
        Assert.AreEqual(new GuidUdi(Constants.UdiEntityType.Element, itemGuid), item.Udi);
        Assert.AreEqual(itemGuid, item.Key);

        Assert.AreEqual(1, value.Blocks.SettingsData.Count);
        item = value.Blocks.SettingsData.Single();
        contentTypeGuid = Guid.Parse("e7a9447f-e14d-44dd-9ae8-e68c3c3da598");
        itemGuid = Guid.Parse("d2eeef66-4111-42f4-a164-7a523eaffbc2");
        Assert.AreEqual(contentTypeGuid, item.ContentTypeKey);
        Assert.AreEqual(new GuidUdi(Constants.UdiEntityType.Element, itemGuid), item.Udi);
        Assert.AreEqual(itemGuid, item.Key);
    }

    [Test]
    public void Can_Parse_Blocks_With_Both_Content_And_Settings()
    {
        const string input = """
                             {
                              "markup": "<p>this is some markup</p><umb-rte-block data-content-udi=\"umb://element/36cc710ad8a645d0a07f7bbd8742cf02\"><!--Umbraco-Block--></umb-rte-block>",
                              "blocks": {
                                  "layout": {
                                      "Umbraco.TinyMCE": [{
                                              "contentUdi": "umb://element/36cc710ad8a645d0a07f7bbd8742cf02",
                                              "settingsUdi": "umb://element/d2eeef66411142f4a1647a523eaffbc2",
                                          }
                                      ]
                                  },
                                  "contentData": [{
                                          "contentTypeKey": "b2f0806c-d231-4c78-88b2-3c97d26e1123",
                                          "udi": "umb://element/36cc710ad8a645d0a07f7bbd8742cf02",
                                          "contentPropertyAlias": "A content property value"
                                        }
                                    ],
                                    "settingsData": [{
                                            "contentTypeKey": "e7a9447f-e14d-44dd-9ae8-e68c3c3da598",
                                            "udi": "umb://element/d2eeef66411142f4a1647a523eaffbc2",
                                            "settingsPropertyAlias": "A settings property value"
                                        }
                                    ]
                                }
                             }
                             """;

        var result = RichTextPropertyEditorHelper.TryParseRichTextEditorValue(input, JsonSerializer(), Logger(), out RichTextEditorValue? value);
        Assert.IsTrue(result);
        Assert.IsNotNull(value);
        Assert.AreEqual("<p>this is some markup</p><umb-rte-block data-content-udi=\"umb://element/36cc710ad8a645d0a07f7bbd8742cf02\"><!--Umbraco-Block--></umb-rte-block>", value.Markup);

        Assert.IsNotNull(value.Blocks);

        Assert.AreEqual(1, value.Blocks.ContentData.Count);
        var item = value.Blocks.ContentData.Single();
        var contentTypeGuid = Guid.Parse("b2f0806c-d231-4c78-88b2-3c97d26e1123");
        var itemGuid = Guid.Parse("36cc710a-d8a6-45d0-a07f-7bbd8742cf02");
        Assert.AreEqual(contentTypeGuid, item.ContentTypeKey);
        Assert.AreEqual(new GuidUdi(Constants.UdiEntityType.Element, itemGuid), item.Udi);
        Assert.AreEqual(itemGuid, item.Key);

        Assert.AreEqual(1, value.Blocks.SettingsData.Count);
        item = value.Blocks.SettingsData.Single();
        contentTypeGuid = Guid.Parse("e7a9447f-e14d-44dd-9ae8-e68c3c3da598");
        itemGuid = Guid.Parse("d2eeef66-4111-42f4-a164-7a523eaffbc2");
        Assert.AreEqual(contentTypeGuid, item.ContentTypeKey);
        Assert.AreEqual(new GuidUdi(Constants.UdiEntityType.Element, itemGuid), item.Udi);
        Assert.AreEqual(itemGuid, item.Key);
    }

    [Test]
    public void Can_Parse_Blocks_With_Content_Only()
    {
        const string input = """
                             {
                              "markup": "<p>this is some markup</p><umb-rte-block data-content-udi=\"umb://element/36cc710ad8a645d0a07f7bbd8742cf02\"><!--Umbraco-Block--></umb-rte-block>",
                              "blocks": {
                                  "layout": {
                                      "Umbraco.TinyMCE": [{
                                              "contentUdi": "umb://element/36cc710ad8a645d0a07f7bbd8742cf02"
                                          }
                                      ]
                                  },
                                  "contentData": [{
                                          "contentTypeKey": "b2f0806c-d231-4c78-88b2-3c97d26e1123",
                                          "udi": "umb://element/36cc710ad8a645d0a07f7bbd8742cf02",
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
        Assert.AreEqual("<p>this is some markup</p><umb-rte-block data-content-udi=\"umb://element/36cc710ad8a645d0a07f7bbd8742cf02\"><!--Umbraco-Block--></umb-rte-block>", value.Markup);

        Assert.IsNotNull(value.Blocks);

        Assert.AreEqual(1, value.Blocks.ContentData.Count);
        var item = value.Blocks.ContentData.Single();
        var contentTypeGuid = Guid.Parse("b2f0806c-d231-4c78-88b2-3c97d26e1123");
        var itemGuid = Guid.Parse("36cc710a-d8a6-45d0-a07f-7bbd8742cf02");
        Assert.AreEqual(contentTypeGuid, item.ContentTypeKey);
        Assert.AreEqual(new GuidUdi(Constants.UdiEntityType.Element, itemGuid), item.Udi);
        Assert.AreEqual(itemGuid, item.Key);

        Assert.AreEqual(0, value.Blocks.SettingsData.Count);
    }

    private IJsonSerializer JsonSerializer() => new JsonNetSerializer();

    private ILogger Logger() => Mock.Of<ILogger>();
}
