// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.DynamicRoot;
using Umbraco.Cms.Core.DynamicRoot.QuerySteps;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
///     Tests covering the DynamicRootService resolution of dynamic roots for content pickers.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
[SuppressMessage("ReSharper", "NotNullOrRequiredMemberIsNotInitialized")]
internal sealed class DynamicRootServiceTests : UmbracoIntegrationTest
{
    public enum DynamicRootOrigin
    {
        Root,
        Parent,
        Current,
        Site,
        ByKey,
        ContentRoot
    }

    public enum DynamicRootStepAlias
    {
        NearestAncestorOrSelf,
        NearestDescendantOrSelf,
        FurthestDescendantOrSelf,
    }

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private ContentService ContentService => (ContentService)GetRequiredService<IContentService>();

    private DynamicRootService DynamicRootService => (DynamicRootService)GetRequiredService<IDynamicRootService>();

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
    public async Task SetupContentStructure()
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

        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        // DocTypes
        ContentTypeAct = ContentTypeBuilder.CreateSimpleContentType("act", "Act", defaultTemplateId: template.Id);
        ContentTypeAct.Key = new Guid("B3A50C84-5F6E-473A-A0B5-D41CBEC4EB36");
        await ContentTypeService.CreateAsync(ContentTypeAct, Constants.Security.SuperUserKey);

        ContentTypeStage = ContentTypeBuilder.CreateSimpleContentType("stage", "Stage", defaultTemplateId: template.Id);
        ContentTypeStage.Key = new Guid("C6DCDB3C-9D4B-4F91-9D1C-8C3B74AECA45");
        await ContentTypeService.CreateAsync(ContentTypeStage, Constants.Security.SuperUserKey);

        ContentTypeStages =
            ContentTypeBuilder.CreateSimpleContentType("stages", "Stages", defaultTemplateId: template.Id);
        ContentTypeStages.Key = new Guid("BFC4C6C1-51D0-4538-B818-042BEEA0461E");
        ContentTypeStages.AllowedContentTypes = [CreateContentTypeSort(ContentTypeStage, 0)];
        await ContentTypeService.CreateAsync(ContentTypeStages, Constants.Security.SuperUserKey);

        ContentTypeActs = ContentTypeBuilder.CreateSimpleContentType("acts", "Acts", defaultTemplateId: template.Id);
        ContentTypeActs.Key = new Guid("110B6BC7-59E0-427D-B350-E488786788E7");
        ContentTypeActs.AllowedContentTypes = [CreateContentTypeSort(ContentTypeAct, 0)];
        await ContentTypeService.CreateAsync(ContentTypeActs, Constants.Security.SuperUserKey);

        ContentTypeYear = ContentTypeBuilder.CreateSimpleContentType("year", "Year", defaultTemplateId: template.Id);
        ContentTypeYear.Key = new Guid("001E9029-6BF9-4A68-B11E-7730109E4E28");
        ContentTypeYear.AllowedContentTypes =
        [
            CreateContentTypeSort(ContentTypeStages, 0), CreateContentTypeSort(ContentTypeActs, 1),
        ];
        await ContentTypeService.CreateAsync(ContentTypeYear, Constants.Security.SuperUserKey);

        ContentTypeYears = ContentTypeBuilder.CreateSimpleContentType("years", "Years", defaultTemplateId: template.Id);
        ContentTypeYears.Key = new Guid("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522");
        ContentTypeYears.AllowedContentTypes = [CreateContentTypeSort(ContentTypeYear, 0)];
        await ContentTypeService.CreateAsync(ContentTypeYears, Constants.Security.SuperUserKey);

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

