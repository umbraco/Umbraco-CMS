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
            InvariantName = "Updated Name",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The updated title" },
                new PropertyValueModel { Alias = "text", Value = "The updated text" }
            }
        };

        var result = await ContentEditingService.UpdateAsync(content, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        VerifyUpdate(result.Result);

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
    public async Task Can_Update_Variant()
    {
        var content = await CreateVariantContent();

        var updateModel = new ContentUpdateModel
        {
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "invariantTitle", Value = "The updated invariant title" }
            },
            Variants = new []
            {
                new VariantModel
                {
                    Culture = "en-US",
                    Name = "Updated English Name",
                    Properties = new []
                    {
                        new PropertyValueModel { Alias = "variantTitle", Value = "The updated English title" }
                    }
                },
                new VariantModel
                {
                    Culture = "da-DK",
                    Name = "Updated Danish Name",
                    Properties = new []
                    {
                        new PropertyValueModel { Alias = "variantTitle", Value = "The updated Danish title" }
                    }
                }
            }
        };

        var result = await ContentEditingService.UpdateAsync(content, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        VerifyUpdate(result.Result);

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
            InvariantName = "Updated Name",
            TemplateKey = templateTwo.Key
        };

        var result = await ContentEditingService.UpdateAsync(content, updateModel, Constants.Security.SuperUserKey);
        VerifyUpdate(result.Result);

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
            InvariantName = "Updated Name",
            TemplateKey = null
        };

        var result = await ContentEditingService.UpdateAsync(content, updateModel, Constants.Security.SuperUserKey);
        VerifyUpdate(result.Result);

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
            InvariantName = "Updated Name",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The updated title" }
            }
        };

        var result = await ContentEditingService.UpdateAsync(content, updateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.Success, result.Status);
        VerifyUpdate(result.Result);

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

    [Test]
    public async Task Cannot_Update_With_Variant_Property_Value_For_Invariant_Content()
    {
        var content = await CreateInvariantContent();

        var updateModel = new ContentUpdateModel
        {
            InvariantName = "Updated Name",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "title", Value = "The updated title" },
            },
            Variants = new[]
            {
                new VariantModel
                {
                    Culture = "en-US",
                    Name = "Updated English Name",
                    Properties = new[]
                    {
                        new PropertyValueModel { Alias = "text", Value = "The updated text" }
                    }
                }
            }
        };

        var result = await ContentEditingService.UpdateAsync(content, updateModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.ContentTypeCultureVarianceMismatch, result.Status);

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
        var content = await CreateVariantContent();

        var updateModel = new ContentUpdateModel
        {
            InvariantName = "Updated Name",
            InvariantProperties = new[]
            {
                new PropertyValueModel { Alias = "invariantTitle", Value = "The updated invariant title" },
                new PropertyValueModel { Alias = "variantTitle", Value = "The updated variant title" }
            }
        };

        var result = await ContentEditingService.UpdateAsync(content, updateModel, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentEditingOperationStatus.PropertyTypeNotFound, result.Status);
        Assert.IsNotNull(result.Result);

        // re-get and validate
        content = await ContentEditingService.GetAsync(content.Key);
        Assert.IsNotNull(content);
        Assert.AreEqual("Initial English Name", content.GetCultureName("en-US"));
        Assert.AreEqual("The initial invariant title", content.GetValue<string>("invariantTitle"));
        Assert.AreEqual("The initial English title", content.GetValue<string>("variantTitle", "en-US"));
        Assert.AreEqual("The initial Danish title", content.GetValue<string>("variantTitle", "da-DK"));
    }
}
