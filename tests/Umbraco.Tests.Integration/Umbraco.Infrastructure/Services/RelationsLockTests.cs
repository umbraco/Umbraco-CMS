using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Relations;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
/// Relations written after the triggering operation has committed are written in a transaction of their own, which
/// must take the relations write lock before reading. These tests remove the lock's row, so that taking the lock fails,
/// to verify that each of those writers acquires it.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class RelationsLockTests : UmbracoIntegrationTestWithContent
{
    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder
            .AddNotificationHandler<ContentSavedNotification, ContentRelationsUpdate>()
            .AddNotificationAsyncHandler<ContentMovedToRecycleBinNotification, RelateOnTrashNotificationHandler>()
            .AddNotificationAsyncHandler<MediaMovedToRecycleBinNotification, RelateOnTrashNotificationHandler>();
    }

    [Test]
    public void Persisting_Automatic_Relations_Takes_The_Relations_Lock()
    {
        RemoveRelationsLock();

        AssertFailsToTakeTheLock(() => ContentService.Save(Subpage));
    }

    [Test]
    public void Relating_Trashed_Content_To_Its_Original_Parent_Takes_The_Relations_Lock()
    {
        RemoveRelationsLock();

        AssertFailsToTakeTheLock(() => ContentService.MoveToRecycleBin(Subpage));
    }

    [Test]
    public async Task Relating_Trashed_Media_To_Its_Original_Parent_Takes_The_Relations_Lock()
    {
        var mediaType = MediaTypeBuilder.CreateSimpleMediaType("testMediaType", "Test Media Type");
        await MediaTypeService.CreateAsync(mediaType, Constants.Security.SuperUserKey);
        IMedia media = MediaBuilder.CreateSimpleMedia(mediaType, "media", Constants.System.Root);
        MediaService.Save(media);
        RemoveRelationsLock();

        AssertFailsToTakeTheLock(() => MediaService.MoveToRecycleBin(media));
    }

    private void RemoveRelationsLock()
    {
        using IScope scope = ScopeProvider.CreateScope();
        scope.Database.Delete<LockDto>(Constants.Locks.Relations);
        scope.Complete();
    }

    private static void AssertFailsToTakeTheLock(TestDelegate operation)
    {
        // Taking a lock whose row is missing throws an ArgumentException, which may reach the caller wrapped, for
        // example in an AggregateException when raised by an asynchronous notification handler.
        Exception? exception = Assert.Catch(operation);
        Assert.That(ExceptionChain(exception), Has.Some.InstanceOf<ArgumentException>());
    }

    private static IEnumerable<Exception> ExceptionChain(Exception? exception)
    {
        for (; exception is not null; exception = exception.InnerException)
        {
            yield return exception;
            if (exception is AggregateException aggregate)
            {
                foreach (Exception inner in aggregate.Flatten().InnerExceptions.SelectMany(ExceptionChain))
                {
                    yield return inner;
                }
            }
        }
    }
}
