// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.DynamicRoot;
using Umbraco.Cms.Core.DynamicRoot.QuerySteps;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
///     Tests covering the DynamicRootService
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
[SuppressMessage("ReSharper", "NotNullOrRequiredMemberIsNotInitialized")]
public class DynamicRootServiceTests : UmbracoIntegrationTest
{
    public enum DynamicRootOrigin
    {
        Root,
        Parent,
        Current,
        Site,
        ByKey
    }

    public enum DynamicRootStepAlias
    {
        NearestAncestorOrSelf,
        NearestDescendantOrSelf,
        FurthestDescendantOrSelf,
    }

    protected IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    protected IFileService FileService => GetRequiredService<IFileService>();

    protected ContentService ContentService => (ContentService)GetRequiredService<IContentService>();

    private DynamicRootService DynamicRootService => (GetRequiredService<IDynamicRootService>() as DynamicRootService)!;

    private IDomainService DomainService => GetRequiredService<IDomainService>();

    private ContentType ContentTypeYears { get; set; }

    private ContentType ContentTypeYear { get; set; }

    private ContentType ContentTypeAct { get; set; }

    private ContentType ContentTypeActs { get; set; }

    private ContentType ContentTypeStages { get; set; }

    private ContentType ContentTypeStage { get; set; }

    private Content ContentYears { get; set; }

    private Content ContentYear2022 { get; set; }

    private Content ContentActs2022 { get; set; }

    private Content ContentAct2022RanD { get; set; }

    private Content ContentStages2022 { get; set; }

    private Content ContentStage2022Red { get; set; }

    private Content ContentStage2022Blue { get; set; }

    private Content ContentYear2023 { get; set; }

    private Content ContentYear2024 { get; set; }

    private Content Trashed { get; set; }