    /// <summary>
    ///     Verifies that using "Current" origin with NearestAncestorOrSelf filtered to the item's own doc type returns the item itself.
    /// </summary>
    [Test]
    public async Task GetDynamicRootsAsync_NearestAncestorOrSelfFilterOfOwnDocType_ReturnsSelf()
    {
        // Arrange
        var startNodeSelector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Current.ToString(),
            OriginKey = null,
            Context =
                new DynamicRootContext() { CurrentKey = ContentAct2022RanD.Key, ParentKey = ContentActs2022.Key },
            QuerySteps =
            [
                new DynamicRootQueryStep()
                {
                    Alias = DynamicRootStepAlias.NearestAncestorOrSelf.ToString(),
                    AnyOfDocTypeKeys = [ContentAct2022RanD.ContentType.Key],
                },
            ],
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

    /// <summary>
    ///     Verifies that NearestAncestorOrSelf from the "Root" origin returns empty when filtered to a doc type that doesn't appear above root.
    /// </summary>
    [Test]
    public async Task GetDynamicRootsAsync_NearestAncestorOrSelfWithOriginRoot_ReturnsEmptyList()
    {
        // Arrange
        var startNodeSelector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Root.ToString(),
            OriginKey = null,
            Context =
                new DynamicRootContext() { CurrentKey = ContentAct2022RanD.Key, ParentKey = ContentActs2022.Key },
            QuerySteps =
            [
                new DynamicRootQueryStep()
                {
                    Alias = DynamicRootStepAlias.NearestAncestorOrSelf.ToString(),
                    AnyOfDocTypeKeys = [ContentAct2022RanD.ContentType.Key],
                },
            ],
        };

        // Act
        var result = await DynamicRootService.GetDynamicRootsAsync(startNodeSelector);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, result.Count());
        });
    }

    /// <summary>
    ///     Verifies that DescendantOrSelf steps return empty when filtered to a doc type that has no matching descendants.
    /// </summary>
    [Test]
    [TestCase(DynamicRootStepAlias.NearestDescendantOrSelf)]
    [TestCase(DynamicRootStepAlias.FurthestDescendantOrSelf)]
    public async Task GetDynamicRootsAsync_DescendantOrSelfWithNoMatches_ReturnsEmptyList(
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
            QuerySteps =
            [
                new DynamicRootQueryStep()
                {
                    Alias = dynamicRootAlias.ToString(), AnyOfDocTypeKeys = [Guid.NewGuid()]
                },
            ],
        };

        // Act
        var result = await DynamicRootService.GetDynamicRootsAsync(startNodeSelector);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, result.Count());
        });
    }

    /// <summary>
    ///     Verifies that NearestDescendantOrSelf returns only the shallowest matching descendant, not deeper ones.
    /// </summary>
    [Test]
    public async Task GetDynamicRootsAsync_NearestDescendantOrSelf_FindsOnlyNearest()
    {
        // Arrange

        // Allow act to add acts
        ContentTypeAct.AllowedContentTypes =
            ContentTypeAct.AllowedContentTypes!.Union(
            [
                CreateContentTypeSort(ContentTypeActs, 0),
            ]);
        await ContentTypeService.UpdateAsync(ContentTypeAct, Constants.Security.SuperUserKey);

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
            QuerySteps =
            [
                new DynamicRootQueryStep()
                {
                    Alias = DynamicRootStepAlias.NearestDescendantOrSelf.ToString(),
                    AnyOfDocTypeKeys = [ContentTypeActs.Key],
                },
            ],
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

    /// <summary>
    ///     Verifies that FurthestDescendantOrSelf returns only the deepest matching descendant, not shallower ones.
    /// </summary>
    [Test]
    public async Task GetDynamicRootsAsync_FurthestDescendantOrSelf_FindsOnlyFurthest()
    {
        // Arrange

        // Allow act to add acts
        ContentTypeAct.AllowedContentTypes =
            ContentTypeAct.AllowedContentTypes!.Union([CreateContentTypeSort(ContentTypeActs, 0)]);
        await ContentTypeService.UpdateAsync(ContentTypeAct, Constants.Security.SuperUserKey);

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
            QuerySteps =
            [
                new DynamicRootQueryStep()
                {
                    Alias = DynamicRootStepAlias.FurthestDescendantOrSelf.ToString(),
                    AnyOfDocTypeKeys = [ContentTypeActs.Key],
                },
            ],
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

    /// <summary>
    ///     Verifies that chaining multiple query steps (ancestor then descendant) correctly navigates the tree to find the expected node.
    /// </summary>
    [Test]
    public async Task GetDynamicRootsAsync_MultipleQuerySteps_ReturnsExpectedNodes()
    {
        // Arrange
        var startNodeSelector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Current.ToString(),
            OriginKey = null,
            Context =
                new DynamicRootContext() { CurrentKey = ContentAct2022RanD.Key, ParentKey = ContentActs2022.Key },
            QuerySteps =
            [
                new DynamicRootQueryStep()
                {
                    Alias = DynamicRootStepAlias.NearestAncestorOrSelf.ToString(),
                    AnyOfDocTypeKeys = [ContentTypeYear.Key],
                },
                new DynamicRootQueryStep()
                {
                    Alias = DynamicRootStepAlias.NearestDescendantOrSelf.ToString(),
                    AnyOfDocTypeKeys = [ContentTypeStages.Key],
                },
            ],
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

    /// <summary>
    ///     Verifies that NearestDescendantOrSelf filtered to the origin's own doc type returns the origin itself.
    /// </summary>
    [Test]
    public async Task GetDynamicRootsAsync_NearestDescendantOrSelfFilterOfOwnDocType_ReturnsSelf()
    {
        // Arrange
        var startNodeSelector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Current.ToString(),
            OriginKey = null,
            Context = new DynamicRootContext() { CurrentKey = ContentYear2022.Key, ParentKey = ContentYears.Key },
            QuerySteps =
            [
                new DynamicRootQueryStep()
                {
                    Alias = DynamicRootStepAlias.NearestDescendantOrSelf.ToString(),
                    AnyOfDocTypeKeys = [ContentYear2022.ContentType.Key],
                },
            ],
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

    /// <summary>
    ///     Verifies that when no query steps are provided, the origin itself is returned as the root.
    /// </summary>
    [Test]
    public async Task GetDynamicRootsAsync_NoQuerySteps_ReturnsOrigin()
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

    /// <summary>
    ///     Verifies that chaining multiple query steps returns empty when the final step has no matching descendants.
    /// </summary>
    [Test]
    public async Task GetDynamicRootsAsync_MultipleQueryStepsWithNoResults_ReturnsEmptyList()
    {
        // Arrange
        var startNodeSelector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Current.ToString(),
            OriginKey = null,
            Context =
                new DynamicRootContext() { CurrentKey = ContentAct2022RanD.Key, ParentKey = ContentActs2022.Key },
            QuerySteps =
            [
                new DynamicRootQueryStep()
                {
                    Alias = DynamicRootStepAlias.NearestAncestorOrSelf.ToString(),
                    AnyOfDocTypeKeys = [ContentTypeYear.Key],
                },
                new DynamicRootQueryStep()
                {
                    Alias = DynamicRootStepAlias.NearestDescendantOrSelf.ToString(),
                    AnyOfDocTypeKeys = [ContentTypeStages.Key],
                },
                new DynamicRootQueryStep()
                {
                    Alias = DynamicRootStepAlias.NearestDescendantOrSelf.ToString(),
                    AnyOfDocTypeKeys = [ContentTypeYears.Key],
                },
                new DynamicRootQueryStep()
                {
                    Alias = DynamicRootStepAlias.NearestDescendantOrSelf.ToString(),
                    AnyOfDocTypeKeys = [ContentTypeYears.Key],
                },
            ],
        };

        // Act
        var result = (await DynamicRootService.GetDynamicRootsAsync(startNodeSelector)).ToList();

        // Assert
        Assert.AreEqual(0, result.Count());
    }

    /// <summary>
    ///     Verifies that "Current" origin with null CurrentKey and query steps resolves via the parent,
    ///     matching the scenario from issue #22213 where a content picker with dynamic root fails for new unsaved content.
    /// </summary>
    [Test]
    public async Task GetDynamicRootsAsync_CurrentOriginWithNullCurrentKeyAndQuerySteps_ResolvesViaParent()
    {
        // Arrange - simulates creating a new Act under Acts2022 (new content has no key yet)
        var startNodeSelector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Current.ToString(),
            OriginKey = null,
            Context = new DynamicRootContext()
            {
                CurrentKey = null,
                ParentKey = ContentActs2022.Key,
            },
            QuerySteps =
            [
                new DynamicRootQueryStep()
                {
                    Alias = DynamicRootStepAlias.NearestAncestorOrSelf.ToString(),
                    AnyOfDocTypeKeys = [ContentTypeYear.Key],
                },
                new DynamicRootQueryStep()
                {
                    Alias = DynamicRootStepAlias.NearestDescendantOrSelf.ToString(),
                    AnyOfDocTypeKeys = [ContentTypeStages.Key],
                },
            ],
        };

        // Act
        var result = (await DynamicRootService.GetDynamicRootsAsync(startNodeSelector)).ToList();

        // Assert - should resolve the same as if editing existing content at the same tree position
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, result.Count());
            CollectionAssert.Contains(result, ContentStages2022.Key);
        });
    }

    /// <summary>
    ///     Verifies that "Parent" origin returns the parent key from the context.
    /// </summary>
    [Test]
    public void FindOriginKey_ParentOrigin_ReturnsParentKey()
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

    /// <summary>
    ///     Verifies that "Current" origin returns the current key when the content item exists in the database.
    /// </summary>
    [Test]
    public void FindOriginKey_CurrentOriginWithExistingKey_ReturnsCurrentKey()
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

    /// <summary>
    ///     Verifies that "Current" origin returns null when the current key points to a non-existent entity
    ///     and the parent key also points to a non-existent entity.
    /// </summary>
    [Test]
    public void FindOriginKey_CurrentOriginWithNonExistentKey_ReturnsNull()
    {
        // Arrange
        var selector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Current.ToString(),
            OriginKey = null,
            Context = new DynamicRootContext() { CurrentKey = Guid.NewGuid(), ParentKey = Guid.NewGuid() },
        };

        // Act
        var result = DynamicRootService.FindOriginKey(selector);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    ///     Verifies that "Current" origin falls back to the parent key when the current key is null,
    ///     which is the case when creating new unsaved content.
    /// </summary>
    [Test]
    public void FindOriginKey_CurrentOriginWithNullCurrentKey_FallsBackToParentKey()
    {
        // Arrange
        var selector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Current.ToString(),
            OriginKey = null,
            Context = new DynamicRootContext() { CurrentKey = null, ParentKey = ContentYears.Key },
        };

        // Act
        var result = DynamicRootService.FindOriginKey(selector);

        // Assert
        Assert.AreEqual(ContentYears.Key, result);
    }

    /// <summary>
    ///     Verifies that "Root" origin traverses the content path upward and returns the topmost document node below the system root.
    /// </summary>
    [Test]
    public void FindOriginKey_RootOrigin_ReturnsContentTreeRoot()
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

    /// <summary>
    ///     Verifies that "ContentRoot" origin returns the system root key regardless of the current content position.
    /// </summary>
    [Test]
    public void FindOriginKey_ContentRootOrigin_ReturnsSystemRootKey()
    {
        // Arrange
        var selector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.ContentRoot.ToString(),
            OriginKey = null,
            Context = new DynamicRootContext()
            {
                CurrentKey = ContentAct2022RanD.Key,
                ParentKey = ContentActs2022.Key,
            },
        };

        // Act
        var result = DynamicRootService.FindOriginKey(selector);

        // Assert
        Assert.AreEqual(Constants.System.RootSystemKey, result);
    }

    /// <summary>
    ///     Verifies that "Site" origin returns the current node when it has a domain assigned to it.
    /// </summary>
    [Test]
    public async Task FindOriginKey_SiteOriginWithDomainOnSelf_ReturnsSelf()
    {
        // Arrange
        var origin = ContentYear2022;
        var selector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Site.ToString(),
            OriginKey = origin.Key,
            Context = new DynamicRootContext() { CurrentKey = origin.Key, ParentKey = ContentYears.Key },
        };

        await DomainService.UpdateDomainsAsync(origin.Key, new DomainsUpdateModel
        {
            Domains =
            [
                new DomainModel
                {
                    IsoCode = "en-US",
                    DomainName = "http://test.umbraco.com",
                },
            ],
        });

        // Act
        var result = DynamicRootService.FindOriginKey(selector);

        // Assert
        Assert.AreEqual(origin.Key, result);
    }

    /// <summary>
    ///     Verifies that "Site" origin returns the nearest ancestor that has a domain assigned.
    /// </summary>
    [Test]
    public async Task FindOriginKey_SiteOriginWithDomainOnAncestor_ReturnsAncestorWithDomain()
    {
        // Arrange
        var origin = ContentAct2022RanD;
        var selector = new DynamicRootNodeQuery()
        {
            OriginAlias = DynamicRootOrigin.Site.ToString(),
            OriginKey = origin.Key,
            Context = new DynamicRootContext() { CurrentKey = origin.Key, ParentKey = ContentActs2022.Key },
        };

        await DomainService.UpdateDomainsAsync(ContentYears.Key, new DomainsUpdateModel
        {
            Domains =
            [
                new DomainModel
                {
                    IsoCode = "en-US",
                    DomainName = "http://test.umbraco.com",
                },
            ],
        });

        // Act
        var result = DynamicRootService.FindOriginKey(selector);

        // Assert
        Assert.AreEqual(ContentYears.Key, result);
    }

    /// <summary>
    ///     Verifies that "Site" origin falls back to the content tree root when no domain is assigned to any ancestor.
    /// </summary>
    [Test]
    public void FindOriginKey_SiteOriginWithNoDomain_FallsBackToRoot()
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

    /// <summary>
    ///     Verifies that all origin types return null when both context keys reference non-existent entities.
    /// </summary>
    [TestCase(DynamicRootOrigin.ByKey)]
    [TestCase(DynamicRootOrigin.Parent)]
    [TestCase(DynamicRootOrigin.Current)]
    [TestCase(DynamicRootOrigin.Root)]
    [TestCase(DynamicRootOrigin.Site)]
    public void FindOriginKey_WithRandomNonExistentKey_ReturnsNull(DynamicRootOrigin origin)
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

    /// <summary>
    ///     Verifies that all origin types still resolve when the context keys reference a trashed content item.
    /// </summary>
    [TestCase(DynamicRootOrigin.ByKey)]
    [TestCase(DynamicRootOrigin.Parent)]
    [TestCase(DynamicRootOrigin.Root)]
    [TestCase(DynamicRootOrigin.Site)]
    [TestCase(DynamicRootOrigin.Current)]
    public void FindOriginKey_WithTrashedKey_ReturnsNonNull(DynamicRootOrigin origin)
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

    /// <summary>
    ///     Verifies that all origin types return null when the context keys reference a content type key (wrong entity type).
    /// </summary>
    [TestCase(DynamicRootOrigin.ByKey)]
    [TestCase(DynamicRootOrigin.Parent)]
    [TestCase(DynamicRootOrigin.Root)]
    [TestCase(DynamicRootOrigin.Site)]
    [TestCase(DynamicRootOrigin.Current)]
    public void FindOriginKey_WithContentTypeKey_ReturnsNull(DynamicRootOrigin origin)
    {
        // Arrange
        var contentTypeKey = ContentTypeYears.Key;
        var selector = new DynamicRootNodeQuery()
        {
            OriginAlias = origin.ToString(),
            OriginKey = contentTypeKey,
            Context = new DynamicRootContext() { CurrentKey = contentTypeKey, ParentKey = contentTypeKey },
        };

        // Act
        var result = DynamicRootService.FindOriginKey(selector);

        // Assert
        Assert.IsNull(result);
    }

    private static ContentTypeSort CreateContentTypeSort(ContentType contentType, int sortOrder)
        => new(contentType.Key, sortOrder, contentType.Alias);
}
