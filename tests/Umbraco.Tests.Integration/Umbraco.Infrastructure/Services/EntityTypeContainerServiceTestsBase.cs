using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Tests covering the container (folder) operations that behave the same for every tree supporting them.
/// </summary>
/// <remarks>
///     Container operations are implemented once, on the shared container service base, so the tests are shared too.
///     A fixture per tree supplies only how one of its entities is created, and adds any tests specific to that tree.
///     The iteration over multiple pages of descendants is covered by the element container service tests, which are
///     the only ones that create enough descendants to need more than one page.
/// </remarks>
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal abstract class EntityTypeContainerServiceTestsBase<TTreeEntity> : UmbracoIntegrationTest
    where TTreeEntity : ITreeEntity
{
    /// <summary>
    ///     Gets the container service for the tree under test.
    /// </summary>
    protected abstract IEntityTypeContainerService<TTreeEntity> ContainerService { get; }

    private IEntityService EntityService => GetRequiredService<IEntityService>();

    /// <summary>
    ///     Creates a single entity of the tree under test, inside the given container.
    /// </summary>
    /// <param name="container">The container to create the entity in.</param>
    /// <returns>The key of the created entity.</returns>
    protected abstract Task<Guid> CreateContainedEntityAsync(EntityContainer container);

    [Test]
    public async Task Can_Move_Empty_Container_To_Another_Container()
    {
        EntityContainer source = await CreateContainerAsync("Source");
        EntityContainer target = await CreateContainerAsync("Target");

        Attempt<EntityContainerOperationStatus> result = await ContainerService.MoveAsync(source.Key, target.Key, Constants.Security.SuperUserKey);
        AssertMoveSucceeded(result);

        EntityContainer moved = await GetContainerAsync(source.Key);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(target.Id, moved.ParentId);
            Assert.AreEqual($"{target.Path},{moved.Id}", moved.Path);
            Assert.AreEqual(target.Level + 1, moved.Level);
        });
    }

    [Test]
    public async Task Can_Move_Empty_Container_To_Root()
    {
        EntityContainer parent = await CreateContainerAsync("Parent");
        EntityContainer child = await CreateContainerAsync("Child", parent.Key);

        Attempt<EntityContainerOperationStatus> result = await ContainerService.MoveAsync(child.Key, null, Constants.Security.SuperUserKey);
        AssertMoveSucceeded(result);

        EntityContainer moved = await GetContainerAsync(child.Key);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(Constants.System.Root, moved.ParentId);
            Assert.AreEqual($"{Constants.System.Root},{moved.Id}", moved.Path);
            Assert.AreEqual(1, moved.Level);
        });
    }

    [Test]
    public async Task Can_Move_Container_With_Descendant_Containers()
    {
        EntityContainer source = await CreateContainerAsync("Source");
        EntityContainer child = await CreateContainerAsync("Child", source.Key);
        EntityContainer grandchild = await CreateContainerAsync("Grandchild", child.Key);
        EntityContainer target = await CreateContainerAsync("Target");

        Attempt<EntityContainerOperationStatus> result = await ContainerService.MoveAsync(source.Key, target.Key, Constants.Security.SuperUserKey);
        AssertMoveSucceeded(result);

        EntityContainer movedSource = await GetContainerAsync(source.Key);
        EntityContainer movedChild = await GetContainerAsync(child.Key);
        EntityContainer movedGrandchild = await GetContainerAsync(grandchild.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual($"{target.Path},{movedSource.Id}", movedSource.Path);
            Assert.AreEqual($"{target.Path},{movedSource.Id},{movedChild.Id}", movedChild.Path);
            Assert.AreEqual($"{target.Path},{movedSource.Id},{movedChild.Id},{movedGrandchild.Id}", movedGrandchild.Path);
            Assert.AreEqual(target.Level + 1, movedSource.Level);
            Assert.AreEqual(target.Level + 2, movedChild.Level);
            Assert.AreEqual(target.Level + 3, movedGrandchild.Level);
            Assert.AreEqual(movedSource.Id, movedChild.ParentId);
            Assert.AreEqual(movedChild.Id, movedGrandchild.ParentId);
        });
    }

    [Test]
    public async Task Can_Move_Container_With_Contained_Entities()
    {
        EntityContainer source = await CreateContainerAsync("Source");
        EntityContainer child = await CreateContainerAsync("Child", source.Key);
        EntityContainer target = await CreateContainerAsync("Target");

        Guid entityInSourceKey = await CreateContainedEntityAsync(source);
        Guid entityInChildKey = await CreateContainedEntityAsync(child);

        Attempt<EntityContainerOperationStatus> result = await ContainerService.MoveAsync(source.Key, target.Key, Constants.Security.SuperUserKey);
        AssertMoveSucceeded(result);

        EntityContainer movedSource = await GetContainerAsync(source.Key);
        EntityContainer movedChild = await GetContainerAsync(child.Key);

        // Read the entities back from umbracoNode rather than through their service, so that a cached entity
        // cannot hide an unwritten path.
        IEntitySlim entityInSource = GetEntity(entityInSourceKey);
        IEntitySlim entityInChild = GetEntity(entityInChildKey);

        Assert.Multiple(() =>
        {
            Assert.AreEqual($"{movedSource.Path},{entityInSource.Id}", entityInSource.Path);
            Assert.AreEqual(movedSource.Level + 1, entityInSource.Level);
            Assert.AreEqual($"{movedChild.Path},{entityInChild.Id}", entityInChild.Path);
            Assert.AreEqual(movedChild.Level + 1, entityInChild.Level);
        });
    }

    [Test]
    public async Task Can_Move_Container_To_Its_Current_Parent()
    {
        EntityContainer parent = await CreateContainerAsync("Parent");
        EntityContainer child = await CreateContainerAsync("Child", parent.Key);

        Attempt<EntityContainerOperationStatus> result = await ContainerService.MoveAsync(child.Key, parent.Key, Constants.Security.SuperUserKey);
        AssertMoveSucceeded(result);

        EntityContainer moved = await GetContainerAsync(child.Key);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(parent.Id, moved.ParentId);
            Assert.AreEqual(child.Path, moved.Path);
        });
    }

    [Test]
    public async Task Cannot_Move_Container_To_Self()
    {
        EntityContainer container = await CreateContainerAsync("Container");

        Attempt<EntityContainerOperationStatus> result = await ContainerService.MoveAsync(container.Key, container.Key, Constants.Security.SuperUserKey);
        AssertMoveFailed(result, EntityContainerOperationStatus.InvalidParent);
    }

    [Test]
    public async Task Cannot_Move_Container_To_Child_Of_Self()
    {
        EntityContainer parent = await CreateContainerAsync("Parent");
        EntityContainer child = await CreateContainerAsync("Child", parent.Key);

        Attempt<EntityContainerOperationStatus> result = await ContainerService.MoveAsync(parent.Key, child.Key, Constants.Security.SuperUserKey);
        AssertMoveFailed(result, EntityContainerOperationStatus.InvalidParent);
    }

    [Test]
    public async Task Cannot_Move_Non_Existing_Container()
    {
        EntityContainer target = await CreateContainerAsync("Target");

        Attempt<EntityContainerOperationStatus> result = await ContainerService.MoveAsync(Guid.NewGuid(), target.Key, Constants.Security.SuperUserKey);
        AssertMoveFailed(result, EntityContainerOperationStatus.NotFound);
    }

    [Test]
    public async Task Cannot_Move_Container_To_Non_Existing_Parent()
    {
        EntityContainer container = await CreateContainerAsync("Container");

        Attempt<EntityContainerOperationStatus> result = await ContainerService.MoveAsync(container.Key, Guid.NewGuid(), Constants.Security.SuperUserKey);
        AssertMoveFailed(result, EntityContainerOperationStatus.ParentNotFound);
    }

    [Test]
    public async Task Cannot_Move_Container_To_Parent_With_Same_Named_Child()
    {
        EntityContainer source = await CreateContainerAsync("Shared Name");
        EntityContainer target = await CreateContainerAsync("Target");
        await CreateContainerAsync("Shared Name", target.Key);

        Attempt<EntityContainerOperationStatus> result = await ContainerService.MoveAsync(source.Key, target.Key, Constants.Security.SuperUserKey);
        AssertMoveFailed(result, EntityContainerOperationStatus.DuplicateName);

        // the failed move must not have been persisted
        EntityContainer notMoved = await GetContainerAsync(source.Key);
        Assert.AreEqual(Constants.System.Root, notMoved.ParentId);
    }

    private static void AssertMoveSucceeded(Attempt<EntityContainerOperationStatus> result)
        => Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(EntityContainerOperationStatus.Success, result.Result);
        });

    private static void AssertMoveFailed(Attempt<EntityContainerOperationStatus> result, EntityContainerOperationStatus expectedStatus)
        => Assert.Multiple(() =>
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(expectedStatus, result.Result);
        });

    /// <summary>
    ///     Creates a container with the given name, optionally inside another container.
    /// </summary>
    /// <param name="name">The name of the container.</param>
    /// <param name="parentKey">The key of the parent container, or null to create at the tree root.</param>
    /// <returns>The created container.</returns>
    protected async Task<EntityContainer> CreateContainerAsync(string name, Guid? parentKey = null)
    {
        Attempt<EntityContainer?, EntityContainerOperationStatus> result =
            await ContainerService.CreateAsync(null, name, parentKey, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success, $"Failed to create container '{name}': {result.Status}");
        return result.Result!;
    }

    private async Task<EntityContainer> GetContainerAsync(Guid key)
    {
        EntityContainer? container = await ContainerService.GetAsync(key);
        Assert.NotNull(container);
        return container;
    }

    private IEntitySlim GetEntity(Guid key)
    {
        IEntitySlim? entity = EntityService.Get(key);
        Assert.NotNull(entity);
        return entity;
    }
}