    [SetUp]
    public new void Setup()
    {
        // Root
        //    - Years (years)
        //      - 2022 (year)
        //          - Acts
        //                - Ran-D (Act)
        //          - Stages (stages)
        //                - Red (Stage)
        //                - Blue (Stage)
        //      - 2023
        //          - Acts
        //          - Stages
        //      - 2024
        //          - Acts
        //          - Stages

        // NOTE Maybe not the best way to create/save test data as we are using the services, which are being tested.
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        // DocTypes
        ContentTypeAct = ContentTypeBuilder.CreateSimpleContentType("act", "Act", defaultTemplateId: template.Id);
        ContentTypeAct.Key = new Guid("B3A50C84-5F6E-473A-A0B5-D41CBEC4EB36");
        ContentTypeService.Save(ContentTypeAct);

        ContentTypeStage = ContentTypeBuilder.CreateSimpleContentType("stage", "Stage", defaultTemplateId: template.Id);
        ContentTypeStage.Key = new Guid("C6DCDB3C-9D4B-4F91-9D1C-8C3B74AECA45");
        ContentTypeService.Save(ContentTypeStage);

        ContentTypeStages =
            ContentTypeBuilder.CreateSimpleContentType("stages", "Stages", defaultTemplateId: template.Id);
        ContentTypeStages.Key = new Guid("BFC4C6C1-51D0-4538-B818-042BEEA0461E");
        ContentTypeStages.AllowedContentTypes = new[] { new ContentTypeSort(ContentTypeStage.Id, 0) };
        ContentTypeService.Save(ContentTypeStages);

        ContentTypeActs = ContentTypeBuilder.CreateSimpleContentType("acts", "Acts", defaultTemplateId: template.Id);
        ContentTypeActs.Key = new Guid("110B6BC7-59E0-427D-B350-E488786788E7");
        ContentTypeActs.AllowedContentTypes = new[] { new ContentTypeSort(ContentTypeAct.Id, 0) };
        ContentTypeService.Save(ContentTypeActs);

        ContentTypeYear = ContentTypeBuilder.CreateSimpleContentType("year", "Year", defaultTemplateId: template.Id);
        ContentTypeYear.Key = new Guid("001E9029-6BF9-4A68-B11E-7730109E4E28");
        ContentTypeYear.AllowedContentTypes = new[]
        {
            new ContentTypeSort(ContentTypeStages.Id, 0), new ContentTypeSort(ContentTypeActs.Id, 1),
        };
        ContentTypeService.Save(ContentTypeYear);

        ContentTypeYears = ContentTypeBuilder.CreateSimpleContentType("years", "Years", defaultTemplateId: template.Id);
        ContentTypeYears.Key = new Guid("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522");
        ContentTypeActs.AllowedContentTypes = new[] { new ContentTypeSort(ContentTypeYear.Id, 0) };
        ContentTypeService.Save(ContentTypeYears);

        ContentYears = ContentBuilder.CreateSimpleContent(ContentTypeYears, "Years");
        ContentYears.Key = new Guid("CD3BBE28-D03F-422B-9DC6-A0E591543A8E");
        ContentService.Save(ContentYears, -1);

        ContentYear2022 = ContentBuilder.CreateSimpleContent(ContentTypeYear, "2022", ContentYears.Id);
        ContentYear2022.Key = new Guid("9B3066E3-3CE9-4DF6-82C7-444236FF4DAC");
        ContentService.Save(ContentYear2022, -1);

        ContentActs2022 = ContentBuilder.CreateSimpleContent(ContentTypeActs, "Acts", ContentYear2022.Id);
        ContentActs2022.Key = new Guid("6FD7F030-269D-45BE-BEB4-030FF8764B6D");
        ContentService.Save(ContentActs2022, -1);

        ContentAct2022RanD = ContentBuilder.CreateSimpleContent(ContentTypeAct, "Ran-D", ContentActs2022.Id);
        ContentAct2022RanD.Key = new Guid("9BE4C615-240E-4616-BB65-C1F2DE9C3873");
        ContentService.Save(ContentAct2022RanD, -1);

        ContentStages2022 = ContentBuilder.CreateSimpleContent(ContentTypeStages, "Stages", ContentYear2022.Id);
        ContentStages2022.Key = new Guid("1FF59D2F-FCE8-455B-98A6-7686BF41FD33");
        ContentService.Save(ContentStages2022, -1);

        ContentStage2022Red = ContentBuilder.CreateSimpleContent(ContentTypeStage, "Red", ContentStages2022.Id);
        ContentStage2022Red.Key = new Guid("F1C4E4D6-FFDE-4053-9240-EC594CE2A073");
        ContentService.Save(ContentStage2022Red, -1);

        ContentStage2022Blue = ContentBuilder.CreateSimpleContent(ContentTypeStage, "Blue", ContentStages2022.Id);
        ContentStage2022Blue.Key = new Guid("085311BB-2E75-4FB3-AC30-05F8CF2D3CB5");
        ContentService.Save(ContentStage2022Blue, -1);

        ContentYear2023 = ContentBuilder.CreateSimpleContent(ContentTypeYear, "2023", ContentYears.Id);
        ContentYear2023.Key = new Guid("2A863C61-8422-4863-8818-795711FFF0FC");
        ContentService.Save(ContentYear2023, -1);

        ContentYear2024 = ContentBuilder.CreateSimpleContent(ContentTypeYear, "2024", ContentYears.Id);
        ContentYear2024.Key = new Guid("E547A970-3923-4EF0-9EDA-10CB83FF038F");
        ContentService.Save(ContentYear2024, -1);

        Trashed = ContentBuilder.CreateSimpleContent(ContentTypeYears, "Text Page Deleted", -20);
        Trashed.Trashed = true;
        ContentService.Save(Trashed, -1);
    }


    [Test]
    public async Task GetDynamicRoots__With_NearestAncestorOrSelf_and_filter_of_own_doc_type_should_return_self()
    {
        // Arrange
        var startNodeSelector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Current.ToString(),
            OriginKey = null,
            Context =
                new DynamicRootContext() { CurrentKey = ContentAct2022RanD.Key, ParentKey = ContentActs2022.Key },
            QuerySteps = new DynamicRootQueryStep[]
            {
                new DynamicRootQueryStep()
                {
                    Alias = DynamicRootStepAlias.NearestAncestorOrSelf.ToString(),
                    AnyOfDocTypeKeys = new[] { ContentAct2022RanD.ContentType.Key },
                },
            },
        };

