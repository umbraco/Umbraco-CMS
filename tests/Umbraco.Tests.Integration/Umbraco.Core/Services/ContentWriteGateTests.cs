using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true)]
internal sealed class ContentWriteGateTests : UmbracoIntegrationTestWithContent
{
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(30);

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private ICoreScopeProvider CoreScopeProvider => GetRequiredService<ICoreScopeProvider>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder.AddNotificationHandler<ContentSavingNotification, SaveObserver>();

        ServiceDescriptor resolver = builder.Services.First(descriptor => descriptor.ServiceType == typeof(IUserIdKeyResolver));
        builder.Services.AddUnique<IUserIdKeyResolver>(provider =>
            new HoldableUserIdKeyResolver((IUserIdKeyResolver)ActivatorUtilities.CreateInstance(provider, resolver.ImplementationType!)));
    }

    [TearDown]
    public void ResetHooks()
    {
        SaveObserver.Saving = null;
        HoldableUserIdKeyResolver.BeforeResolve = null;
    }

    /// <summary>
    /// The saving notification is the first thing the content service publishes for a save, right after it takes the
    /// content tree lock. Holding the first save there, a second request must not reach the content service until the
    /// first has left it.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task Admits_One_Save_Into_The_Content_Service_At_A_Time()
    {
        var firstEntered = new TaskCompletionSource();
        var secondEntered = new TaskCompletionSource();
        using var releaseFirst = new ManualResetEventSlim(false);
        var entered = 0;
        SaveObserver.Saving = _ =>
        {
            if (Interlocked.Increment(ref entered) == 1)
            {
                firstEntered.SetResult();
                releaseFirst.Wait(Patience);
            }
            else
            {
                secondEntered.TrySetResult();
            }
        };

        Task<Attempt<ContentUpdateResult, ContentEditingOperationStatus>> first =
            Task.Run(() => ContentEditingService.UpdateAsync(Textpage.Key, Rename("first"), Constants.Security.SuperUserKey));
        Task<Attempt<ContentUpdateResult, ContentEditingOperationStatus>>? second = null;
        try
        {
            await firstEntered.Task.WaitAsync(Patience);

            second = Task.Run(() => ContentEditingService.UpdateAsync(Subpage.Key, Rename("second"), Constants.Security.SuperUserKey));
            await Task.WhenAny(secondEntered.Task, Task.Delay(TimeSpan.FromSeconds(2)));

            Assert.That(secondEntered.Task.IsCompleted, Is.False, "The second save reached the content service while the first was still inside it.");
        }
        finally
        {
            releaseFirst.Set();
        }

        Assert.Multiple(async () =>
        {
            Assert.That((await first.WaitAsync(Patience)).Success, Is.True);
            Assert.That((await second.WaitAsync(Patience)).Success, Is.True);
        });
        await secondEntered.Task.WaitAsync(Patience);
    }

    /// <summary>
    /// A caller that groups several writes in one scope holds the content tree write lock from its first write until
    /// the scope completes. Its next write must not wait for a request that has been admitted into the content write
    /// path and is blocked on that lock, or neither can finish until the request's lock wait fails.
    /// </summary>
    /// <remarks>
    /// The request is held after its reads and right before the write path, so that it is admitted once the batch's
    /// first write has left and then blocks on the lock the batch keeps.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Test]
    public async Task A_Write_In_A_Scope_Holding_The_Content_Tree_Lock_Does_Not_Wait_For_A_Request_Blocked_On_That_Lock()
    {
        var requestBeforeWritePath = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseRequest = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        HoldableUserIdKeyResolver.BeforeResolve = () =>
        {
            HoldableUserIdKeyResolver.BeforeResolve = null;
            requestBeforeWritePath.SetResult();
            return releaseRequest.Task;
        };

        Task<Attempt<ContentUpdateResult, ContentEditingOperationStatus>> request =
            Task.Run(() => ContentEditingService.UpdateAsync(Textpage.Key, Rename("request"), Constants.Security.SuperUserKey));
        await requestBeforeWritePath.Task.WaitAsync(Patience);

        var firstWriteDone = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var requestAdmitted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<(bool FirstSucceeded, bool SecondSucceeded, TimeSpan SecondTook)> batch = Task.Run(async () =>
        {
            using ICoreScope scope = CoreScopeProvider.CreateCoreScope();

            Attempt<ContentUpdateResult, ContentEditingOperationStatus> first =
                await ContentEditingService.UpdateAsync(Subpage.Key, Rename("batch first"), Constants.Security.SuperUserKey);
            firstWriteDone.SetResult();
            await requestAdmitted.Task.WaitAsync(Patience);

            var stopwatch = Stopwatch.StartNew();
            Attempt<ContentUpdateResult, ContentEditingOperationStatus> second =
                await ContentEditingService.UpdateAsync(Subpage2.Key, Rename("batch second"), Constants.Security.SuperUserKey);
            stopwatch.Stop();

            scope.Complete();
            return (first.Success, second.Success, stopwatch.Elapsed);
        });
        await firstWriteDone.Task.WaitAsync(Patience);

        // The batch keeps the lock. The request now enters the write path and blocks on that lock.
        releaseRequest.SetResult();
        await Task.Delay(500);
        requestAdmitted.SetResult();

        Task finishedFirst = await Task.WhenAny(batch, Task.Delay(Patience));
        Assert.That(finishedFirst, Is.SameAs(batch), "The batch's second write is waiting for the request, which is blocked on the batch's own lock.");
        (bool firstSucceeded, bool secondSucceeded, TimeSpan secondTook) = await batch;
        Attempt<ContentUpdateResult, ContentEditingOperationStatus> requestResult = await request.WaitAsync(Patience);
        TestContext.Out.WriteLine($"Batch second write took {secondTook.TotalSeconds:F1} s; request status {requestResult.Status}.");

        Assert.Multiple(() =>
        {
            Assert.That(firstSucceeded, Is.True);
            Assert.That(secondSucceeded, Is.True);
            Assert.That(secondTook, Is.LessThan(TimeSpan.FromSeconds(2)), "The batch's second write waited for a request that was blocked on the batch's own lock.");
            Assert.That(requestResult.Success, Is.True, "The request should wait for the batch to commit and then succeed.");
        });
    }

    private static ContentUpdateModel Rename(string name)
        => new() { Variants = [new() { Name = name }] };

    internal sealed class SaveObserver : INotificationHandler<ContentSavingNotification>
    {
        public static Action<ContentSavingNotification>? Saving { get; set; }

        public void Handle(ContentSavingNotification notification) => Saving?.Invoke(notification);
    }

    /// <summary>
    /// Lets a test hold a save after its reads and right before the content write path, where the editing service
    /// resolves the user id.
    /// </summary>
    internal sealed class HoldableUserIdKeyResolver(IUserIdKeyResolver inner) : IUserIdKeyResolver
    {
        public static Func<Task>? BeforeResolve { get; set; }

        public async Task<int> GetAsync(Guid key)
        {
            if (BeforeResolve is { } hold)
            {
                await hold();
            }

            return await inner.GetAsync(key);
        }

        public Task<Attempt<int>> TryGetAsync(Guid key) => inner.TryGetAsync(key);

        public Task<Guid> GetAsync(int id) => inner.GetAsync(id);

        public Task<Attempt<Guid>> TryGetAsync(int id) => inner.TryGetAsync(id);
    }
}
