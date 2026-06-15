using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ElementContainerServiceTests
{
    private IRelationService RelationService => GetRequiredService<IRelationService>();

    [Test]
    public async Task Can_Purge_Empty_Containers_From_Recycle_Bin()
    {
        for (var i = 0; i < 5; i++)
        {
            var key = Guid.NewGuid();
            await ElementContainerService.CreateAsync(key, $"Root Container {i}", null, Constants.Security.SuperUserKey);
            await ElementContainerService.MoveToRecycleBinAsync(key, Constants.Security.SuperUserKey);
        }

        Assert.That(EntityService.GetDescendants(Constants.System.RecycleBinElement).Count(), Is.EqualTo(5));

        var emptyResult = await ElementContainerService.EmptyRecycleBinAsync(Constants.Security.SuperUserKey);

        Assert.That((bool)emptyResult, Is.True);
        Assert.That(EntityService.GetDescendants(Constants.System.RecycleBinElement).Count(), Is.EqualTo(0));
    }

    [Test]
    [LongRunning]
    public async Task Can_Purge_Container_With_Descendant_Containers_And_Lots_Of_Elements_From_Recycle_Bin()
    {
        var setup = await CreateContainerWithDescendantContainersAndLotsOfElements(false);
        await ElementContainerService.MoveToRecycleBinAsync(setup.RootContainerKey, Constants.Security.SuperUserKey);
        Assert.That(EntityService.GetDescendants(Constants.System.RecycleBinElement).Count(), Is.Not.EqualTo(0));

        var emptyResult = await ElementContainerService.EmptyRecycleBinAsync(Constants.Security.SuperUserKey);

        Assert.That((bool)emptyResult, Is.True);
        Assert.That(EntityService.GetDescendants(Constants.System.RecycleBinElement).Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task Emptying_The_Recycle_Bin_Does_Not_Affect_Items_Outside_The_Recycle_Bin()
    {
        var elementType = await CreateElementType();

        await CreateElement(elementType.Key, null);
        var elementToBin = await CreateElement(elementType.Key, null);
        await ElementEditingService.MoveToRecycleBinAsync(elementToBin.Key, Constants.Security.SuperUserKey);
        for (var i = 0; i < 5; i++)
        {
            var key = Guid.NewGuid();
            await ElementContainerService.CreateAsync(key, $"Root Container {i}", null, Constants.Security.SuperUserKey);
            await CreateElement(elementType.Key, key);
            if (i % 2 == 0)
            {
                await ElementContainerService.MoveToRecycleBinAsync(key, Constants.Security.SuperUserKey);
            }
        }

        Assert.That(EntityService.GetRootEntities(UmbracoObjectTypes.Element).Count(), Is.EqualTo(1));
        var rootEntities = EntityService.GetRootEntities(UmbracoObjectTypes.ElementContainer).ToArray();
        Assert.That(rootEntities, Has.Length.EqualTo(2));
        foreach (var rootEntity in rootEntities)
        {
            Assert.That(GetFolderChildren(rootEntity.Key), Has.Length.EqualTo(1));
        }

        // trashed root element + three trashed folders, each containing a single element
        Assert.That(EntityService.GetDescendants(Constants.System.RecycleBinElement).Count(), Is.EqualTo(7));

        var emptyResult = await ElementContainerService.EmptyRecycleBinAsync(Constants.Security.SuperUserKey);
        Assert.That((bool)emptyResult, Is.True);

        Assert.That(EntityService.GetRootEntities(UmbracoObjectTypes.Element).Count(), Is.EqualTo(1));
        rootEntities = EntityService.GetRootEntities(UmbracoObjectTypes.ElementContainer).ToArray();
        Assert.That(rootEntities, Has.Length.EqualTo(2));
        foreach (var rootEntity in rootEntities)
        {
            Assert.That(GetFolderChildren(rootEntity.Key), Has.Length.EqualTo(1));
        }

        Assert.That(EntityService.GetDescendants(Constants.System.RecycleBinElement).Count(), Is.EqualTo(0));
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureDisableDeleteWhenReferenced))]
    public async Task Emptying_Recycle_Bin_With_DisableDeleteWhenReferenced_Deletes_All_Unreferenced_Items_Across_Multiple_Pages()
    {
        var elementType = await CreateElementType();

        // Create a container outside the recycle bin to use as the "referencing" entity
        var referencingContainerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(referencingContainerKey, "Referencing Container", null, Constants.Security.SuperUserKey);
        var referencingElement = await CreateElement(elementType.Key, referencingContainerKey);

        // Create a container with elements
        var containerKey = Guid.NewGuid();
        await ElementContainerService.CreateAsync(containerKey, "Trashed Container", null, Constants.Security.SuperUserKey);

        var referencedElements = new List<IElement>();
        var unreferencedElements = new List<IElement>();

        // Create more elements than the test page size, with some being referenced
        // Using a small page size (5) means we only need ~15 items to test multiple pages
        const int testPageSize = 5;
        const int totalElements = testPageSize * 3;
        for (var i = 0; i < totalElements; i++)
        {
            var element = await CreateElement(elementType.Key, containerKey);

            // Mark every 5th element as referenced (one per page)
            if (i % testPageSize == 0)
            {
                RelateElements(referencingElement, element);
                referencedElements.Add(element);
            }
            else
            {
                unreferencedElements.Add(element);
            }
        }

        // Move the container to recycle bin
        var trashResult =
            await ElementContainerService.MoveToRecycleBinAsync(containerKey, Constants.Security.SuperUserKey);
        Assert.That(trashResult.Success, Is.True);

        // Verify initial state
        var initialRecycleBinCount = EntityService.GetDescendants(Constants.System.RecycleBinElement).Count();
        Assert.That(initialRecycleBinCount, Is.EqualTo(totalElements + 1), "Should have all elements plus the container in recycle bin");

        // Empty the recycle bin using the internal method with a small page size
        var emptyResult = await ((ElementContainerService)ElementContainerService)
            .EmptyRecycleBinAsync(Constants.Security.SuperUserKey, testPageSize);
        Assert.That(emptyResult.Success, Is.True);

        // Verify that all unreferenced elements were deleted
        foreach (var element in unreferencedElements)
        {
            var found = await ElementEditingService.GetAsync(element.Key);
            Assert.That(found, Is.Null, $"Unreferenced element {element.Key} should have been deleted");
        }

        // Verify that all referenced elements still exist
        foreach (var element in referencedElements)
        {
            var found = await ElementEditingService.GetAsync(element.Key);
            Assert.That(found, Is.Not.Null, $"Referenced element {element.Key} should NOT have been deleted");
        }

        // The container should also still exist (because it contains referenced items)
        // or alternatively all referenced items should still be in the recycle bin
        var remainingInRecycleBin = EntityService.GetDescendants(Constants.System.RecycleBinElement).Count();
        Assert.That(
            remainingInRecycleBin, Is.EqualTo(referencedElements.Count + 1),
            $"Should have {referencedElements.Count} referenced elements plus the container remaining in recycle bin");
    }

    private void RelateElements(IElement parent, IElement child)
    {
        var relatedContentRelType = RelationService.GetRelationTypeByAlias(Constants.Conventions.RelationTypes.RelatedDocumentAlias);

        var relation = RelationService.Relate(parent.Id, child.Id, relatedContentRelType);
        RelationService.Save(relation);
    }
}
