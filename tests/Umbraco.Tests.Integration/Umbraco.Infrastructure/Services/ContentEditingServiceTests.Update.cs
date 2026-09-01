using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentEditingServiceTests
{
    [Test]
    public async Task Can_Update_Invariant()
    {
        var content = await CreateInvariantContent();

        var updateModel = new ContentUpdateModel
        {
            Variants =
            [
                new VariantModel { Name = "Updated Name" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The updated title" },
                new PropertyValueModel { Alias = "text", Value = "The updated text" }
            ]
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ContentEditingService.GetAsync(content.Key));

        void VerifyUpdate(IContent? updatedContent)
        {
            Assert.That(updatedContent, Is.Not.Null);
            Assert.That(updatedContent.Name, Is.EqualTo("Updated Name"));
            Assert.That(updatedContent.GetValue<string>("title"), Is.EqualTo("The updated title"));
            Assert.That(updatedContent.GetValue<string>("text"), Is.EqualTo("The updated text"));
        }
    }

    [Test]
    public async Task Can_Update_Culture_Variant()
    {
        var content = await CreateCultureVariantContent();

        var updateModel = new ContentUpdateModel
        {
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The updated invariant title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated English title", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated Danish title", Culture = "da-DK" },
            ],
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "Updated English Name" },
                new VariantModel { Culture = "da-DK", Name = "Updated Danish Name" }
            ]
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ContentEditingService.GetAsync(content.Key));

        void VerifyUpdate(IContent? updatedContent)
        {
            Assert.That(updatedContent, Is.Not.Null);
            Assert.That(updatedContent.GetCultureName("en-US"), Is.EqualTo("Updated English Name"));
            Assert.That(updatedContent.GetCultureName("da-DK"), Is.EqualTo("Updated Danish Name"));
            Assert.That(updatedContent.GetValue<string>("invariantTitle"), Is.EqualTo("The updated invariant title"));
            Assert.That(updatedContent.GetValue<string>("variantTitle", "en-US"), Is.EqualTo("The updated English title"));
            Assert.That(updatedContent.GetValue<string>("variantTitle", "da-DK"), Is.EqualTo("The updated Danish title"));
        }
    }

    [Test]
    public async Task Can_Update_Segment_Variant()
    {
        var content = await CreateSegmentVariantContent();

        var updateModel = new ContentUpdateModel
        {
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The updated invariant title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated default title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated seg-1 title", Segment = "seg-1" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated seg-2 title", Segment = "seg-2" }
            ],
            Variants =
            [
                new VariantModel { Name = "The Updated Name" },
                new VariantModel { Segment = "seg-1", Name = "The Updated Name" },
                new VariantModel { Segment = "seg-2", Name = "The Updated Name" }
            ]
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ContentEditingService.GetAsync(content.Key));

        void VerifyUpdate(IContent? updatedContent)
        {
            Assert.That(updatedContent, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(updatedContent.Name, Is.EqualTo("The Updated Name"));
                Assert.That(updatedContent.GetValue<string>("invariantTitle"), Is.EqualTo("The updated invariant title"));
                Assert.That(updatedContent.GetValue<string>("variantTitle", segment: null), Is.EqualTo("The updated default title"));
                Assert.That(updatedContent.GetValue<string>("variantTitle", segment: "seg-1"), Is.EqualTo("The updated seg-1 title"));
                Assert.That(updatedContent.GetValue<string>("variantTitle", segment: "seg-2"), Is.EqualTo("The updated seg-2 title"));
            });
        }
    }

    [Test]
    public async Task Can_Update_Culture_And_Segment_Variant_With_Culture_Only_Variant_Property()
    {
        var content = await CreateCultureAndSegmentVariantContent(ContentVariation.Culture);

        var updateModel = new ContentUpdateModel
        {
            Properties =
            [
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated default English title", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated seg-1 English title", Culture = "en-US", Segment = "seg-1" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated seg-2 English title", Culture = "en-US", Segment = "seg-2" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated default Danish title", Culture = "da-DK" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated seg-1 Danish title", Culture = "da-DK", Segment = "seg-1" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated seg-2 Danish title", Culture = "da-DK", Segment = "seg-2" },
                new PropertyValueModel { Alias = "otherTitle", Value = "The updated other English title", Culture = "en-US" },
                new PropertyValueModel { Alias = "otherTitle", Value = "The updated other Danish title", Culture = "da-DK" },
            ],
            Variants =
            [
                new VariantModel { Name = "The Updated English Default Name", Culture = "en-US" },
                new VariantModel { Name = "The Updated English Seg-1 Name", Culture = "en-US", Segment = "seg-1" },
                new VariantModel { Name = "The Updated English Seg-2 Name", Culture = "en-US", Segment = "seg-2" },
                new VariantModel { Name = "The Updated Danish Default Name", Culture = "da-DK" },
                new VariantModel { Name = "The Updated Danish Seg-1 Name", Culture = "da-DK", Segment = "seg-1" },
                new VariantModel { Name = "The Updated Danish Seg-2 Name", Culture = "da-DK", Segment = "seg-2" },
            ]
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ContentEditingService.GetAsync(content.Key));

        void VerifyUpdate(IContent? updatedContent)
        {
            Assert.That(updatedContent, Is.Not.Null);
            Assert.Multiple(() =>
            {
                // NOTE: names cannot differ between segments, only between culture - should always prefer segment-less names
                Assert.That(updatedContent.GetCultureName("en-US"), Is.EqualTo("The Updated English Default Name"));
                Assert.That(updatedContent.GetCultureName("da-DK"), Is.EqualTo("The Updated Danish Default Name"));
                Assert.That(updatedContent.GetValue<string>("variantTitle", culture: "en-US", segment: null), Is.EqualTo("The updated default English title"));
                Assert.That(updatedContent.GetValue<string>("variantTitle", culture: "en-US", segment: "seg-1"), Is.EqualTo("The updated seg-1 English title"));
                Assert.That(updatedContent.GetValue<string>("variantTitle", culture: "en-US", segment: "seg-2"), Is.EqualTo("The updated seg-2 English title"));
                Assert.That(updatedContent.GetValue<string>("variantTitle", culture: "da-DK", segment: null), Is.EqualTo("The updated default Danish title"));
                Assert.That(updatedContent.GetValue<string>("variantTitle", culture: "da-DK", segment: "seg-1"), Is.EqualTo("The updated seg-1 Danish title"));
                Assert.That(updatedContent.GetValue<string>("variantTitle", culture: "da-DK", segment: "seg-2"), Is.EqualTo("The updated seg-2 Danish title"));
                Assert.That(updatedContent.GetValue<string>("otherTitle", culture: "en-US", segment: null), Is.EqualTo("The updated other English title"));
                Assert.That(updatedContent.GetValue<string>("otherTitle", culture: "da-DK", segment: null), Is.EqualTo("The updated other Danish title"));
            });
        }
    }

    [Test]
    public async Task Can_Update_Culture_And_Segment_Variant_With_Segment_Only_Variant_Property()
    {
        var content = await CreateCultureAndSegmentVariantContent(ContentVariation.Segment);

        var updateModel = new ContentUpdateModel
        {
            Properties =
            [
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated default English title", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated seg-1 English title", Culture = "en-US", Segment = "seg-1" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated seg-2 English title", Culture = "en-US", Segment = "seg-2" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated default Danish title", Culture = "da-DK" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated seg-1 Danish title", Culture = "da-DK", Segment = "seg-1" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated seg-2 Danish title", Culture = "da-DK", Segment = "seg-2" },
                new PropertyValueModel { Alias = "otherTitle", Value = "The updated other default title", Segment = null },
                new PropertyValueModel { Alias = "otherTitle", Value = "The updated other seg-1 title", Segment = "seg-1" },
                new PropertyValueModel { Alias = "otherTitle", Value = "The updated other seg-2 title", Segment = "seg-2" },
            ],
            Variants =
            [
                new VariantModel { Name = "The Updated English Default Name", Culture = "en-US" },
                new VariantModel { Name = "The Updated English Seg-1 Name", Culture = "en-US", Segment = "seg-1" },
                new VariantModel { Name = "The Updated English Seg-2 Name", Culture = "en-US", Segment = "seg-2" },
                new VariantModel { Name = "The Updated Danish Default Name", Culture = "da-DK" },
                new VariantModel { Name = "The Updated Danish Seg-1 Name", Culture = "da-DK", Segment = "seg-1" },
                new VariantModel { Name = "The Updated Danish Seg-2 Name", Culture = "da-DK", Segment = "seg-2" },
            ]
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ContentEditingService.GetAsync(content.Key));

        void VerifyUpdate(IContent? updatedContent)
        {
            Assert.That(updatedContent, Is.Not.Null);
            Assert.Multiple(() =>
            {
                // NOTE: names cannot differ between segments, only between culture - should always prefer segment-less names
                Assert.That(updatedContent.GetCultureName("en-US"), Is.EqualTo("The Updated English Default Name"));
                Assert.That(updatedContent.GetCultureName("da-DK"), Is.EqualTo("The Updated Danish Default Name"));
                Assert.That(updatedContent.GetValue<string>("variantTitle", culture: "en-US", segment: null), Is.EqualTo("The updated default English title"));
                Assert.That(updatedContent.GetValue<string>("variantTitle", culture: "en-US", segment: "seg-1"), Is.EqualTo("The updated seg-1 English title"));
                Assert.That(updatedContent.GetValue<string>("variantTitle", culture: "en-US", segment: "seg-2"), Is.EqualTo("The updated seg-2 English title"));
                Assert.That(updatedContent.GetValue<string>("variantTitle", culture: "da-DK", segment: null), Is.EqualTo("The updated default Danish title"));
                Assert.That(updatedContent.GetValue<string>("variantTitle", culture: "da-DK", segment: "seg-1"), Is.EqualTo("The updated seg-1 Danish title"));
                Assert.That(updatedContent.GetValue<string>("variantTitle", culture: "da-DK", segment: "seg-2"), Is.EqualTo("The updated seg-2 Danish title"));
                Assert.That(updatedContent.GetValue<string>("otherTitle", culture: null, segment: null), Is.EqualTo("The updated other default title"));
                Assert.That(updatedContent.GetValue<string>("otherTitle", culture: null, segment: "seg-1"), Is.EqualTo("The updated other seg-1 title"));
                Assert.That(updatedContent.GetValue<string>("otherTitle", culture: null, segment: "seg-2"), Is.EqualTo("The updated other seg-2 title"));
            });
        }
    }

    [Test]
    public async Task Can_Update_Template()
    {
        var templateOne = new TemplateBuilder().WithAlias("textPageOne").WithName("Text page one").Build();
        var templateTwo = new TemplateBuilder().WithAlias("textPageTwo").WithName("Text page two").Build();
        await TemplateService.CreateAsync(templateOne, Constants.Security.SuperUserKey);
        await TemplateService.CreateAsync(templateTwo, Constants.Security.SuperUserKey);

        var content = await CreateInvariantContent(templateOne, templateTwo);
        Assert.That(content.TemplateId, Is.EqualTo(templateOne.Id));

        var updateModel = new ContentUpdateModel
        {
            Variants =
            [
                new VariantModel { Name = "Updated Name" }
            ],
            TemplateKey = templateTwo.Key
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, Constants.Security.SuperUserKey);
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ContentEditingService.GetAsync(content.Key));

        void VerifyUpdate(IContent? updatedContent)
        {
            Assert.That(updatedContent, Is.Not.Null);
            Assert.That(updatedContent.Name, Is.EqualTo("Updated Name"));
            Assert.That(updatedContent.TemplateId, Is.EqualTo(templateTwo.Id));
        }
    }

    [Test]
    public async Task Can_Remove_Template()
    {
        var templateOne = new TemplateBuilder().WithAlias("textPageOne").WithName("Text page one").Build();
        await TemplateService.CreateAsync(templateOne, Constants.Security.SuperUserKey);

        var content = await CreateInvariantContent(templateOne);
        Assert.That(content.TemplateId, Is.EqualTo(templateOne.Id));

        var updateModel = new ContentUpdateModel
        {
            Variants =
            [
                new VariantModel { Name = "Updated Name" }
            ],
            TemplateKey = null
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, Constants.Security.SuperUserKey);
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ContentEditingService.GetAsync(content.Key));

        void VerifyUpdate(IContent? updatedContent)
        {
            Assert.That(updatedContent, Is.Not.Null);
            Assert.That(updatedContent.Name, Is.EqualTo("Updated Name"));
            Assert.That(updatedContent.TemplateId, Is.EqualTo(null));
        }
    }

    [Test]
    public async Task Can_Remove_Property_Value()
    {
        var content = await CreateInvariantContent();

        var updateModel = new ContentUpdateModel
        {
            Variants =
            [
                new VariantModel { Name = "Updated Name" }
            ],
            Properties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The updated title" }
            }
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ContentEditingService.GetAsync(content.Key));

        void VerifyUpdate(IContent? updatedContent)
        {
            Assert.That(updatedContent, Is.Not.Null);
            Assert.That(updatedContent.Name, Is.EqualTo("Updated Name"));
            Assert.That(updatedContent.GetValue<string>("title"), Is.EqualTo("The updated title"));
            Assert.That(updatedContent.GetValue<string>("text"), Is.EqualTo(null));
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Update_With_Property_Validation(bool addValidProperties)
    {
        var content = await CreateInvariantContent();
        var contentType = await ContentTypeService.GetAsync(content.ContentType.Key)!;
        contentType.PropertyTypes.First(pt => pt.Alias == "title").Mandatory = true;
        contentType.PropertyTypes.First(pt => pt.Alias == "text").ValidationRegExp = "^\\d*$";
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        var titleValue = addValidProperties ? "The title value" : null;
        var textValue = addValidProperties ? "12345" : "This is not a number";

        var updateModel = new ContentUpdateModel
        {
            Variants =
            [
                new VariantModel { Name = content.Name }
            ],
            Properties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = titleValue },
                new PropertyValueModel { Alias = "text", Value = textValue }
            }
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, Constants.Security.SuperUserKey);

        // success is expected regardless of property level validation - the validation error status is communicated in the attempt status (see below)
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(addValidProperties ? ContentEditingOperationStatus.Success : ContentEditingOperationStatus.PropertyValidationError));
        Assert.That(result.Result, Is.Not.Null);

        if (addValidProperties is false)
        {
            Assert.That(result.Result.ValidationResult.ValidationErrors.Count(), Is.EqualTo(2));
            Assert.That(result.Result.ValidationResult.ValidationErrors.FirstOrDefault(v => v.Alias == "title" && v.ErrorMessages.Length == 1), Is.Not.Null);
            Assert.That(result.Result.ValidationResult.ValidationErrors.FirstOrDefault(v => v.Alias == "text" && v.ErrorMessages.Length == 1), Is.Not.Null);
        }

        // NOTE: content update must be successful, even if the mandatory property is missing (publishing however should not!)
        Assert.That(result.Result.Content!.GetValue<string>("title"), Is.EqualTo(titleValue));
        Assert.That(result.Result.Content!.GetValue<string>("text"), Is.EqualTo(textValue));
    }

    [Test]
    public async Task Cannot_Update_With_Variant_Property_Value_For_Invariant_Content()
    {
        var content = await CreateInvariantContent();

        var updateModel = new ContentUpdateModel
        {
            Variants =
            [
                new VariantModel { Name = "Updated Name" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The updated title" },
                new PropertyValueModel { Alias = "text", Value = "The updated title", Culture = "en-US" }
            ]
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.PropertyTypeCultureVarianceMismatch));

        // re-get and validate
        content = await ContentEditingService.GetAsync(content.Key);
        Assert.That(content, Is.Not.Null);
        Assert.That(content.Name, Is.EqualTo("Initial Name"));
        Assert.That(content.GetValue<string>("title"), Is.EqualTo("The initial title"));
        Assert.That(content.GetValue<string>("text"), Is.EqualTo("The initial text"));
    }

    [Test]
    public async Task Cannot_Update_With_Invariant_Property_Value_For_Variant_Content()
    {
        var content = await CreateCultureVariantContent();

        var updateModel = new ContentUpdateModel
        {
            Variants =
            [
                new VariantModel { Name = "Updated English Name", Culture = "en-US" },
                new VariantModel { Name = "Updated Danish Name", Culture = "da-DK" },
            ],
            Properties = new[]
            {
                new PropertyValueModel { Alias = "invariantTitle", Value = "The updated invariant title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated variant title" }
            }
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.PropertyTypeCultureVarianceMismatch));
        Assert.That(result.Result.Content, Is.Not.Null);

        // re-get and validate
        content = await ContentEditingService.GetAsync(content.Key);
        Assert.That(content, Is.Not.Null);
        Assert.That(content.GetCultureName("en-US"), Is.EqualTo("Initial English Name"));
        Assert.That(content.GetValue<string>("invariantTitle"), Is.EqualTo("The initial invariant title"));
        Assert.That(content.GetValue<string>("variantTitle", "en-US"), Is.EqualTo("The initial English title"));
        Assert.That(content.GetValue<string>("variantTitle", "da-DK"), Is.EqualTo("The initial Danish title"));
    }

    [Test]
    public async Task Cannot_Update_Variant_With_Incorrect_Culture_Casing()
    {
        var content = await CreateCultureVariantContent();

        var updateModel = new ContentUpdateModel
        {
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The updated invariant title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated English title", Culture = "en-us" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated Danish title", Culture = "da-dk" }
            ],
            Variants =
            [
                new VariantModel { Culture = "en-us", Name = "Updated English Name" },
                new VariantModel { Culture = "da-dk", Name = "Updated Danish Name" }
            ]
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.InvalidCulture));
    }

    [Test]
    public async Task Cannot_Update_Invariant_Readonly_Property_Value()
    {
        var content = await CreateInvariantContent();
        content.SetValue("label", "The initial label value");
        ContentService.Save(content);

        var updateModel = new ContentUpdateModel
        {
            Variants =
            [
                new VariantModel { Name = "Updated Name" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "The initial title" },
                new PropertyValueModel { Alias = "label", Value = "The updated label value" }
            ]
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
            Assert.That(result.Result.Content, Is.Not.Null);
        });

        // re-get and validate
        content = await ContentEditingService.GetAsync(content.Key);
        Assert.That(content, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(content.Name, Is.EqualTo("Updated Name"));
            Assert.That(content.GetValue<string>("label"), Is.EqualTo("The initial label value"));
        });
    }

    [Test]
    public async Task Cannot_Update_Variant_Readonly_Property_Value()
    {
        var content = await CreateCultureVariantContent();
        content.SetValue("variantLabel", "The initial English label value", "en-US");
        content.SetValue("variantLabel", "The initial Danish label value", "da-DK");
        ContentService.Save(content);

        var updateModel = new ContentUpdateModel
        {
            Properties =
            [
                new PropertyValueModel { Alias = "invariantTitle", Value = "The updated invariant title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The initial English title", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantLabel", Value = "The updated English label value", Culture = "en-US" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The initial Danish title", Culture = "da-DK" },
                new PropertyValueModel { Alias = "variantLabel", Value = "The updated Danish label value", Culture = "da-DK" }
            ],
            Variants =
            [
                new VariantModel
                {
                    Culture = "en-US",
                    Name = "Updated English Name"
                },
                new VariantModel
                {
                    Culture = "da-DK",
                    Name = "Updated Danish Name"
                }
            ]
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
            Assert.That(result.Result.Content, Is.Not.Null);
        });

        // re-get and validate
        content = await ContentEditingService.GetAsync(content.Key);
        Assert.That(content, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(content.GetCultureName("en-US"), Is.EqualTo("Updated English Name"));
            Assert.That(content.GetCultureName("da-DK"), Is.EqualTo("Updated Danish Name"));
            Assert.That(content.GetValue<string>("variantLabel", "en-US"), Is.EqualTo("The initial English label value"));
            Assert.That(content.GetValue<string>("variantLabel", "da-DK"), Is.EqualTo("The initial Danish label value"));
        });
    }

    [Test]
    public async Task Updating_Single_Variant_Name_Does_Not_Change_Update_Dates_Of_Other_Variants()
    {
        var contentType = await CreateVariantContentType(variantTitleAsMandatory: false);

        var createModel = new ContentCreateModel
        {
            ContentTypeKey = contentType.Key,
            ParentKey = Constants.System.RootKey,
            Properties = [],
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "Initial English Name" },
                new VariantModel { Culture = "da-DK", Name = "Initial Danish Name" }
            ],
        };

        var createResult = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(createResult.Success, Is.True);
        Assert.That(createResult.Result.Content, Is.Not.Null);

        // Retrieve the actual stored content to ensure the database truncation of times isn't interfering with the test
        var createdContent = await ContentEditingService.GetAsync(createResult.Result.Content.Key);
        Assert.That(createdContent, Is.Not.Null);
        var firstUpdateDateEn = createdContent.GetUpdateDate("en-US");
        Assert.That(firstUpdateDateEn, Is.Not.Null);
        var firstUpdateDateDa = createdContent.GetUpdateDate("da-DK");
        Assert.That(firstUpdateDateDa, Is.Not.Null);

        await Task.Delay(100);

        var updateModel = new ContentUpdateModel
        {
            Properties = [],
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = "Updated English Name" },
                new VariantModel { Culture = "da-DK", Name = "Initial Danish Name" }
            ]
        };

        var updateResult = await ContentEditingService.UpdateAsync(createResult.Result.Content.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.That(updateResult.Success, Is.True);
        Assert.That(updateResult.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        VerifyUpdate(updateResult.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ContentEditingService.GetAsync(updateResult.Result.Content!.Key));
        return;

        void VerifyUpdate(IContent? updatedContent)
        {
            Assert.That(updatedContent, Is.Not.Null);
            Assert.That(updatedContent.GetUpdateDate("da-DK"), Is.EqualTo(firstUpdateDateDa));
            Assert.That(firstUpdateDateEn, Is.LessThan(updatedContent.GetUpdateDate("en-US")));
        }
    }

    [Test]
    public async Task Updating_Single_Variant_Property_Does_Not_Change_Update_Dates_Of_Other_Variants()
    {
        var content = await CreateCultureVariantContent();

        // Retrieve the actual stored content to ensure the database truncation of times isn't interfering with the test
        var createdContent = await ContentEditingService.GetAsync(content.Key);
        Assert.That(createdContent, Is.Not.Null);
        var firstUpdateDateEn = createdContent.GetUpdateDate("en-US");
        Assert.That(firstUpdateDateEn, Is.Not.Null);
        var firstUpdateDateDa = createdContent.GetUpdateDate("da-DK");
        Assert.That(firstUpdateDateDa, Is.Not.Null);

        await Task.Delay(100);

        var updateModel = new ContentUpdateModel
        {
            Properties =
            [
                new PropertyValueModel
                {
                    Alias = "invariantTitle",
                    Value = "The invariant title"
                },
                new PropertyValueModel
                {
                    Culture = "en-US",
                    Alias = "variantTitle",
                    Value = content.GetValue<string>("variantTitle", "en-US")!
                },
                new PropertyValueModel
                {
                    Culture = "da-DK",
                    Alias = "variantTitle",
                    Value = "The updated Danish title"
                }
            ],
            Variants =
            [
                new VariantModel { Culture = "en-US", Name = content.GetCultureName("en-US")! },
                new VariantModel { Culture = "da-DK", Name = content.GetCultureName("da-DK")! }
            ]
        };

        var result = await ContentEditingService.UpdateAsync(content.Key, updateModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentEditingOperationStatus.Success));
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ContentEditingService.GetAsync(content.Key));
        return;

        void VerifyUpdate(IContent? updatedContent)
        {
            Assert.That(updatedContent, Is.Not.Null);
            Assert.That(updatedContent.GetUpdateDate("en-US"), Is.EqualTo(firstUpdateDateEn));
            Assert.That(firstUpdateDateDa, Is.LessThan(updatedContent.GetUpdateDate("da-DK")));
        }
    }
}
