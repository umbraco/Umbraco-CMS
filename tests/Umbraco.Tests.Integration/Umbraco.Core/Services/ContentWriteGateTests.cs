using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
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

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder.AddNotificationHandler<ContentSavingNotification, SaveObserver>();
    }

    [TearDown]
    public void ResetObserver() => SaveObserver.Saving = null;

    /// <summary>
    /// The saving notification is the first thing the content service publishes for a save, before it touches the
    /// database and before the content tree lock is taken. Holding the first save there, a second request must not
    /// reach the content service until the first has left it.
    /// </summary>
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

    private static ContentUpdateModel Rename(string name)
        => new() { Variants = [new() { Name = name }] };

    internal sealed class SaveObserver : INotificationHandler<ContentSavingNotification>
    {
        public static Action<ContentSavingNotification>? Saving { get; set; }

        public void Handle(ContentSavingNotification notification) => Saving?.Invoke(notification);
    }
}
