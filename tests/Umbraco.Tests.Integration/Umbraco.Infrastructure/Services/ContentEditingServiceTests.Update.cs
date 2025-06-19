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
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ContentEditingService.GetAsync(content.Key));

        void VerifyUpdate(IContent? updatedContent)
        {
            Assert.IsNotNull(updatedContent);
            Assert.AreEqual("Updated Name", updatedContent.Name);
            Assert.AreEqual("The updated title", updatedContent.GetValue<string>("title"));
            Assert.AreEqual("The updated text", updatedContent.GetValue<string>("text"));
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
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ContentEditingService.GetAsync(content.Key));

        void VerifyUpdate(IContent? updatedContent)
        {
            Assert.IsNotNull(updatedContent);
            Assert.AreEqual("Updated English Name", updatedContent.GetCultureName("en-US"));
            Assert.AreEqual("Updated Danish Name", updatedContent.GetCultureName("da-DK"));
            Assert.AreEqual("The updated invariant title", updatedContent.GetValue<string>("invariantTitle"));
            Assert.AreEqual("The updated English title", updatedContent.GetValue<string>("variantTitle", "en-US"));
            Assert.AreEqual("The updated Danish title", updatedContent.GetValue<string>("variantTitle", "da-DK"));
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
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ContentEditingService.GetAsync(content.Key));

        void VerifyUpdate(IContent? updatedContent)
        {
            Assert.IsNotNull(updatedContent);
            Assert.Multiple(() =>
            {
                Assert.AreEqual("The Updated Name", updatedContent.Name);
                Assert.AreEqual("The updated invariant title", updatedContent.GetValue<string>("invariantTitle"));
                Assert.AreEqual("The updated default title", updatedContent.GetValue<string>("variantTitle", segment: null));
                Assert.AreEqual("The updated seg-1 title", updatedContent.GetValue<string>("variantTitle", segment: "seg-1"));
                Assert.AreEqual("The updated seg-2 title", updatedContent.GetValue<string>("variantTitle", segment: "seg-2"));
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
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ContentEditingService.GetAsync(content.Key));

        void VerifyUpdate(IContent? updatedContent)
        {
            Assert.IsNotNull(updatedContent);
            Assert.Multiple(() =>
            {
                // NOTE: names cannot differ between segments, only between culture - should always prefer segment-less names
                Assert.AreEqual("The Updated English Default Name", updatedContent.GetCultureName("en-US"));
                Assert.AreEqual("The Updated Danish Default Name", updatedContent.GetCultureName("da-DK"));
                Assert.AreEqual("The updated default English title", updatedContent.GetValue<string>("variantTitle", culture: "en-US", segment: null));
                Assert.AreEqual("The updated seg-1 English title", updatedContent.GetValue<string>("variantTitle", culture: "en-US", segment: "seg-1"));
                Assert.AreEqual("The updated seg-2 English title", updatedContent.GetValue<string>("variantTitle", culture: "en-US", segment: "seg-2"));
                Assert.AreEqual("The updated default Danish title", updatedContent.GetValue<string>("variantTitle", culture: "da-DK", segment: null));
                Assert.AreEqual("The updated seg-1 Danish title", updatedContent.GetValue<string>("variantTitle", culture: "da-DK", segment: "seg-1"));
                Assert.AreEqual("The updated seg-2 Danish title", updatedContent.GetValue<string>("variantTitle", culture: "da-DK", segment: "seg-2"));
                Assert.AreEqual("The updated other English title", updatedContent.GetValue<string>("otherTitle", culture: "en-US", segment: null));
                Assert.AreEqual("The updated other Danish title", updatedContent.GetValue<string>("otherTitle", culture: "da-DK", segment: null));
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
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ContentEditingService.GetAsync(content.Key));

        void VerifyUpdate(IContent? updatedContent)
        {
            Assert.IsNotNull(updatedContent);
            Assert.Multiple(() =>
            {
                // NOTE: names cannot differ between segments, only between culture - should always prefer segment-less names
                Assert.AreEqual("The Updated English Default Name", updatedContent.GetCultureName("en-US"));
                Assert.AreEqual("The Updated Danish Default Name", updatedContent.GetCultureName("da-DK"));
                Assert.AreEqual("The updated default English title", updatedContent.GetValue<string>("variantTitle", culture: "en-US", segment: null));
                Assert.AreEqual("The updated seg-1 English title", updatedContent.GetValue<string>("variantTitle", culture: "en-US", segment: "seg-1"));
                Assert.AreEqual("The updated seg-2 English title", updatedContent.GetValue<string>("variantTitle", culture: "en-US", segment: "seg-2"));
                Assert.AreEqual("The updated default Danish title", updatedContent.GetValue<string>("variantTitle", culture: "da-DK", segment: null));
                Assert.AreEqual("The updated seg-1 Danish title", updatedContent.GetValue<string>("variantTitle", culture: "da-DK", segment: "seg-1"));
                Assert.AreEqual("The updated seg-2 Danish title", updatedContent.GetValue<string>("variantTitle", culture: "da-DK", segment: "seg-2"));
                Assert.AreEqual("The updated other default title", updatedContent.GetValue<string>("otherTitle", culture: null, segment: null));
                Assert.AreEqual("The updated other seg-1 title", updatedContent.GetValue<string>("otherTitle", culture: null, segment: "seg-1"));
                Assert.AreEqual("The updated other seg-2 title", updatedContent.GetValue<string>("otherTitle", culture: null, segment: "seg-2"));
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
        Assert.AreEqual(templateOne.Id, content.TemplateId);

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
            Assert.IsNotNull(updatedContent);
            Assert.AreEqual("Updated Name", updatedContent.Name);
            Assert.AreEqual(templateTwo.Id, updatedContent.TemplateId);
        }
    }

    [Test]
    public async Task Can_Remove_Template()
    {
        var templateOne = new TemplateBuilder().WithAlias("textPageOne").WithName("Text page one").Build();
        await TemplateService.CreateAsync(templateOne, Constants.Security.SuperUserKey);

        var content = await CreateInvariantContent(templateOne);
        Assert.AreEqual(templateOne.Id, content.TemplateId);

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
            Assert.IsNotNull(updatedContent);
            Assert.AreEqual("Updated Name", updatedContent.Name);
            Assert.AreEqual(null, updatedContent.TemplateId);
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
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ContentEditingService.GetAsync(content.Key));

        void VerifyUpdate(IContent? updatedContent)
        {
            Assert.IsNotNull(updatedContent);
            Assert.AreEqual("Updated Name", updatedContent.Name);
            Assert.AreEqual("The updated title", updatedContent.GetValue<string>("title"));
            Assert.AreEqual(null, updatedContent.GetValue<string>("text"));
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
        await ContentTypeService.SaveAsync(contentType, Constants.Security.SuperUserKey);

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
        Assert.IsTrue(result.Success);
        Assert.AreEqual(addValidProperties ? ContentEditingOperationStatus.Success : ContentEditingOperationStatus.PropertyValidationError, result.Status);
        Assert.IsNotNull(result.Result);

        if (addValidProperties is false)
        {
            Assert.AreEqual(2, result.Result.ValidationResult.ValidationErrors.Count());
            Assert.IsNotNull(result.Result.ValidationResult.ValidationErrors.FirstOrDefault(v => v.Alias == "title" && v.ErrorMessages.Length == 1));
            Assert.IsNotNull(result.Result.ValidationResult.ValidationErrors.FirstOrDefault(v => v.Alias == "text" && v.ErrorMessages.Length == 1));
        }

        // NOTE: content update must be successful, even if the mandatory property is missing (publishing however should not!)
        Assert.AreEqual(titleValue, result.Result.Content!.GetValue<string>("title"));
        Assert.AreEqual(textValue, result.Result.Content!.GetValue<string>("text"));
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
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.PropertyTypeNotFound, result.Status);

        // re-get and validate
        content = await ContentEditingService.GetAsync(content.Key);
        Assert.IsNotNull(content);
        Assert.AreEqual("Initial Name", content.Name);
        Assert.AreEqual("The initial title", content.GetValue<string>("title"));
        Assert.AreEqual("The initial text", content.GetValue<string>("text"));
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
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.PropertyTypeNotFound, result.Status);
        Assert.IsNotNull(result.Result.Content);

        // re-get and validate
        content = await ContentEditingService.GetAsync(content.Key);
        Assert.IsNotNull(content);
        Assert.AreEqual("Initial English Name", content.GetCultureName("en-US"));
        Assert.AreEqual("The initial invariant title", content.GetValue<string>("invariantTitle"));
        Assert.AreEqual("The initial English title", content.GetValue<string>("variantTitle", "en-US"));
        Assert.AreEqual("The initial Danish title", content.GetValue<string>("variantTitle", "da-DK"));
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
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.InvalidCulture, result.Status);
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
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
            Assert.IsNotNull(result.Result.Content);
        });

        // re-get and validate
        content = await ContentEditingService.GetAsync(content.Key);
        Assert.IsNotNull(content);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("Updated Name", content.Name);
            Assert.AreEqual("The initial label value", content.GetValue<string>("label"));
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
            Assert.IsTrue(result.Success);
            Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
            Assert.IsNotNull(result.Result.Content);
        });

        // re-get and validate
        content = await ContentEditingService.GetAsync(content.Key);
        Assert.IsNotNull(content);
        Assert.Multiple(() =>
        {
            Assert.AreEqual("Updated English Name", content.GetCultureName("en-US"));
            Assert.AreEqual("Updated Danish Name", content.GetCultureName("da-DK"));
            Assert.AreEqual("The initial English label value", content.GetValue<string>("variantLabel", "en-US"));
            Assert.AreEqual("The initial Danish label value", content.GetValue<string>("variantLabel", "da-DK"));
        });
    }

    [Test]
    public async Task Updating_Single_Variant_Name_Does_Not_Change_Update_Dates_Of_Other_Vaiants()
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
        Assert.IsTrue(createResult.Success);

        var firstUpdateDateEn = createResult.Result.Content!.GetUpdateDate("en-US")!;
        var firstUpdateDateDa = createResult.Result.Content!.GetUpdateDate("da-DK")!;

        Thread.Sleep(100);

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
        Assert.IsTrue(updateResult.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, updateResult.Status);
        VerifyUpdate(updateResult.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ContentEditingService.GetAsync(updateResult.Result.Content!.Key));

        void VerifyUpdate(IContent? updatedContent)
        {
            Assert.IsNotNull(updatedContent);
            Assert.AreEqual(firstUpdateDateDa, updatedContent.GetUpdateDate("da-DK"));

            var lastUpdateDateEn = updatedContent.GetUpdateDate("en-US")
                                   ?? throw new InvalidOperationException("Expected a publish date for EN");
            Assert.Greater(lastUpdateDateEn, firstUpdateDateEn);
        }
    }

    [Test]
    public async Task Updating_Single_Variant_Property_Does_Not_Change_Update_Dates_Of_Other_Variants()
    {
        var content = await CreateCultureVariantContent();
        var firstUpdateDateEn = content.GetUpdateDate("en-US")
                                ?? throw new InvalidOperationException("Expected an update date for EN");
        var firstUpdateDateDa = content.GetUpdateDate("da-DK")
                                ?? throw new InvalidOperationException("Expected an update date for DA");

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
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        VerifyUpdate(result.Result.Content);

        // re-get and re-test
        VerifyUpdate(await ContentEditingService.GetAsync(content.Key));

        void VerifyUpdate(IContent? updatedContent)
        {
            Assert.IsNotNull(updatedContent);
            Assert.AreEqual(firstUpdateDateEn, updatedContent.GetUpdateDate("en-US"));

            var lastUpdateDateDa = updatedContent.GetUpdateDate("da-DK")
                                   ?? throw new InvalidOperationException("Expected an update date for DA");
            Assert.Greater(lastUpdateDateDa, firstUpdateDateDa);
        }
    }
}
