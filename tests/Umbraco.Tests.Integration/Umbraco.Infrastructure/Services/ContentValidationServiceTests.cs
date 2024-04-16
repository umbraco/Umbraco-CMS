using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true)]
public class ContentValidationServiceTests : UmbracoIntegrationTestWithContent
{
    private IContentValidationService ContentValidationService => GetRequiredService<IContentValidationService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    [Test]
    public async Task Can_Validate_Block_List_Nested_In_Block_List()
    {
        var setup = await SetupBlockListTest();

        var validationResult = await ContentValidationService.ValidatePropertiesAsync(
            new ContentCreateModel
            {
                ContentTypeKey = setup.DocumentType.Key,
                InvariantName = "Test Document",
                InvariantProperties = new[]
                {
                    new PropertyValueModel
                    {
                        Alias = "blocks",
                        Value = $$"""
                                {
                                  "layout": {
                                    "Umbraco.BlockList": [{
                                        "contentUdi": "umb://element/9addc377c02c4db088c273b933704f7b",
                                        "settingsUdi": "umb://element/65db1ecd78e041a584f07296123a0a73"
                                      }, {
                                        "contentUdi": "umb://element/3af93b5b5e404c64b1422564309fc4c7",
                                        "settingsUdi": "umb://element/efb9583ce67043f282fb2a0cb0f3e736"
                                      }
                                    ]
                                  },
                                  "contentData": [{
                                      "contentTypeKey": "{{setup.ElementType.Key}}",
                                      "udi": "umb://element/9addc377c02c4db088c273b933704f7b",
                                      "title": "Valid root content",
                                      "blocks": {
                                        "layout": {
                                          "Umbraco.BlockList": [{
                                              "contentUdi": "umb://element/f36cebfad03b44519e604bf32c5b1e2f",
                                              "settingsUdi": "umb://element/c9129a4671bb4b4e8f0ad525ad4a5de3"
                                            }, {
                                              "contentUdi": "umb://element/b8173e4a0618475c8277c3c6af68bee6",
                                              "settingsUdi": "umb://element/77f7ea3507664395bf7f0c9df04530f7"
                                            }
                                          ]
                                        },
                                        "contentData": [{
                                            "contentTypeKey": "{{setup.ElementType.Key}}",
                                            "udi": "umb://element/f36cebfad03b44519e604bf32c5b1e2f",
                                            "title": "Invalid nested content"
                                          }, {
                                            "contentTypeKey": "{{setup.ElementType.Key}}",
                                            "udi": "umb://element/b8173e4a0618475c8277c3c6af68bee6",
                                            "title": "Valid nested content"
                                          }
                                        ],
                                        "settingsData": [{
                                            "contentTypeKey": "{{setup.ElementType.Key}}",
                                            "udi": "umb://element/c9129a4671bb4b4e8f0ad525ad4a5de3",
                                            "title": "Valid nested setting"
                                          }, {
                                            "contentTypeKey": "{{setup.ElementType.Key}}",
                                            "udi": "umb://element/77f7ea3507664395bf7f0c9df04530f7",
                                            "title": "Invalid nested setting"
                                          }
                                        ]
                                      }
                                    }, {
                                      "contentTypeKey": "{{setup.ElementType.Key}}",
                                      "udi": "umb://element/3af93b5b5e404c64b1422564309fc4c7",
                                      "title": "Invalid root content"
                                    }
                                  ],
                                  "settingsData": [{
                                        "contentTypeKey": "{{setup.ElementType.Key}}",
                                        "udi": "umb://element/65db1ecd78e041a584f07296123a0a73",
                                        "title": "Invalid root setting"
                                      }, {
                                        "contentTypeKey": "{{setup.ElementType.Key}}",
                                        "udi": "umb://element/efb9583ce67043f282fb2a0cb0f3e736",
                                        "title": "Valid root setting"
                                      }
                                  ]
                                }
                                """
                    }
                }
            },
            setup.DocumentType);

        Assert.AreEqual(4, validationResult.ValidationErrors.Count());
        Assert.IsNotNull(validationResult.ValidationErrors.SingleOrDefault(r => r.Alias == "blocks" && r.JsonPath == ".contentData[0].blocks.contentData[0].title"));
        Assert.IsNotNull(validationResult.ValidationErrors.SingleOrDefault(r => r.Alias == "blocks" && r.JsonPath == ".contentData[0].blocks.settingsData[1].title"));
        Assert.IsNotNull(validationResult.ValidationErrors.SingleOrDefault(r => r.Alias == "blocks" && r.JsonPath == ".contentData[1].title"));
        Assert.IsNotNull(validationResult.ValidationErrors.SingleOrDefault(r => r.Alias == "blocks" && r.JsonPath == ".settingsData[0].title"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Validate_RegEx_For_Simple_Property_On_Document(bool valid)
    {
        var contentType = SetupSimpleTest();

        var validationResult = await ContentValidationService.ValidatePropertiesAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                InvariantName = "Test Document",
                InvariantProperties = new[]
                {
                    new PropertyValueModel
                    {
                        Alias = "title",
                        Value = "The title value"
                    },
                    new PropertyValueModel
                    {
                        Alias = "author",
                        Value = valid ? "Valid value" : "Invalid value"
                    }
                }
            },
            contentType);

        if (valid)
        {
            Assert.IsEmpty(validationResult.ValidationErrors);
        }
        else
        {
            Assert.AreEqual(1, validationResult.ValidationErrors.Count());
            Assert.IsNotNull(validationResult.ValidationErrors.SingleOrDefault(r => r.Alias == "author" && r.JsonPath == string.Empty));
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Validate_Mandatory_For_Simple_Property_On_Document(bool valid)
    {
        var contentType = SetupSimpleTest();

        var validationResult = await ContentValidationService.ValidatePropertiesAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                InvariantName = "Test Document",
                InvariantProperties = new[]
                {
                    new PropertyValueModel
                    {
                        Alias = "title",
                        Value = valid ? "A value" : string.Empty
                    },
                    new PropertyValueModel
                    {
                        Alias = "author",
                        Value = "Valid value"
                    }
                }
            },
            contentType);

        if (valid)
        {
            Assert.IsEmpty(validationResult.ValidationErrors);
        }
        else
        {
            Assert.AreEqual(1, validationResult.ValidationErrors.Count());
            Assert.IsNotNull(validationResult.ValidationErrors.SingleOrDefault(r => r.Alias == "title" && r.JsonPath == string.Empty));
        }
    }

    [Test]
    public async Task Can_Validate_Mandatory_For_Property_Not_Present_In_Document()
    {
        var contentType = SetupSimpleTest();

        var validationResult = await ContentValidationService.ValidatePropertiesAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                InvariantName = "Test Document",
                InvariantProperties = new[]
                {
                    new PropertyValueModel
                    {
                        Alias = "author",
                        Value = "Valid value"
                    }
                }
            },
            contentType);

