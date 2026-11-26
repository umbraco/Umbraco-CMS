// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Attributes;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Verifies that every element and element folder deleted by an operation shares the state of the deleting
///     notification for the item chosen for deletion: the element itself, or the folder it was deleted with.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ElementServiceNotificationStateTests : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IElementService ElementService => GetRequiredService<IElementService>();

    private IElementEditingService ElementEditingService => GetRequiredService<IElementEditingService>();

    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private NotificationStateRecorder Recorder => GetRequiredService<NotificationStateRecorder>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<NotificationStateRecorder>();
        builder
            .AddNotificationHandler<ElementDeletingNotification, NotificationStateRecorder.WritingHandler<ElementDeletingNotification>>()
            .AddNotificationHandler<ElementDeletedNotification, NotificationStateRecorder.ReadingHandler<ElementDeletedNotification>>()
            .AddNotificationHandler<EntityContainerDeletingNotification, NotificationStateRecorder.WritingHandler<EntityContainerDeletingNotification>>()
            .AddNotificationHandler<EntityContainerDeletedNotification, NotificationStateRecorder.ReadingHandler<EntityContainerDeletedNotification>>();
    }

    public static void ConfigureCancelContainerDeleting(IUmbracoBuilder builder)
        => builder.AddNotificationHandler<EntityContainerDeletingNotification, CancelContainerDeletingHandler>();

    [Test]
    public async Task Delete_Shares_Deleting_State_With_Deleted_Notification()
    {
        IContentType elementType = await CreateElementType();
        IElement element = await CreateElement(elementType.Key);

        Attempt<IElement?, ContentEditingOperationStatus> result = await ElementEditingService.DeleteAsync(element.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        ElementDeletingNotification deleting = Recorder.Starting.OfType<ElementDeletingNotification>().Single();
        AssertDeletedStates(new Dictionary<Guid, IDictionary<string, object?>> { [element.Key] = deleting.State });
    }

    [Test]
    public async Task DeleteOfTypes_Shares_Deleting_State_With_Deleted_Notifications()
    {
        IContentType elementType = await CreateElementType();
        IElement first = await CreateElement(elementType.Key);
        IElement second = await CreateElement(elementType.Key);

        ElementService.DeleteOfTypes([elementType.Id]);

        ElementDeletingNotification deleting = Recorder.Starting.OfType<ElementDeletingNotification>().Single();
        AssertDeletedStates(new Dictionary<Guid, IDictionary<string, object?>>
        {
            [first.Key] = deleting.State,
            [second.Key] = deleting.State,
        });
    }

    [Test]
    public async Task DeleteFromRecycleBin_Folder_Shares_Folder_Deleting_State_With_Everything_Deleted_With_It()
    {
        TrashedStructure trashed = await CreateTrashedStructure();

        Attempt<EntityContainer?, EntityContainerOperationStatus> result = await ElementContainerService.DeleteFromRecycleBinAsync(trashed.Folder, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        EntityContainerDeletingNotification folderDeleting = Recorder.Starting.OfType<EntityContainerDeletingNotification>().Single();
        Assert.That(Recorder.Starting.OfType<ElementDeletingNotification>(), Is.Empty);
        AssertDeletedStates(new Dictionary<Guid, IDictionary<string, object?>>
        {
            [trashed.Folder] = folderDeleting.State,
            [trashed.FolderElement] = folderDeleting.State,
            [trashed.Subfolder] = folderDeleting.State,
            [trashed.SubfolderElement] = folderDeleting.State,
        });
    }

    [Test]
    public async Task EmptyRecycleBin_Shares_Deleting_State_Of_The_Root_Item_Each_Item_Was_Deleted_With()
    {
        TrashedStructure trashed = await CreateTrashedStructure();

        Attempt<EntityContainerOperationStatus> result = await ElementContainerService.EmptyRecycleBinAsync(Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        ElementDeletingNotification elementDeleting = Recorder.Starting.OfType<ElementDeletingNotification>().Single();
        EntityContainerDeletingNotification folderDeleting = Recorder.Starting.OfType<EntityContainerDeletingNotification>().Single();

        Assert.Multiple(() =>
        {
            Assert.That(elementDeleting.DeletedEntities.Select(x => x.Key), Is.EquivalentTo(new[] { trashed.LooseElement }));
            Assert.That(folderDeleting.DeletedEntities.Select(x => x.Key), Is.EquivalentTo(new[] { trashed.Folder }));
        });
        AssertDeletedStates(new Dictionary<Guid, IDictionary<string, object?>>
        {
            [trashed.LooseElement] = elementDeleting.State,
            [trashed.Folder] = folderDeleting.State,
            [trashed.FolderElement] = folderDeleting.State,
            [trashed.Subfolder] = folderDeleting.State,
            [trashed.SubfolderElement] = folderDeleting.State,
        });
    }

    [Test]
    public async Task EmptyRecycleBin_Publishes_Deleting_Notification_For_Every_Root_Folder_Across_Pages()
    {
        IContentType elementType = await CreateElementType();
        IElement looseElement = await CreateElement(elementType.Key);
        Assert.That((await ElementEditingService.MoveToRecycleBinAsync(looseElement.Key, Constants.Security.SuperUserKey)).Success, Is.True);
        (Guid firstFolder, Guid firstFolderElement) = await CreateTrashedFolder(elementType.Key);
        (Guid secondFolder, Guid secondFolderElement) = await CreateTrashedFolder(elementType.Key);

        Attempt<EntityContainerOperationStatus> result = await ((ElementContainerService)ElementContainerService)
            .EmptyRecycleBinAsync(Constants.Security.SuperUserKey, 1);
        Assert.That(result.Success, Is.True);

        ElementDeletingNotification elementDeleting = Recorder.Starting.OfType<ElementDeletingNotification>().Single();
        EntityContainerDeletingNotification[] folderDeletings = Recorder.Starting.OfType<EntityContainerDeletingNotification>().ToArray();
        Assert.That(folderDeletings.SelectMany(x => x.DeletedEntities).Select(x => x.Key), Is.EquivalentTo(new[] { firstFolder, secondFolder }));

        IDictionary<string, object?> FolderState(Guid folderKey)
            => folderDeletings.Single(x => x.DeletedEntities.Any(e => e.Key == folderKey)).State;

        AssertDeletedStates(new Dictionary<Guid, IDictionary<string, object?>>
        {
            [looseElement.Key] = elementDeleting.State,
            [firstFolder] = FolderState(firstFolder),
            [firstFolderElement] = FolderState(firstFolder),
            [secondFolder] = FolderState(secondFolder),
            [secondFolderElement] = FolderState(secondFolder),
        });
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureCancelContainerDeleting))]
    public async Task EmptyRecycleBin_Is_Cancelled_By_Root_Folder_Deleting_Notification()
    {
        TrashedStructure trashed = await CreateTrashedStructure();

        Attempt<EntityContainerOperationStatus> result = await ElementContainerService.EmptyRecycleBinAsync(Constants.Security.SuperUserKey);

        Assert.Multiple(async () =>
        {
            Assert.That(result.Result, Is.EqualTo(EntityContainerOperationStatus.CancelledByNotification));
            Assert.That(Recorder.Ending, Is.Empty);
            Assert.That(await ElementEditingService.GetAsync(trashed.LooseElement), Is.Not.Null);
            Assert.That(await ElementEditingService.GetAsync(trashed.SubfolderElement), Is.Not.Null);
            Assert.That(await ElementContainerService.GetAsync(trashed.Folder), Is.Not.Null);
        });
    }

    private void AssertDeletedStates(IDictionary<Guid, IDictionary<string, object?>> expectedStateByKey)
    {
        var actualStateByKey = new Dictionary<Guid, IDictionary<string, object?>>();
        foreach (ElementDeletedNotification notification in Recorder.Ending.OfType<ElementDeletedNotification>())
        {
            foreach (IElement element in notification.DeletedEntities)
            {
                actualStateByKey.Add(element.Key, notification.State);
            }
        }

        foreach (EntityContainerDeletedNotification notification in Recorder.Ending.OfType<EntityContainerDeletedNotification>())
        {
            foreach (EntityContainer container in notification.DeletedEntities)
            {
                actualStateByKey.Add(container.Key, notification.State);
            }
        }

        Assert.That(actualStateByKey.Keys, Is.EquivalentTo(expectedStateByKey.Keys));
        Assert.Multiple(() =>
        {
            foreach ((Guid key, IDictionary<string, object?> expectedState) in expectedStateByKey)
            {
                Assert.That(actualStateByKey[key], Is.SameAs(expectedState), $"State for {key}");
                Assert.That(expectedState.ContainsKey(NotificationStateRecorder.StateKey), Is.True, $"Deleting state for {key}");
            }
        });
    }

    /// <summary>
    ///     Creates, in the recycle bin, a loose element and a folder holding an element and a subfolder with an element.
    /// </summary>
    private async Task<TrashedStructure> CreateTrashedStructure()
    {
        IContentType elementType = await CreateElementType();

        IElement looseElement = await CreateElement(elementType.Key);

        var folderKey = Guid.NewGuid();
        Assert.That((await ElementContainerService.CreateAsync(folderKey, "Folder", null, Constants.Security.SuperUserKey)).Success, Is.True);
        IElement folderElement = await CreateElement(elementType.Key, folderKey);

        var subfolderKey = Guid.NewGuid();
        Assert.That((await ElementContainerService.CreateAsync(subfolderKey, "Subfolder", folderKey, Constants.Security.SuperUserKey)).Success, Is.True);
        IElement subfolderElement = await CreateElement(elementType.Key, subfolderKey);

        Assert.That((await ElementEditingService.MoveToRecycleBinAsync(looseElement.Key, Constants.Security.SuperUserKey)).Success, Is.True);
        Assert.That((await ElementContainerService.MoveToRecycleBinAsync(folderKey, Constants.Security.SuperUserKey)).Success, Is.True);

        return new TrashedStructure(looseElement.Key, folderKey, folderElement.Key, subfolderKey, subfolderElement.Key);
    }

    private async Task<(Guid Folder, Guid Element)> CreateTrashedFolder(Guid elementTypeKey)
    {
        var folderKey = Guid.NewGuid();
        Assert.That((await ElementContainerService.CreateAsync(folderKey, folderKey.ToString("N"), null, Constants.Security.SuperUserKey)).Success, Is.True);
        IElement element = await CreateElement(elementTypeKey, folderKey);
        Assert.That((await ElementContainerService.MoveToRecycleBinAsync(folderKey, Constants.Security.SuperUserKey)).Success, Is.True);

        return (folderKey, element.Key);
    }

    private async Task<IContentType> CreateElementType()
    {
        IContentType elementType = new ContentTypeBuilder()
            .WithAlias("stateElement")
            .WithName("State Element")
            .WithAllowAsRoot(true)
            .WithIsElement(true)
            .WithAllowedInLibrary(true)
            .Build();

        Assert.That((await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey)).Success, Is.True);
        return elementType;
    }

    private async Task<IElement> CreateElement(Guid contentTypeKey, Guid? parentKey = null)
    {
        var createModel = new ElementCreateModel
        {
            ContentTypeKey = contentTypeKey,
            ParentKey = parentKey,
            Variants = [new VariantModel { Name = Guid.NewGuid().ToString("N") }],
        };

        Attempt<ElementCreateResult, ContentEditingOperationStatus> result = await ElementEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        return result.Result.Content!;
    }

    private sealed record TrashedStructure(Guid LooseElement, Guid Folder, Guid FolderElement, Guid Subfolder, Guid SubfolderElement);

    private sealed class CancelContainerDeletingHandler : INotificationHandler<EntityContainerDeletingNotification>
    {
        public void Handle(EntityContainerDeletingNotification notification) => notification.Cancel = true;
    }
}
