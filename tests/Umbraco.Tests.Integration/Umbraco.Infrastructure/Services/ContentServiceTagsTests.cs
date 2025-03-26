// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true,
    Logger = UmbracoTestOptions.Logger.Console)]
public class ContentServiceTagsTests : UmbracoIntegrationTest
{
    [SetUp]
    public void Setup() => ContentRepositoryBase.ThrowOnWarning = true;

    [TearDown]
    public void Teardown() => ContentRepositoryBase.ThrowOnWarning = false;

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private ITagService TagService => GetRequiredService<ITagService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private IFileService FileService => GetRequiredService<IFileService>();

    private IJsonSerializer Serializer => GetRequiredService<IJsonSerializer>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer =>
        GetRequiredService<IConfigurationEditorJsonSerializer>();

    public PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    [Test]
    public void TagsCanBeInvariant()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type",
            mandatoryProperties: true, defaultTemplateId: template.Id);
        CreateAndAddTagsPropertyType(contentType);
        ContentTypeService.Save(contentType);

        IContent content1 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1");
        content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "another", "one" });
        ContentService.Save(content1);
        ContentService.Publish(content1, content1.AvailableCultures.ToArray());

        content1 = ContentService.GetById(content1.Id);

        var enTags = content1.Properties["tags"].GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer)
            .ToArray();
        Assert.AreEqual(4, enTags.Length);
        Assert.Contains("one", enTags);
        Assert.AreEqual(-1, enTags.IndexOf("plus"));

        var tagGroups = TagService.GetAllTags().GroupBy(x => x.LanguageId);
        foreach (var tag in TagService.GetAllTags())
        {
            Console.WriteLine($"{tag.Group}:{tag.Text} {tag.LanguageId}");
        }

        Assert.AreEqual(1, tagGroups.Count());
        var enTagGroup = tagGroups.FirstOrDefault(x => x.Key == null);
        Assert.IsNotNull(enTagGroup);
        Assert.AreEqual(4, enTagGroup.Count());
        Assert.IsTrue(enTagGroup.Any(x => x.Text == "one"));
        Assert.IsFalse(enTagGroup.Any(x => x.Text == "plus"));
    }

    [Test]
    public async Task TagsCanBeVariant()
    {
       var language = new LanguageBuilder()
            .WithCultureInfo("fr-FR")
            .Build();
        await LanguageService.CreateAsync(language, Constants.Security.SuperUserKey); // en-US is already there

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type",
            mandatoryProperties: true, defaultTemplateId: template.Id);
        CreateAndAddTagsPropertyType(contentType, ContentVariation.Culture);
        ContentTypeService.Save(contentType);

        IContent content1 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1");
        content1.SetCultureName("name-fr", "fr-FR");
        content1.SetCultureName("name-en", "en-US");
        content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "some", "tags", "plus" }, culture: "fr-FR");
        content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "another", "one" }, culture: "en-US");
        ContentService.Save(content1);
        ContentService.Publish(content1, content1.AvailableCultures.ToArray());

        content1 = ContentService.GetById(content1.Id);

        var frTags = content1.Properties["tags"]
            .GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer, "fr-FR").ToArray();
        Assert.AreEqual(5, frTags.Length);
        Assert.Contains("plus", frTags);
        Assert.AreEqual(-1, frTags.IndexOf("one"));

        var enTags = content1.Properties["tags"]
            .GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer, "en-US").ToArray();
        Assert.AreEqual(4, enTags.Length);
        Assert.Contains("one", enTags);
        Assert.AreEqual(-1, enTags.IndexOf("plus"));

        var tagGroups = TagService.GetAllTags(culture: "*").GroupBy(x => x.LanguageId);
        foreach (var tag in TagService.GetAllTags())
        {
            Console.WriteLine($"{tag.Group}:{tag.Text} {tag.LanguageId}");
        }

        Assert.AreEqual(2, tagGroups.Count());
        var frTagGroup = tagGroups.FirstOrDefault(x => x.Key == 2);
        Assert.IsNotNull(frTagGroup);
        Assert.AreEqual(5, frTagGroup.Count());
        Assert.IsTrue(frTagGroup.Any(x => x.Text == "plus"));
        Assert.IsFalse(frTagGroup.Any(x => x.Text == "one"));
        var enTagGroup = tagGroups.FirstOrDefault(x => x.Key == 1);
        Assert.IsNotNull(enTagGroup);
        Assert.AreEqual(4, enTagGroup.Count());
        Assert.IsTrue(enTagGroup.Any(x => x.Text == "one"));
        Assert.IsFalse(enTagGroup.Any(x => x.Text == "plus"));
    }

    [Test]
    public async Task TagsCanBecomeVariant()
    {
        var enId = (await LanguageService.GetAsync("en-US"))!.Id;

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type",
            mandatoryProperties: true, defaultTemplateId: template.Id);
        var propertyType = CreateAndAddTagsPropertyType(contentType);
        ContentTypeService.Save(contentType);

        IContent content1 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1");
        content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "another", "one" });
        ContentService.Save(content1);
        ContentService.Publish(content1, content1.AvailableCultures.ToArray());

        contentType.Variations = ContentVariation.Culture;
        ContentTypeService.Save(contentType);

        // no changes
        content1 = ContentService.GetById(content1.Id);

        var tags = content1.Properties["tags"].GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer)
            .ToArray();
        Assert.AreEqual(4, tags.Length);
        Assert.Contains("one", tags);
        Assert.AreEqual(-1, tags.IndexOf("plus"));

        var tagGroups = TagService.GetAllTags().GroupBy(x => x.LanguageId);
        foreach (var tag in TagService.GetAllTags())
        {
            Console.WriteLine($"{tag.Group}:{tag.Text} {tag.LanguageId}");
        }

        Assert.AreEqual(1, tagGroups.Count());
        var enTagGroup = tagGroups.FirstOrDefault(x => x.Key == null);
        Assert.IsNotNull(enTagGroup);
        Assert.AreEqual(4, enTagGroup.Count());
        Assert.IsTrue(enTagGroup.Any(x => x.Text == "one"));
        Assert.IsFalse(enTagGroup.Any(x => x.Text == "plus"));

        propertyType.Variations = ContentVariation.Culture;
        ContentTypeService.Save(contentType);

        // changes
        content1 = ContentService.GetById(content1.Id);

        // property value has been moved from invariant to en-US
        tags = content1.Properties["tags"].GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer)
            .ToArray();
        Assert.IsEmpty(tags);

        tags = content1.Properties["tags"].GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer, "en-US")
            .ToArray();
        Assert.AreEqual(4, tags.Length);
        Assert.Contains("one", tags);
        Assert.AreEqual(-1, tags.IndexOf("plus"));

        // tags have been copied from invariant to en-US
        tagGroups = TagService.GetAllTags(culture: "*").GroupBy(x => x.LanguageId);
        foreach (var tag in TagService.GetAllTags("*"))
        {
            Console.WriteLine($"{tag.Group}:{tag.Text} {tag.LanguageId}");
        }

        Assert.AreEqual(1, tagGroups.Count());

        enTagGroup = tagGroups.FirstOrDefault(x => x.Key == enId);
        Assert.IsNotNull(enTagGroup);
        Assert.AreEqual(4, enTagGroup.Count());
        Assert.IsTrue(enTagGroup.Any(x => x.Text == "one"));
        Assert.IsFalse(enTagGroup.Any(x => x.Text == "plus"));
    }

    [Test]
    public async Task TagsCanBecomeInvariant()
    {
        var language = new LanguageBuilder()
            .WithCultureInfo("fr-FR")
            .Build();
        await LanguageService.CreateAsync(language, Constants.Security.SuperUserKey); // en-US is already there

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type",
            mandatoryProperties: true, defaultTemplateId: template.Id);
        CreateAndAddTagsPropertyType(contentType, ContentVariation.Culture);
        ContentTypeService.Save(contentType);

        IContent content1 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1");
        content1.SetCultureName("name-fr", "fr-FR");
        content1.SetCultureName("name-en", "en-US");
        content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "some", "tags", "plus" }, culture: "fr-FR");
        content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "another", "one" }, culture: "en-US");
        ContentService.Save(content1);
        ContentService.Publish(content1, content1.AvailableCultures.ToArray());

        contentType.Variations = ContentVariation.Nothing;
        ContentTypeService.Save(contentType);

        // changes
        content1 = ContentService.GetById(content1.Id);

        // property value has been moved from en-US to invariant, fr-FR tags are gone
        Assert.IsEmpty(content1.Properties["tags"]
            .GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer, "fr-FR"));
        Assert.IsEmpty(content1.Properties["tags"]
            .GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer, "en-US"));

        var tags = content1.Properties["tags"].GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer)
            .ToArray();
        Assert.AreEqual(4, tags.Length);
        Assert.Contains("one", tags);
        Assert.AreEqual(-1, tags.IndexOf("plus"));

        // tags have been copied from en-US to invariant, fr-FR tags are gone
        var tagGroups = TagService.GetAllTags(culture: "*").GroupBy(x => x.LanguageId);
        foreach (var tag in TagService.GetAllTags("*"))
        {
            Console.WriteLine($"{tag.Group}:{tag.Text} {tag.LanguageId}");
        }

        Assert.AreEqual(1, tagGroups.Count());

        var enTagGroup = tagGroups.FirstOrDefault(x => x.Key == null);
        Assert.IsNotNull(enTagGroup);
        Assert.AreEqual(4, enTagGroup.Count());
        Assert.IsTrue(enTagGroup.Any(x => x.Text == "one"));
        Assert.IsFalse(enTagGroup.Any(x => x.Text == "plus"));
    }

    [Test]
    public async Task TagsCanBecomeInvariant2()
    {
        var language = new LanguageBuilder()
            .WithCultureInfo("fr-FR")
            .Build();
        await LanguageService.CreateAsync(language, Constants.Security.SuperUserKey); // en-US is already there

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type",
            mandatoryProperties: true, defaultTemplateId: template.Id);
        var propertyType = CreateAndAddTagsPropertyType(contentType, ContentVariation.Culture);
        ContentTypeService.Save(contentType);

        IContent content1 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1");
        content1.SetCultureName("name-fr", "fr-FR");
        content1.SetCultureName("name-en", "en-US");
        content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "some", "tags", "plus" }, culture: "fr-FR");
        content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "another", "one" }, culture: "en-US");
        ContentService.Save(content1);
        ContentService.Publish(content1, content1.AvailableCultures.ToArray());

        IContent content2 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 2");
        content2.SetCultureName("name-fr", "fr-FR");
        content2.SetCultureName("name-en", "en-US");
        content2.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "some", "tags", "plus" }, culture: "fr-FR");
        content2.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "another", "one" }, culture: "en-US");
        ContentService.Save(content2);
        ContentService.Publish(content2, content2.AvailableCultures.ToArray());

        //// pretend we already have invariant values
        // using (var scope = ScopeProvider.CreateScope())
        // {
        //    scope.Database.Execute("INSERT INTO [cmsTags] ([tag], [group], [languageId]) SELECT DISTINCT [tag], [group], NULL FROM [cmsTags] WHERE [languageId] IS NOT NULL");
        // }

        // this should work
        propertyType.Variations = ContentVariation.Nothing;
        Assert.DoesNotThrow(() => ContentTypeService.Save(contentType));
    }

    [Test]
    public async Task TagsCanBecomeInvariantByPropertyType()
    {
        var language = new LanguageBuilder()
            .WithCultureInfo("fr-FR")
            .Build();
        await LanguageService.CreateAsync(language, Constants.Security.SuperUserKey); // en-US is already there

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type",
            mandatoryProperties: true, defaultTemplateId: template.Id);
        var propertyType = CreateAndAddTagsPropertyType(contentType, ContentVariation.Culture);
        ContentTypeService.Save(contentType);

        IContent content1 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1");
        content1.SetCultureName("name-fr", "fr-FR");
        content1.SetCultureName("name-en", "en-US");
        content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "some", "tags", "plus" }, culture: "fr-FR");
        content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "another", "one" }, culture: "en-US");
        ContentService.Save(content1);
        ContentService.Publish(content1, content1.AvailableCultures.ToArray());

        propertyType.Variations = ContentVariation.Nothing;
        ContentTypeService.Save(contentType);

        // changes
        content1 = ContentService.GetById(content1.Id);

        // property value has been moved from en-US to invariant, fr-FR tags are gone
        Assert.IsEmpty(content1.Properties["tags"]
            .GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer, "fr-FR"));
        Assert.IsEmpty(content1.Properties["tags"]
            .GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer, "en-US"));

        var tags = content1.Properties["tags"].GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer)
            .ToArray();
        Assert.AreEqual(4, tags.Length);
        Assert.Contains("one", tags);
        Assert.AreEqual(-1, tags.IndexOf("plus"));

        // tags have been copied from en-US to invariant, fr-FR tags are gone
        var tagGroups = TagService.GetAllTags(culture: "*").GroupBy(x => x.LanguageId);
        foreach (var tag in TagService.GetAllTags("*"))
        {
            Console.WriteLine($"{tag.Group}:{tag.Text} {tag.LanguageId}");
        }

        Assert.AreEqual(1, tagGroups.Count());

        var enTagGroup = tagGroups.FirstOrDefault(x => x.Key == null);
        Assert.IsNotNull(enTagGroup);
        Assert.AreEqual(4, enTagGroup.Count());
        Assert.IsTrue(enTagGroup.Any(x => x.Text == "one"));
        Assert.IsFalse(enTagGroup.Any(x => x.Text == "plus"));
    }

    [Test]
    public async Task TagsCanBecomeInvariantByPropertyTypeAndBackToVariant()
    {
        var frValue = new string[] { "hello", "world", "some", "tags", "plus" };
        var enValue = new string[] { "hello", "world", "another", "one" };

        var language = new LanguageBuilder()
            .WithCultureInfo("fr-FR")
            .Build();
        await LanguageService.CreateAsync(language, Constants.Security.SuperUserKey); // en-US is already there

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type",
            mandatoryProperties: true, defaultTemplateId: template.Id);
        var propertyType = CreateAndAddTagsPropertyType(contentType, ContentVariation.Culture);
        ContentTypeService.Save(contentType);

        IContent content1 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1");
        content1.SetCultureName("name-fr", "fr-FR");
        content1.SetCultureName("name-en", "en-US");
        content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            frValue, culture: "fr-FR");
        content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            enValue, culture: "en-US");
        ContentService.Save(content1);
        ContentService.Publish(content1, content1.AvailableCultures.ToArray());

        propertyType.Variations = ContentVariation.Nothing;
        ContentTypeService.Save(contentType);

        // TODO: This throws due to index violations
        propertyType.Variations = ContentVariation.Culture;
        ContentTypeService.Save(contentType);

        Assert.AreEqual(frValue, Serializer.Deserialize<string[]>(content1.GetValue<string>("tags", "fr-FR")));
        Assert.AreEqual(enValue, Serializer.Deserialize<string[]>(content1.GetValue<string>("tags", "en-US")));
    }

    [Test]
    public void TagsAreUpdatedWhenContentIsTrashedAndUnTrashed_One()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type",
            mandatoryProperties: true, defaultTemplateId: template.Id);
        CreateAndAddTagsPropertyType(contentType);
        ContentTypeService.Save(contentType);

        var content1 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1");
        content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "some", "tags", "plus" });
        ContentService.Save(content1);
        ContentService.Publish(content1, content1.AvailableCultures.ToArray());

        var content2 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 2");
        content2.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "some", "tags" });
        ContentService.Save(content2);
        ContentService.Publish(content2, content2.AvailableCultures.ToArray());

        // verify
        var tags = TagService.GetTagsForEntity(content1.Id);
        Assert.AreEqual(5, tags.Count());
        var allTags = TagService.GetAllContentTags();
        Assert.AreEqual(5, allTags.Count());

        ContentService.MoveToRecycleBin(content1);
    }

    [Test]
    public void TagsAreUpdatedWhenContentIsTrashedAndUnTrashed_All()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type",
            mandatoryProperties: true, defaultTemplateId: template.Id);
        CreateAndAddTagsPropertyType(contentType);
        ContentTypeService.Save(contentType);

        var content1 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1");
        content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "some", "tags", "bam" });
        ContentService.Save(content1);
        ContentService.Publish(content1, content1.AvailableCultures.ToArray());

        var content2 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 2");
        content2.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "some", "tags" });
        ContentService.Save(content2);
        ContentService.Publish(content2, content2.AvailableCultures.ToArray());

        // verify
        var tags = TagService.GetTagsForEntity(content1.Id);
        Assert.AreEqual(5, tags.Count());
        var allTags = TagService.GetAllContentTags();
        Assert.AreEqual(5, allTags.Count());

        ContentService.Unpublish(content1);
        ContentService.Unpublish(content2);
    }

    [Test]
    [Ignore("https://github.com/umbraco/Umbraco-CMS/issues/3821 (U4-8442), will need to be fixed.")]
    public void TagsAreUpdatedWhenContentIsTrashedAndUnTrashed_Tree()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type",
            mandatoryProperties: true, defaultTemplateId: template.Id);
        CreateAndAddTagsPropertyType(contentType);
        ContentTypeService.Save(contentType);

        var content1 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1");
        content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "some", "tags", "plus" });
        ContentService.Save(content1);
        ContentService.Publish(content1, Array.Empty<string>());

        var content2 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 2", content1.Id);
        content2.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "some", "tags" });
        ContentService.Save(content2);
        ContentService.Publish(content2, Array.Empty<string>());

        // verify
        var tags = TagService.GetTagsForEntity(content1.Id);
        Assert.AreEqual(5, tags.Count());
        var allTags = TagService.GetAllContentTags();
        Assert.AreEqual(5, allTags.Count());

        ContentService.MoveToRecycleBin(content1);

        // no more tags
        tags = TagService.GetTagsForEntity(content1.Id);
        Assert.AreEqual(0, tags.Count());
        tags = TagService.GetTagsForEntity(content2.Id);
        Assert.AreEqual(0, tags.Count());

        // no more tags
        allTags = TagService.GetAllContentTags();
        Assert.AreEqual(0, allTags.Count());

        ContentService.Move(content1, -1);

        Assert.IsFalse(content1.Published);

        // no more tags
        tags = TagService.GetTagsForEntity(content1.Id);
        Assert.AreEqual(0, tags.Count());
        tags = TagService.GetTagsForEntity(content2.Id);
        Assert.AreEqual(0, tags.Count());

        // no more tags
        allTags = TagService.GetAllContentTags();
        Assert.AreEqual(0, allTags.Count());

        ContentService.Save(content1);
        ContentService.Publish(content1, Array.Empty<string>());

        Assert.IsTrue(content1.Published);

        // tags are back
        tags = TagService.GetTagsForEntity(content1.Id);
        Assert.AreEqual(5, tags.Count());

        // TODO: tag & tree issue
        // when we publish, we 'just' publish the top one and not the ones below = fails
        // what we should do is... NOT clear tags when unpublishing or trashing or...
        // and just update the tag service to NOT return anything related to trashed or
        // unpublished entities (since trashed is set on ALL entities in the trashed branch)
        tags = TagService.GetTagsForEntity(content2.Id); // including that one!
        Assert.AreEqual(4, tags.Count());

        // tags are back
        allTags = TagService.GetAllContentTags();
        Assert.AreEqual(5, allTags.Count());
    }

    [Test]
    public void TagsAreUpdatedWhenContentIsUnpublishedAndRePublished()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type",
            mandatoryProperties: true, defaultTemplateId: template.Id);
        CreateAndAddTagsPropertyType(contentType);
        ContentTypeService.Save(contentType);

        var content1 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1");
        content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "some", "tags", "bam" });
        ContentService.Save(content1);
        ContentService.Publish(content1, content1.AvailableCultures.ToArray());

        var content2 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 2");
        content2.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "some", "tags" });
        ContentService.Save(content2);
        ContentService.Publish(content2, content2.AvailableCultures.ToArray());

        ContentService.Unpublish(content1);
        ContentService.Unpublish(content2);
    }

    [Test]
    [Ignore("https://github.com/umbraco/Umbraco-CMS/issues/3821 (U4-8442), will need to be fixed.")]
    public void TagsAreUpdatedWhenContentIsUnpublishedAndRePublished_Tree()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type",
            mandatoryProperties: true, defaultTemplateId: template.Id);
        CreateAndAddTagsPropertyType(contentType);
        ContentTypeService.Save(contentType);

        var content1 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 1");
        content1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "some", "tags", "bam" });
        ContentService.Save(content1);
        ContentService.Publish(content1, Array.Empty<string>());

        var content2 = ContentBuilder.CreateSimpleContent(contentType, "Tagged content 2", content1);
        content2.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "some", "tags" });
        ContentService.Save(content2);
        ContentService.Publish(content2, Array.Empty<string>());

        ContentService.Unpublish(content1);

        var tags = TagService.GetTagsForEntity(content1.Id);
        Assert.AreEqual(0, tags.Count());

        // TODO: tag & tree issue
        // when we (un)publish, we 'just' publish the top one and not the ones below = fails
        // see similar note above
        tags = TagService.GetTagsForEntity(content2.Id);
        Assert.AreEqual(0, tags.Count());
        var allTags = TagService.GetAllContentTags();
        Assert.AreEqual(0, allTags.Count());

        ContentService.Publish(content1, Array.Empty<string>());

        tags = TagService.GetTagsForEntity(content2.Id);
        Assert.AreEqual(4, tags.Count());
        allTags = TagService.GetAllContentTags();
        Assert.AreEqual(5, allTags.Count());
    }

    [Test]
    public void Create_Tag_Data_Bulk_Publish_Operation()
    {
        // Arrange
        // set configuration
        var dataType = DataTypeService.GetDataType(1041);
        dataType.ConfigurationData = dataType.Editor!.GetConfigurationEditor()
            .FromConfigurationObject(
                new TagConfiguration { Group = "test", StorageType = TagsStorageType.Csv },
                ConfigurationEditorJsonSerializer);

        // updating the data type with the new configuration
        DataTypeService.Save(dataType);

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type",
            mandatoryProperties: true, defaultTemplateId: template.Id);
        CreateAndAddTagsPropertyType(contentType);
        ContentTypeService.Save(contentType);
        contentType.AllowedContentTypes =
            new[] { new ContentTypeSort(contentType.Key, 0, contentType.Alias) };

        var content = ContentBuilder.CreateSimpleContent(contentType, "Tagged content");
        content.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "some", "tags" });
        ContentService.Save(content);

        var child1 = ContentBuilder.CreateSimpleContent(contentType, "child 1 content", content.Id);
        child1.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello1", "world1", "some1" });
        ContentService.Save(child1);

        var child2 = ContentBuilder.CreateSimpleContent(contentType, "child 2 content", content.Id);
        child2.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "hello2", "world2" });
        ContentService.Save(child2);

        // Act
        ContentService.PublishBranch(content, PublishBranchFilter.IncludeUnpublished, content.AvailableCultures.ToArray());

        // Assert
        var propertyTypeId = contentType.PropertyTypes.Single(x => x.Alias == "tags").Id;

        using (var scope = ScopeProvider.CreateScope())
        {
            Assert.AreEqual(4, ScopeAccessor.AmbientScope.Database.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                new { nodeId = content.Id, propTypeId = propertyTypeId }));

            Assert.AreEqual(3, ScopeAccessor.AmbientScope.Database.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                new { nodeId = child1.Id, propTypeId = propertyTypeId }));

            Assert.AreEqual(2, ScopeAccessor.AmbientScope.Database.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                new { nodeId = child2.Id, propTypeId = propertyTypeId }));

            scope.Complete();
        }
    }

    [Test]
    public void Does_Not_Create_Tag_Data_For_Non_Published_Version()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        // create content type with a tag property
        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type",
            mandatoryProperties: true, defaultTemplateId: template.Id);
        CreateAndAddTagsPropertyType(contentType);
        ContentTypeService.Save(contentType);

        // create a content with tags and publish
        var content = ContentBuilder.CreateSimpleContent(contentType, "Tagged content");
        content.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "some", "tags" });
        ContentService.Save(content);
        ContentService.Publish(content, content.AvailableCultures.ToArray());

        // edit tags and save
        content.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "another", "world" },
            true);
        ContentService.Save(content);

        // the (edit) property does contain all tags
        Assert.AreEqual(5, content.Properties["tags"].GetValue().ToString().Split(',').Distinct().Count());

        // but the database still contains the initial two tags
        var propertyTypeId = contentType.PropertyTypes.Single(x => x.Alias == "tags").Id;
        using (var scope = ScopeProvider.CreateScope())
        {
            Assert.AreEqual(4, ScopeAccessor.AmbientScope.Database.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                new { nodeId = content.Id, propTypeId = propertyTypeId }));
            scope.Complete();
        }
    }

    [Test]
    public void Can_Replace_Tag_Data_To_Published_Content()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        // Arrange
        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type",
            mandatoryProperties: true, defaultTemplateId: template.Id);
        CreateAndAddTagsPropertyType(contentType);
        ContentTypeService.Save(contentType);

        var content = ContentBuilder.CreateSimpleContent(contentType, "Tagged content");

        // Act
        content.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "some", "tags" });
        ContentService.Save(content);
        ContentService.Publish(content, Array.Empty<string>());

        // Assert
        Assert.AreEqual(4, content.Properties["tags"].GetValue().ToString().Split(',').Distinct().Count());
        var propertyTypeId = contentType.PropertyTypes.Single(x => x.Alias == "tags").Id;
        using (var scope = ScopeProvider.CreateScope())
        {
            Assert.AreEqual(4, ScopeAccessor.AmbientScope.Database.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                new { nodeId = content.Id, propTypeId = propertyTypeId }));

            scope.Complete();
        }
    }

    [Test]
    public void Can_Append_Tag_Data_To_Published_Content()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        // Arrange
        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type",
            mandatoryProperties: true, defaultTemplateId: template.Id);
        CreateAndAddTagsPropertyType(contentType);
        ContentTypeService.Save(contentType);
        var content = ContentBuilder.CreateSimpleContent(contentType, "Tagged content");
        content.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "some", "tags" });
        ContentService.Save(content);
        ContentService.Publish(content, Array.Empty<string>());

        // Act
        content.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "another", "world" },
            true);
        ContentService.Save(content);
        ContentService.Publish(content, Array.Empty<string>());

        // Assert
        Assert.AreEqual(5, content.Properties["tags"].GetValue().ToString().Split(',').Distinct().Count());
        var propertyTypeId = contentType.PropertyTypes.Single(x => x.Alias == "tags").Id;
        using (var scope = ScopeProvider.CreateScope())
        {
            Assert.AreEqual(5, ScopeAccessor.AmbientScope.Database.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                new { nodeId = content.Id, propTypeId = propertyTypeId }));

            scope.Complete();
        }
    }

    [Test]
    public void Can_Remove_Tag_Data_To_Published_Content()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        // Arrange
        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type",
            mandatoryProperties: true, defaultTemplateId: template.Id);
        CreateAndAddTagsPropertyType(contentType);
        ContentTypeService.Save(contentType);
        var content = ContentBuilder.CreateSimpleContent(contentType, "Tagged content");
        content.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello", "world", "some", "tags" });
        ContentService.Save(content);
        ContentService.Publish(content, Array.Empty<string>());

        // Act
        content.RemoveTags(PropertyEditorCollection, DataTypeService, Serializer, "tags", new[] { "some", "world" });
        ContentService.Save(content);
        ContentService.Publish(content, Array.Empty<string>());

        // Assert
        Assert.AreEqual(2, content.Properties["tags"].GetValue().ToString().Split(',').Distinct().Count());
        var propertyTypeId = contentType.PropertyTypes.Single(x => x.Alias == "tags").Id;
        using (var scope = ScopeProvider.CreateScope())
        {
            Assert.AreEqual(2, ScopeAccessor.AmbientScope.Database.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM cmsTagRelationship WHERE nodeId=@nodeId AND propertyTypeId=@propTypeId",
                new { nodeId = content.Id, propTypeId = propertyTypeId }));

            scope.Complete();
        }
    }

    [Test]
    public void Does_Not_Save_Multiple_Tags_As_One_When_CSV_Storage()
    {
        // Arrange
        // set configuration
        var dataType = DataTypeService.GetDataType(1041);
        dataType.ConfigurationData = dataType.Editor!.GetConfigurationEditor()
            .FromConfigurationObject(
                new TagConfiguration { Group = "test", StorageType = TagsStorageType.Csv },
                ConfigurationEditorJsonSerializer);

        // updating the data type with the new configuration
        DataTypeService.Save(dataType);

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type",
            mandatoryProperties: true, defaultTemplateId: template.Id);
        CreateAndAddTagsPropertyType(contentType);

        ContentTypeService.Save(contentType);

        IContent content = ContentBuilder.CreateSimpleContent(contentType, "Tagged content");
        content.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello,world,tags", "new" });

        ContentService.Save(content);
        ContentService.Publish(content, content.AvailableCultures.ToArray());

        // Act
        content = ContentService.GetById(content.Id);
        var savedTags = content.Properties["tags"].GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer)
            .ToArray();

        // Assert
        Assert.AreEqual(4, savedTags.Length);
    }

    [Test]
    public void Can_Save_Tag_With_Comma_Separated_Values_As_One_When_JSON_Storage()
    {
        // Arrange
        // set configuration
        var dataType = DataTypeService.GetDataType(1041);
        dataType.ConfigurationData = dataType.Editor!.GetConfigurationEditor()
            .FromConfigurationObject(
                new TagConfiguration { Group = "test", StorageType = TagsStorageType.Json },
                ConfigurationEditorJsonSerializer);

        var configuration = dataType.ConfigurationObject as TagConfiguration;
        Assert.NotNull(configuration);
        Assert.AreEqual("test", configuration.Group);
        Assert.AreEqual(TagsStorageType.Json, configuration.StorageType);

        // updating the data type with the new configuration
        DataTypeService.Save(dataType);

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("umbMandatory", "Mandatory Doc Type",
            mandatoryProperties: true, defaultTemplateId: template.Id);
        CreateAndAddTagsPropertyType(contentType);

        ContentTypeService.Save(contentType);

        IContent content = ContentBuilder.CreateSimpleContent(contentType, "Tagged content");
        content.AssignTags(PropertyEditorCollection, DataTypeService, Serializer, "tags",
            new[] { "hello,world,tags", "new" });

        ContentService.Save(content);
        ContentService.Publish(content, content.AvailableCultures.ToArray());

        // Act
        content = ContentService.GetById(content.Id);
        var savedTags = content.Properties["tags"].GetTagsValue(PropertyEditorCollection, DataTypeService, Serializer)
            .ToArray();

        // Assert
        Assert.AreEqual(2, savedTags.Length);
    }

    private PropertyType CreateAndAddTagsPropertyType(
        ContentType contentType,
        ContentVariation variations = ContentVariation.Nothing)
    {
        var propertyType = new PropertyTypeBuilder()
            .WithPropertyEditorAlias("test")
            .WithAlias("tags")
            .WithDataTypeId(1041)
            .WithVariations(variations)
            .Build();
        contentType.PropertyGroups.First().PropertyTypes.Add(propertyType);
        contentType.Variations = variations;
        return propertyType;
    }
}
