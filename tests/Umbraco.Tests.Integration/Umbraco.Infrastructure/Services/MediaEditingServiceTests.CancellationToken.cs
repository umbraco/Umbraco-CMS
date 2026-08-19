using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

internal sealed partial class MediaEditingServiceTests
{
    [Test]
    public async Task Move_With_Already_Cancelled_Token_Writes_Nothing()
    {
        var root1 = await CreateFolderMediaAsync("Root 1");
        var child = await CreateFolderMediaAsync("Child", parentKey: root1.Key);
        var root2 = await CreateFolderMediaAsync("Root 2");

        using var cts = new System.Threading.CancellationTokenSource();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () =>
            await MediaEditingService.MoveAsync(child.Key, root2.Key, Constants.Security.SuperUserKey, cts.Token));

        var refetchedChild = await MediaEditingService.GetAsync(child.Key);
        Assert.IsNotNull(refetchedChild);
        Assert.AreEqual(root1.Id, refetchedChild!.ParentId, "An already-cancelled token must fail before any write, so the media must not have moved.");
    }
}
