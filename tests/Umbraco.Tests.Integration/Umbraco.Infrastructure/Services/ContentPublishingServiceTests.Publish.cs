﻿using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentPublishingServiceTests
{
    [Test]
    public async Task Can_Publish_Root()
    {
        VerifyIsNotPublished(Textpage.Key);

        var result = await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Status);
        VerifyIsPublished(Textpage.Key);
    }

    [Test]
    public async Task Publish_Single_Item_Does_Not_Publish_Children()
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);

        VerifyIsPublished(Textpage.Key);
        VerifyIsNotPublished(Subpage.Key);
    }

    [Test]
    public async Task Can_Publish_Child_Of_Root()
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);

        var result = await ContentPublishingService.PublishAsync(Subpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Status);
        VerifyIsPublished(Subpage.Key);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Publish_Branch_Does_Not_Publish_Unpublished_Children_Unless_Explicitly_Instructed_To(bool force)
    {
        var result = await ContentPublishingService.PublishBranchAsync(Textpage.Key, _allCultures, force, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);

        VerifyIsPublished(Textpage.Key);

        if (force)
        {
            AssertBranchResultSuccess(result.Result, Textpage.Key, Subpage.Key, Subpage2.Key, Subpage3.Key);
            VerifyIsPublished(Subpage.Key);
            VerifyIsPublished(Subpage2.Key);
            VerifyIsPublished(Subpage3.Key);
        }
        else
        {
            AssertBranchResultSuccess(result.Result, Textpage.Key);
            VerifyIsNotPublished(Subpage.Key);
            VerifyIsNotPublished(Subpage2.Key);
            VerifyIsNotPublished(Subpage3.Key);
        }
    }

    [Test]
    public async Task Can_Publish_Branch_Beneath_Root()
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);
        var subpage2Subpage = ContentBuilder.CreateSimpleContent(ContentType, "Text Page 2-2", Subpage2.Id);
        ContentService.Save(subpage2Subpage, -1);

        VerifyIsNotPublished(Subpage2.Key);
        var result = await ContentPublishingService.PublishBranchAsync(Subpage2.Key, _allCultures, true, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        AssertBranchResultSuccess(result.Result, Subpage2.Key, subpage2Subpage.Key);
        VerifyIsPublished(Subpage2.Key);
        VerifyIsPublished(subpage2Subpage.Key);
        VerifyIsNotPublished(Subpage.Key);
    }

    [Test]
    public async Task Can_Cancel_Publishing_With_Notification()
    {
        ContentNotificationHandler.PublishingContent = notification => notification.Cancel = true;

        var result = await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.CancelledByEvent, result.Status);
    }

    [Test]
    public async Task Can_Publish_Variant_Content()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        IContent content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        content.SetValue("title", "EN title", culture: langEn.IsoCode);
        content.SetValue("title", "DA title", culture: langDa.IsoCode);
        ContentService.Save(content);

        var result = await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { langEn.IsoCode, langDa.IsoCode }), Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Status);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(2, content.PublishedCultures.Count());
        Assert.IsTrue(content.PublishedCultures.InvariantContains(langEn.IsoCode));
        Assert.IsTrue(content.PublishedCultures.InvariantContains(langDa.IsoCode));
    }

    [Test]
    public async Task Can_Publish_All_Variants_And_Unpublish_All_Variants_And_Publish_A_Single_Variant()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        IContent content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        content.SetValue("title", "EN title", culture: langEn.IsoCode);
        content.SetValue("title", "DA title", culture: langDa.IsoCode);
        ContentService.Save(content);

        var publishResult = await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { langEn.IsoCode, langDa.IsoCode }), Constants.Security.SuperUserKey);

        Assert.IsTrue(publishResult.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, publishResult.Status);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(2, content.PublishedCultures.Count());
        Assert.IsTrue(content.PublishedCultures.InvariantContains(langEn.IsoCode));
        Assert.IsTrue(content.PublishedCultures.InvariantContains(langDa.IsoCode));

        var unpublishResult = await ContentPublishingService.UnpublishAsync(content.Key, new HashSet<string>() { "*" }, Constants.Security.SuperUserKey);

        Assert.IsTrue(unpublishResult.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, unpublishResult.Result);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(0, content.PublishedCultures.Count());

        publishResult = await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { langDa.IsoCode }), Constants.Security.SuperUserKey);

        Assert.IsTrue(publishResult.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, publishResult.Status);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(1, content.PublishedCultures.Count());
        Assert.IsTrue(content.PublishedCultures.InvariantContains(langDa.IsoCode));
    }

    [Test]
    public async Task Can_Publish_Branch_Of_Variant_Content()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        IContent root = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        root.SetValue("title", "EN root title", culture: langEn.IsoCode);
        root.SetValue("title", "DA root title", culture: langDa.IsoCode);
        ContentService.Save(root);

        IContent child = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN child")
            .WithCultureName(langDa.IsoCode, "DA child")
            .WithParent(root)
            .Build();
        child.SetValue("title", "EN child title", culture: langEn.IsoCode);
        child.SetValue("title", "DA child title", culture: langDa.IsoCode);
        ContentService.Save(child);

        var result = await ContentPublishingService.PublishBranchAsync(root.Key, new[] { langEn.IsoCode, langDa.IsoCode }, true, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        AssertBranchResultSuccess(result.Result, root.Key, child.Key);

        root = ContentService.GetById(root.Key)!;
        Assert.AreEqual(2, root.PublishedCultures.Count());
        Assert.IsTrue(root.PublishedCultures.InvariantContains(langEn.IsoCode));
        Assert.IsTrue(root.PublishedCultures.InvariantContains(langDa.IsoCode));

        child = ContentService.GetById(child.Key)!;
        Assert.AreEqual(2, child.PublishedCultures.Count());
        Assert.IsTrue(child.PublishedCultures.InvariantContains(langEn.IsoCode));
        Assert.IsTrue(child.PublishedCultures.InvariantContains(langDa.IsoCode));
    }

    [Test]
    public async Task Can_Publish_Culture_With_Other_Culture_Invalid()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        IContent content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        content.SetValue("title", "EN title", culture: langEn.IsoCode);
        content.SetValue("title", null, culture: langDa.IsoCode);
        ContentService.Save(content);

        var result = await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { langEn.IsoCode }), Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Status);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(1, content.PublishedCultures.Count());
        Assert.IsTrue(content.PublishedCultures.First().InvariantEquals(langEn.IsoCode));
    }

    [Test]
    public async Task Can_Publish_Culture_Branch_With_Other_Culture_Invalid()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        IContent root = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        root.SetValue("title", "EN title", culture: langEn.IsoCode);
        root.SetValue("title", null, culture: langDa.IsoCode);
        ContentService.Save(root);

        IContent child = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN child")
            .WithCultureName(langDa.IsoCode, "DA child")
            .WithParent(root)
            .Build();
        child.SetValue("title", "EN child title", culture: langEn.IsoCode);
        child.SetValue("title", "DA child title", culture: langDa.IsoCode);
        ContentService.Save(child);

        var result = await ContentPublishingService.PublishBranchAsync(root.Key, new[] { langEn.IsoCode }, true, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        AssertBranchResultSuccess(result.Result, root.Key, child.Key);

        root = ContentService.GetById(root.Key)!;
        Assert.AreEqual(1, root.PublishedCultures.Count());
        Assert.IsTrue(root.PublishedCultures.InvariantContains(langEn.IsoCode));

        child = ContentService.GetById(child.Key)!;
        Assert.AreEqual(1, child.PublishedCultures.Count());
        Assert.IsTrue(child.PublishedCultures.InvariantContains(langEn.IsoCode));
    }

    [Test]
    public async Task Can_Publish_Culture_Branch_Without_Other_Culture()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        IContent root = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        root.SetValue("title", "EN title", culture: langEn.IsoCode);
        root.SetValue("title", "DA title", culture: langDa.IsoCode);
        ContentService.Save(root);

        root = ContentService.GetById(root.Key)!;

        IContent child = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN child")
            .WithCultureName(langDa.IsoCode, "DA child")
            .WithParent(root)
            .Build();
        child.SetValue("title", "EN child title", culture: langEn.IsoCode);
        child.SetValue("title", "DA child title", culture: langDa.IsoCode);
        ContentService.Save(child);

        var result = await ContentPublishingService.PublishBranchAsync(root.Key, new[] { langEn.IsoCode }, true, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        AssertBranchResultSuccess(result.Result, root.Key, child.Key);

        root = ContentService.GetById(root.Key)!;
        Assert.AreEqual(1, root.PublishedCultures.Count());
        Assert.IsTrue(root.PublishedCultures.InvariantContains(langEn.IsoCode));

        child = ContentService.GetById(child.Key)!;
        Assert.AreEqual(1, child.PublishedCultures.Count());
        Assert.IsTrue(child.PublishedCultures.InvariantContains(langEn.IsoCode));
    }

    [Test]
    public async Task Can_Publish_Variant_Content_With_Mandatory_Culture()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest(true);

        IContent content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        content.SetValue("title", "EN title", culture: langEn.IsoCode);
        content.SetValue("title", "DA title", culture: langDa.IsoCode);
        ContentService.Save(content);

        var result = await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { langEn.IsoCode, langDa.IsoCode }), Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Status);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(2, content.PublishedCultures.Count());
    }

    [Test]
    public async Task Cannot_Publish_Non_Existing_Content()
    {
        var result = await ContentPublishingService.PublishAsync(Guid.NewGuid(), MakeModel(_allCultures), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.ContentNotFound, result.Status);
    }

    [Test]
    public async Task Cannot_Publish_Branch_Of_Non_Existing_Content()
    {
        var key = Guid.NewGuid();
        var result = await ContentPublishingService.PublishBranchAsync(key, _allCultures, true, Constants.Security.SuperUserKey);
        Assert.IsFalse(result);
        AssertBranchResultFailed(result.Result, (key, ContentPublishingOperationStatus.ContentNotFound));
    }

    [Test]
    public async Task Cannot_Publish_Invalid_Content()
    {
        var content = await CreateInvalidContent();

        var result = await ContentPublishingService.PublishAsync(content.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);

        Assert.IsFalse(result);
        Assert.AreEqual(ContentPublishingOperationStatus.ContentInvalid, result.Status);

        var invalidPropertyAliases = result.Result.InvalidPropertyAliases.ToArray();
        Assert.AreEqual(3, invalidPropertyAliases.Length);
        Assert.Contains("title", invalidPropertyAliases);
        Assert.Contains("bodyText", invalidPropertyAliases);
        Assert.Contains("author", invalidPropertyAliases);

        VerifyIsNotPublished(content.Key);
    }

    [Test]
    public async Task Cannot_Publish_Branch_With_Invalid_Parent()
    {
        var content = await CreateInvalidContent(Textpage);
        var child = ContentBuilder.CreateSimpleContent(ContentType, "Child page", content.Id);
        ContentService.Save(child, -1);
        Assert.AreEqual(content.Id, ContentService.GetById(child.Key)!.ParentId);

        var result = await ContentPublishingService.PublishBranchAsync(Textpage.Key, _allCultures, true, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        AssertBranchResultSuccess(result.Result, Textpage.Key, Subpage.Key, Subpage2.Key, Subpage3.Key);
        AssertBranchResultFailed(result.Result, (content.Key, ContentPublishingOperationStatus.ContentInvalid));
        VerifyIsPublished(Textpage.Key);
        VerifyIsPublished(Subpage.Key);
        VerifyIsPublished(Subpage2.Key);
        VerifyIsPublished(Subpage3.Key);
        VerifyIsNotPublished(content.Key);
        VerifyIsNotPublished(child.Key);
    }

    [Test]
    public async Task Cannot_Publish_Invalid_Variant_Content()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        IContent content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        content.SetValue("title", "EN title", culture: langEn.IsoCode);
        content.SetValue("title", null, culture: langDa.IsoCode);
        ContentService.Save(content);

        var result = await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { langEn.IsoCode, langDa.IsoCode }), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.ContentInvalid, result.Status);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(0, content.PublishedCultures.Count());
    }

    [Test]
    public async Task Cannot_Publish_Variant_Content_Without_Mandatory_Culture()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest(true);

        IContent content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        content.SetValue("title", "EN title", culture: langEn.IsoCode);
        content.SetValue("title", "DA title", culture: langDa.IsoCode);
        ContentService.Save(content);

        var result = await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { langDa.IsoCode }), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.MandatoryCultureMissing, result.Status);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(0, content.PublishedCultures.Count());
    }

    [Test]
    public async Task Cannot_Publish_Culture_Branch_With_Invalid_Culture()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        IContent root = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        root.SetValue("title", "EN title", culture: langEn.IsoCode);
        root.SetValue("title", null, culture: langDa.IsoCode);
        ContentService.Save(root);

        IContent child = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN child")
            .WithCultureName(langDa.IsoCode, "DA child")
            .WithParent(root)
            .Build();
        child.SetValue("title", "EN child title", culture: langEn.IsoCode);
        child.SetValue("title", "DA child title", culture: langDa.IsoCode);
        ContentService.Save(child);

        var result = await ContentPublishingService.PublishBranchAsync(root.Key, new[] { langEn.IsoCode, langDa.IsoCode }, true, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        AssertBranchResultFailed(result.Result, (root.Key, ContentPublishingOperationStatus.ContentInvalid));

        root = ContentService.GetById(root.Key)!;
        Assert.AreEqual(0, root.PublishedCultures.Count());

        child = ContentService.GetById(child.Key)!;
        Assert.AreEqual(0, child.PublishedCultures.Count());
    }

    [Test]
    public async Task Cannot_Publish_Child_Of_Unpublished_Parent()
    {
        VerifyIsNotPublished(Textpage.Key);

        var result = await ContentPublishingService.PublishAsync(Subpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.PathNotPublished, result.Status);
        VerifyIsNotPublished(Subpage.Key);
    }

    [Test]
    public async Task Cannot_Publish_From_Trash()
    {
        ContentService.MoveToRecycleBin(Subpage);
        Assert.IsTrue(ContentService.GetById(Subpage.Key)!.Trashed);

        var result = await ContentPublishingService.PublishAsync(Subpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.InTrash, result.Status);
        VerifyIsNotPublished(Subpage.Key);
    }

    [Test]
    public async Task Cannot_Republish_Content_After_Adding_Validation_To_Existing_Property()
    {
        Textpage.SetValue("title", string.Empty);
        Textpage.SetValue("author", "This is not a number");
        ContentService.Save(Textpage);

        var result = await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        VerifyIsPublished(Textpage.Key);

        ContentType.PropertyTypes.First(pt => pt.Alias == "title").Mandatory = true;
        ContentType.PropertyTypes.First(pt => pt.Alias == "author").ValidationRegExp = "^\\d*$";
        await ContentTypeService.SaveAsync(ContentType, Constants.Security.SuperUserKey);

        result = await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.ContentInvalid, result.Status);

        var invalidPropertyAliases = result.Result.InvalidPropertyAliases.ToArray();
        Assert.AreEqual(2, invalidPropertyAliases.Length);
        Assert.Contains("title", invalidPropertyAliases);
        Assert.Contains("author", invalidPropertyAliases);

        // despite the failure to publish, the page should remain published
        VerifyIsPublished(Textpage.Key);
    }

    [Test]
    public async Task Cannot_Republish_Content_After_Adding_Mandatory_Property()
    {
        var result = await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        VerifyIsPublished(Textpage.Key);

        ContentType.AddPropertyType(
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Nvarchar)
            {
                Alias = "mandatoryProperty", Name = "Mandatory Property", Mandatory = true
            });
        await ContentTypeService.SaveAsync(ContentType, Constants.Security.SuperUserKey);

        result = await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.ContentInvalid, result.Status);

        var invalidPropertyAliases = result.Result.InvalidPropertyAliases.ToArray();
        Assert.AreEqual(1, invalidPropertyAliases.Length);
        Assert.Contains("mandatoryProperty", invalidPropertyAliases);

        // despite the failure to publish, the page should remain published
        VerifyIsPublished(Textpage.Key);
    }

    [Test]
    public async Task Cannot_Republish_Branch_After_Adding_Mandatory_Property()
    {
        var result = await ContentPublishingService.PublishBranchAsync(Textpage.Key, _allCultures, true, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        VerifyIsPublished(Textpage.Key);
        VerifyIsPublished(Subpage.Key);
        VerifyIsPublished(Subpage2.Key);
        VerifyIsPublished(Subpage3.Key);

        // force an update on the child pages so they will be subject to branch republishing
        foreach (var key in new [] { Subpage.Key, Subpage2.Key, Subpage3.Key })
        {
            var content = ContentService.GetById(key)!;
            content.SetValue("title", "Updated");
            ContentService.Save(content);
        }

        ContentType.AddPropertyType(
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Nvarchar)
            {
                Alias = "mandatoryProperty", Name = "Mandatory Property", Mandatory = true
            });
        await ContentTypeService.SaveAsync(ContentType, Constants.Security.SuperUserKey);

        // force an update on the root page so it is valid (and also subject to branch republishing).
        // if we didn't do this, the children would never be considered for branch publishing, as the publish logic
        // stops at the first invalid parent.
        // as an added bonus, this lets us test a partially successful branch publish :)
        var textPage = ContentService.GetById(Textpage.Key)!;
        textPage.SetValue("mandatoryProperty", "This is a valid value");
        ContentService.Save(textPage);

        result = await ContentPublishingService.PublishBranchAsync(Textpage.Key, _allCultures, true, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.FailedBranch, result.Status);
        AssertBranchResultSuccess(result.Result, Textpage.Key);
        AssertBranchResultFailed(
            result.Result,
            (Subpage.Key, ContentPublishingOperationStatus.ContentInvalid),
            (Subpage2.Key, ContentPublishingOperationStatus.ContentInvalid),
            (Subpage3.Key, ContentPublishingOperationStatus.ContentInvalid));

        // despite the failure to publish, the entier branch should remain published
        VerifyIsPublished(Textpage.Key);
        VerifyIsPublished(Subpage.Key);
        VerifyIsPublished(Subpage2.Key);
        VerifyIsPublished(Subpage3.Key);
    }

    [TestCase("en-us")]
    [TestCase("da-dk")]
    public async Task Cannot_Publish_Incorrect_Culture_Code(string cultureCode)
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        IContent content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        content.SetValue("title", "EN title", culture: langEn.IsoCode);
        content.SetValue("title", "DA title", culture: langDa.IsoCode);
        ContentService.Save(content);

        var result = await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { cultureCode }), Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.InvalidCulture, result.Status);
    }

    [TestCase("de-DE")]
    [TestCase("es-ES")]
    public async Task Cannot_Publish_Non_Existing_Culture(string cultureCode)
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        IContent content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        content.SetValue("title", "EN title", culture: langEn.IsoCode);
        content.SetValue("title", "DA title", culture: langDa.IsoCode);
        ContentService.Save(content);

        var result = await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { cultureCode }), Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.InvalidCulture, result.Status);
    }

    private void AssertBranchResultSuccess(ContentPublishingBranchResult result, params Guid[] expectedKeys)
    {
        var items = result.SucceededItems.ToArray();
        Assert.AreEqual(expectedKeys.Length, items.Length);
        foreach (var key in expectedKeys)
        {
            var item = items.FirstOrDefault(i => i.Key == key);
            Assert.IsNotNull(item);
            Assert.AreEqual(ContentPublishingOperationStatus.Success, item.OperationStatus);
        }
    }

    private void AssertBranchResultFailed(ContentPublishingBranchResult result, params (Guid, ContentPublishingOperationStatus)[] expectedFailures)
    {
        var items = result.FailedItems.ToArray();
        Assert.AreEqual(expectedFailures.Length, items.Length);
        foreach (var (key, status) in expectedFailures)
        {
            var item = items.FirstOrDefault(i => i.Key == key);
            Assert.IsNotNull(item);
            Assert.AreEqual(status, item.OperationStatus);
        }
    }
}