        Assert.AreEqual(1, validationResult.ValidationErrors.Count());
        Assert.IsNotNull(validationResult.ValidationErrors.SingleOrDefault(r => r.Alias == "title" && r.JsonPath == string.Empty));
    }

    [Test]
    public async Task Uses_Localizaton_Keys_For_Validation_Error_Messages()
    {
        var contentType = SetupSimpleTest();

        var validationResult = await ContentValidationService.ValidatePropertiesAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                InvariantName = "Test Document",
                InvariantProperties = new[]
                {
                    new PropertyValueModel
                    {
                        Alias = "author",
                        Value = "Invalid value"
                    }
                }
            },
            contentType);

        Assert.AreEqual(2, validationResult.ValidationErrors.Count());
        Assert.IsNotNull(validationResult.ValidationErrors.SingleOrDefault(
            r => r.Alias == "title"
                 && r.ErrorMessages.Length == 1
                 && r.ErrorMessages.First() == Constants.Validation.ErrorMessages.Properties.Missing));
        Assert.IsNotNull(validationResult.ValidationErrors.SingleOrDefault(
            r => r.Alias == "author"
                 && r.ErrorMessages.Length == 1
                 && r.ErrorMessages.First() == Constants.Validation.ErrorMessages.Properties.PatternMismatch));
    }

    [Test]
    public async Task Custom_Validation_Error_Messages_Replaces_Localizaton_Keys()
    {
        var contentType = SetupSimpleTest();
        contentType.PropertyTypes.First(pt => pt.Alias == "title").MandatoryMessage = "Custom mandatory message";
        contentType.PropertyTypes.First(pt => pt.Alias == "author").ValidationRegExpMessage = "Custom regex message";
        ContentTypeService.Save(contentType);

        var validationResult = await ContentValidationService.ValidatePropertiesAsync(
            new ContentCreateModel
            {
                ContentTypeKey = contentType.Key,
                InvariantName = "Test Document",
                InvariantProperties = new[]
                {
                    new PropertyValueModel
                    {
                        Alias = "author",
                        Value = "Invalid value"
                    }
                }
            },
            contentType);

        Assert.AreEqual(2, validationResult.ValidationErrors.Count());
        Assert.IsNotNull(validationResult.ValidationErrors.SingleOrDefault(
            r => r.Alias == "title"
                 && r.ErrorMessages.Length == 1
                 && r.ErrorMessages.First() == "Custom mandatory message"));
        Assert.IsNotNull(validationResult.ValidationErrors.SingleOrDefault(
            r => r.Alias == "author"
                 && r.ErrorMessages.Length == 1
                 && r.ErrorMessages.First() == "Custom regex message"));
    }

    [TestCase("en-US", true)]
    [TestCase("en-us", false)]
    [TestCase("da-DK", true)]
    [TestCase("da-dk", false)]
    [TestCase("de-DE", false)]
    [TestCase("de-de", false)]
    public async Task Can_Validate_Culture_Code(string cultureCode, bool expectedResult)
    {
        var language = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        await LanguageService.CreateAsync(language, Constants.Security.SuperUserKey);

        var result = await ContentValidationService.ValidateCulturesAsync(
            new ContentCreateModel
            {
                Variants = new []
                {
                    new VariantModel
                    {
                        Culture = cultureCode,
                        Name = "Whatever",
                        Properties = new []
                        {
                            new PropertyValueModel { Alias = "title", Value = "Something" }
                        }
                    }
                }
            });

        Assert.AreEqual(expectedResult, result);
    }

    private async Task<(IContentType DocumentType, IContentType ElementType)> SetupBlockListTest()
    {
        var propertyEditorCollection = GetRequiredService<PropertyEditorCollection>();
        if (propertyEditorCollection.TryGet(Constants.PropertyEditors.Aliases.BlockList, out IDataEditor dataEditor) is false)
        {
            Assert.Fail("Could not get the Block List data editor");
        }

        var elementType = new ContentType(ShortStringHelper, Constants.System.Root)
        {
            Name = "Test Element Type", Alias = "testElementType", IsElement = true
        };
        await ContentTypeService.SaveAsync(elementType, Constants.Security.SuperUserKey);
        Assert.IsTrue(elementType.HasIdentity, "Could not create the element type");

        var configurationEditorJsonSerializer = GetRequiredService<IConfigurationEditorJsonSerializer>();
        IDataType blockListDataType = new DataType(dataEditor, configurationEditorJsonSerializer)
        {
            Name = "Test Block List",
            ParentId = Constants.System.Root,
            DatabaseType = ValueTypes.ToStorageType(dataEditor.GetValueEditor().ValueType),
            ConfigurationData = new Dictionary<string, object>
            {
                {
                    nameof(BlockListConfiguration.Blocks).ToFirstLowerInvariant(),
                    new[]
                    {
                        new BlockListConfiguration
                        {
                            Blocks = new[]
                            {
                                new BlockListConfiguration.BlockConfiguration
                                {
                                    ContentElementTypeKey = elementType.Key,
                                    SettingsElementTypeKey = elementType.Key
                                }
                            }
                        }
                    }
                }
            }
        };

        var dataTypeService = GetRequiredService<IDataTypeService>();
        var dataTypeCreateResult = await dataTypeService.CreateAsync(blockListDataType, Constants.Security.SuperUserKey);
        Assert.IsTrue(dataTypeCreateResult.Success, "Could not create the block list data type");

        blockListDataType = dataTypeCreateResult.Result;

        // add the block list and a regex validated text box to the element type
        elementType.AddPropertyType(new PropertyType(ShortStringHelper, blockListDataType, "blocks"));
        var textBoxDataType = await dataTypeService.GetAsync("Textstring");
        Assert.IsNotNull(textBoxDataType, "Could not get the default TextBox data type");
        elementType.AddPropertyType(new PropertyType(ShortStringHelper, textBoxDataType, "title")
        {
            ValidationRegExp = "^Valid.*$"
        });
        await ContentTypeService.SaveAsync(elementType, Constants.Security.SuperUserKey);

        // create a document type with the block list and a regex validated text box
        var documentType = new ContentType(ShortStringHelper, Constants.System.Root)
        {
            Name = "Test Document Type", Alias = "testDocumentType", IsElement = false, AllowedAsRoot = true
        };
        documentType.AddPropertyType(new PropertyType(ShortStringHelper, blockListDataType, "blocks"));
        await ContentTypeService.SaveAsync(documentType, Constants.Security.SuperUserKey);
        Assert.IsTrue(documentType.HasIdentity, "Could not create the document type");

        return (documentType, elementType);
    }

    private IContentType SetupSimpleTest()
    {
        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type");
        contentType.PropertyTypes.First(pt => pt.Alias == "title").Mandatory = true;
        contentType.PropertyTypes.First(pt => pt.Alias == "author").ValidationRegExp = "^Valid.*$";
        contentType.AllowedAsRoot = true;
        ContentTypeService.Save(contentType);

        return contentType;
    }
}