        // Act
        var result = (await DynamicRootService.GetDynamicRootsAsync(startNodeSelector)).ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, result.Count());
            CollectionAssert.Contains(result, startNodeSelector.Context.CurrentKey.Value);
        });
    }

    [Test]
    public async Task GetDynamicRoots__With_NearestAncestorOrSelf_and_origin_root_should_return_empty_list()
    {
        // Arrange
        var startNodeSelector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Root.ToString(),
            OriginKey = null,
            Context =
                new DynamicRootContext() { CurrentKey = ContentAct2022RanD.Key, ParentKey = ContentActs2022.Key },
            QuerySteps = new DynamicRootQueryStep[]
            {
                new DynamicRootQueryStep()
                {
                    Alias = DynamicRootStepAlias.NearestAncestorOrSelf.ToString(),
                    AnyOfDocTypeKeys = new[] { ContentAct2022RanD.ContentType.Key },
                },
            },
        };

        // Act
        var result = await DynamicRootService.GetDynamicRootsAsync(startNodeSelector);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, result.Count());
        });
    }

    [Test]
    [TestCase(DynamicRootStepAlias.NearestDescendantOrSelf)]
    [TestCase(DynamicRootStepAlias.FurthestDescendantOrSelf)]
    public async Task
        GetDynamicRoots__DescendantOrSelf_must_handle_when_there_is_not_found_any_and_level_becomes_impossible_to_get(
            DynamicRootStepAlias dynamicRootAlias)
    {
        // Arrange
        var startNodeSelector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Current.ToString(),
            OriginKey = null,
            Context = new DynamicRootContext()
            {
                CurrentKey = ContentAct2022RanD.Key, ParentKey = ContentActs2022.Key,
            },
            QuerySteps = new DynamicRootQueryStep[]
            {
                new DynamicRootQueryStep()
                {
                    Alias = dynamicRootAlias.ToString(), AnyOfDocTypeKeys = new[] { Guid.NewGuid() }
                },
            },
        };

        // Act
        var result = await DynamicRootService.GetDynamicRootsAsync(startNodeSelector);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, result.Count());
        });
    }

    [Test]
    public async Task GetDynamicRoots__NearestDescendantOrSelf__has_to_find_only_the_nearest()
    {
        // Arrange

        // Allow atc to add acts
        ContentTypeAct.AllowedContentTypes =
            ContentTypeAct.AllowedContentTypes!.Union(new ContentTypeSort[]
            {
                new ContentTypeSort(ContentTypeActs.Id, 0),
            });
        ContentTypeService.Save(ContentTypeAct);

        var contentNewActs = ContentBuilder.CreateSimpleContent(ContentTypeActs, "new Acts", ContentAct2022RanD.Id);
        contentNewActs.Key = new Guid("EA309F8C-8F1A-4C19-9613-2F950CDDCB8D");
        ContentService.Save(contentNewActs, -1);

        var contentNewAct =
            ContentBuilder.CreateSimpleContent(ContentTypeAct, "new act under new acts", contentNewActs.Id);
        contentNewAct.Key = new Guid("7E14BA13-C998-46DE-92AE-8E1C18CCEE02");
        ContentService.Save(contentNewAct, -1);


        var startNodeSelector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Root.ToString(),
            OriginKey = null,
            Context = new DynamicRootContext() { CurrentKey = contentNewAct.Key, ParentKey = contentNewActs.Key },
            QuerySteps = new[]
            {
                new DynamicRootQueryStep()
                {
                    Alias = DynamicRootStepAlias.NearestDescendantOrSelf.ToString(),
                    AnyOfDocTypeKeys = new[] { ContentTypeActs.Key },
                },
            },
        };

        // Act
        var result = (await DynamicRootService.GetDynamicRootsAsync(startNodeSelector)).ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, result.Count());
            CollectionAssert.Contains(result, ContentActs2022.Key);
        });
    }

    [Test]
    public async Task GetDynamicRoots__FurthestDescendantOrSelf__has_to_find_only_the_furthest()
    {
        // Arrange

        // Allow act to add acts
        ContentTypeAct.AllowedContentTypes =
            ContentTypeAct.AllowedContentTypes!.Union(new[] { new ContentTypeSort(ContentTypeActs.Id, 0) });
        ContentTypeService.Save(ContentTypeAct);

        var contentNewActs = ContentBuilder.CreateSimpleContent(ContentTypeActs, "new Acts", ContentAct2022RanD.Id);
        contentNewActs.Key = new Guid("EA309F8C-8F1A-4C19-9613-2F950CDDCB8D");
        ContentService.Save(contentNewActs, -1);

        var contentNewAct =
            ContentBuilder.CreateSimpleContent(ContentTypeAct, "new act under new acts", contentNewActs.Id);
        contentNewAct.Key = new Guid("7E14BA13-C998-46DE-92AE-8E1C18CCEE02");
        ContentService.Save(contentNewAct, -1);


        var startNodeSelector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Root.ToString(),
            OriginKey = null,
            Context = new DynamicRootContext() { CurrentKey = contentNewAct.Key, ParentKey = contentNewActs.Key },
            QuerySteps = new[]
            {
                new DynamicRootQueryStep()
                {
                    Alias = DynamicRootStepAlias.FurthestDescendantOrSelf.ToString(),
                    AnyOfDocTypeKeys = new[] { ContentTypeActs.Key },
                },
            },
        };

        // Act
        var result = (await DynamicRootService.GetDynamicRootsAsync(startNodeSelector)).ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, result.Count());
            CollectionAssert.Contains(result, contentNewActs.Key);
        });
    }

    [Test]
    public async Task GetDynamicRoots__With_multiple_filters()
    {
        // Arrange
        var startNodeSelector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Current.ToString(),
            OriginKey = null,
            Context =
                new DynamicRootContext() { CurrentKey = ContentAct2022RanD.Key, ParentKey = ContentActs2022.Key },
            QuerySteps = new[]
            {
                new DynamicRootQueryStep()
                {
                    Alias = DynamicRootStepAlias.NearestAncestorOrSelf.ToString(),
                    AnyOfDocTypeKeys = new[] { ContentTypeYear.Key },
                },
                new DynamicRootQueryStep()
                {
                    Alias = DynamicRootStepAlias.NearestDescendantOrSelf.ToString(),
                    AnyOfDocTypeKeys = new[] { ContentTypeStages.Key },
                },
            },
        };

        // Act
        var result = (await DynamicRootService.GetDynamicRootsAsync(startNodeSelector)).ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, result.Count());
            CollectionAssert.Contains(result, ContentStages2022.Key);
        });
    }

    [Test]
    public async Task GetDynamicRoots__With_NearestDescendantOrSelf_and_filter_of_own_doc_type_should_return_self()
    {
        // Arrange
        var startNodeSelector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Current.ToString(),
            OriginKey = null,
            Context = new DynamicRootContext() { CurrentKey = ContentYear2022.Key, ParentKey = ContentYears.Key },
            QuerySteps = new DynamicRootQueryStep[]
            {
                new DynamicRootQueryStep()
                {
                    Alias = DynamicRootStepAlias.NearestDescendantOrSelf.ToString(),
                    AnyOfDocTypeKeys = new[] { ContentYear2022.ContentType.Key },
                },
            },
        };

        // Act
        var result = (await DynamicRootService.GetDynamicRootsAsync(startNodeSelector)).ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, result.Count());
            CollectionAssert.Contains(result, startNodeSelector.Context.CurrentKey.Value);
        });
    }


    [Test]
    public async Task GetDynamicRoots__With_no_filters_should_return_what_origin_finds()
    {
        // Arrange
        var startNodeSelector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Parent.ToString(),
            OriginKey = null,
            Context = new DynamicRootContext() { CurrentKey = ContentYear2022.Key, ParentKey = ContentYears.Key },
            QuerySteps = Array.Empty<DynamicRootQueryStep>(),
        };

        // Act
        var result = (await DynamicRootService.GetDynamicRootsAsync(startNodeSelector)).ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, result.Count());
            CollectionAssert.Contains(result, startNodeSelector.Context.ParentKey);
        });
    }


    [Test]
    public void CalculateOriginKey__Parent_should_just_return_the_parent_key()
    {
        // Arrange
        var selector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Parent.ToString(),
            OriginKey = null,
            Context = new DynamicRootContext() { CurrentKey = ContentYear2022.Key, ParentKey = ContentYears.Key },
        };

        // Act
        var result = DynamicRootService.FindOriginKey(selector);

        // Assert
        Assert.AreEqual(selector.Context.ParentKey, result);
    }

    [Test]
    public void CalculateOriginKey__Current_should_just_return_the_current_key_when_it_exists()
    {
        // Arrange
        var selector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Current.ToString(),
            OriginKey = null,
            Context = new DynamicRootContext() { CurrentKey = ContentYear2022.Key, ParentKey = ContentYears.Key },
        };

        // Act
        var result = DynamicRootService.FindOriginKey(selector);

        // Assert
        Assert.AreEqual(selector.Context.CurrentKey, result);
    }

    [Test]
    public void CalculateOriginKey__Current_should_just_return_null_when_it_does_not_exist()
    {
        // Arrange
        var selector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Current.ToString(),
            OriginKey = null,
            Context = new DynamicRootContext() { CurrentKey = Guid.NewGuid(), ParentKey = ContentYears.Key },
        };

        // Act
        var result = DynamicRootService.FindOriginKey(selector);

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public void CalculateOriginKey__Root_should_traverse_the_path_and_take_the_first_level_in_the_root()
    {
        // Arrange
        var selector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Root.ToString(),
            OriginKey = null,
            Context = new DynamicRootContext()
            {
                CurrentKey = ContentAct2022RanD.Key, ParentKey = ContentActs2022.Key,
            },
        };

        // Act
        var result = DynamicRootService.FindOriginKey(selector);

        // Assert
        Assert.AreEqual(ContentYears.Key, result);
    }

    [Test]
    public void CalculateOriginKey__Site_should_return_the_first_with_an_assigned_domain_also_it_self()
    {
        // Arrange
        var origin = ContentYear2022;
        var selector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Site.ToString(),
            OriginKey = origin.Key,
            Context = new DynamicRootContext() { CurrentKey = origin.Key, ParentKey = ContentYears.Key },
        };

        DomainService.Save(
            new UmbracoDomain("http://test.umbraco.com") { RootContentId = origin.Id, LanguageIsoCode = "en-us" });

        // Act
        var result = DynamicRootService.FindOriginKey(selector);

        // Assert
        Assert.AreEqual(origin.Key, result);
    }

    [Test]
    public void CalculateOriginKey__Site_should_return_the_first_with_an_assigned_domain()
    {
        // Arrange
        var origin = ContentAct2022RanD;
        var selector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Site.ToString(),
            OriginKey = origin.Key,
            Context = new DynamicRootContext() { CurrentKey = origin.Key, ParentKey = ContentActs2022.Key },
        };

        DomainService.Save(new UmbracoDomain("http://test.umbraco.com")
        {
            RootContentId = ContentYears.Id,
            LanguageIsoCode = "en-us",
        });

        // Act
        var result = DynamicRootService.FindOriginKey(selector);

        // Assert
        Assert.AreEqual(ContentYears.Key, result);
    }

    [Test]
    public void CalculateOriginKey__Site_should_fallback_to_root_when_no_domain_is_assigned()
    {
        // Arrange
        var selector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Site.ToString(),
            OriginKey = ContentActs2022.Key,
            Context = new DynamicRootContext()
            {
                CurrentKey = ContentAct2022RanD.Key,
                ParentKey = ContentActs2022.Key,
            },
        };

        // Act
        var result = DynamicRootService.FindOriginKey(selector);

        // Assert
        Assert.AreEqual(ContentYears.Key, result);
    }

    [Test]
    [TestCase(DynamicRootOrigin.ByKey)]
    [TestCase(DynamicRootOrigin.Parent)]
    [TestCase(DynamicRootOrigin.Root)]
    [TestCase(DynamicRootOrigin.Site)]
    [TestCase(DynamicRootOrigin.Site)]
    public void CalculateOriginKey__with_a_random_key_should_return_null(DynamicRootOrigin origin)
    {
        // Arrange
        var randomKey = Guid.NewGuid();
        var selector = new DynamicRootNodeQuery()
        {
            OriginAlias = origin.ToString(),
            OriginKey = randomKey,
            Context = new DynamicRootContext() { CurrentKey = randomKey, ParentKey = Guid.NewGuid() },
        };

        // Act
        var result = DynamicRootService.FindOriginKey(selector);

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    [TestCase(DynamicRootOrigin.ByKey)]
    [TestCase(DynamicRootOrigin.Parent)]
    [TestCase(DynamicRootOrigin.Root)]
    [TestCase(DynamicRootOrigin.Site)]
    [TestCase(DynamicRootOrigin.Current)]
    public void CalculateOriginKey__with_a_trashed_key_should_still_be_allowed(DynamicRootOrigin origin)
    {
        // Arrange
        var trashedKey = Trashed.Key;
        var selector = new DynamicRootNodeQuery()
        {
            OriginAlias = origin.ToString(),
            OriginKey = trashedKey,
            Context = new DynamicRootContext() { CurrentKey = trashedKey, ParentKey = trashedKey },
        };

        // Act
        var result = DynamicRootService.FindOriginKey(selector);

        // Assert
        Assert.IsNotNull(result);
    }

    [Test]
    [TestCase(DynamicRootOrigin.ByKey)]
    [TestCase(DynamicRootOrigin.Parent)]
    [TestCase(DynamicRootOrigin.Root)]
    [TestCase(DynamicRootOrigin.Site)]
    [TestCase(DynamicRootOrigin.Current)]
    public void CalculateOriginKey__with_a_ContentType_key_should_return_null(DynamicRootOrigin origin)
    {
        // Arrange
        var contentTypeKey = ContentTypeYears.Key;
        var selector = new DynamicRootNodeQuery()
        {
            OriginAlias = origin.ToString(),
            OriginKey = contentTypeKey,
            Context = new DynamicRootContext() { CurrentKey = contentTypeKey, ParentKey = contentTypeKey }
        };

        // Act
        var result = DynamicRootService.FindOriginKey(selector);

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public async Task GetDynamicRoots__With_multiple_filters_that_do_not_return_any_results()
    {
        // Arrange
        var startNodeSelector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Current.ToString(),
            OriginKey = null,
            Context =
                new DynamicRootContext() { CurrentKey = ContentAct2022RanD.Key, ParentKey = ContentActs2022.Key },
            QuerySteps = new[]
            {
                new DynamicRootQueryStep()
                {
                    Alias = DynamicRootStepAlias.NearestAncestorOrSelf.ToString(),
                    AnyOfDocTypeKeys = new[] { ContentTypeYear.Key },
                },
                new DynamicRootQueryStep()
                {
                    Alias = DynamicRootStepAlias.NearestDescendantOrSelf.ToString(),
                    AnyOfDocTypeKeys = new[] { ContentTypeStages.Key },
                },
                new DynamicRootQueryStep()
                {
                    Alias = DynamicRootStepAlias.NearestDescendantOrSelf.ToString(),
                    AnyOfDocTypeKeys = new[] { ContentTypeYears.Key },
                },
                new DynamicRootQueryStep()
                {
                    Alias = DynamicRootStepAlias.NearestDescendantOrSelf.ToString(),
                    AnyOfDocTypeKeys = new[] { ContentTypeYears.Key },
                },
            },
        };

        // Act
        var result = (await DynamicRootService.GetDynamicRootsAsync(startNodeSelector)).ToList();

        // Assert
        Assert.AreEqual(0, result.Count());
    }
}
