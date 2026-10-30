using NUnit.Framework;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class ContentWriteGateTests
{
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(10);

    [Test]
    public async Task Second_Caller_Waits_Until_The_First_Has_Left()
    {
        var gate = new ContentWriteGate();
        var firstEntered = new TaskCompletionSource();
        var releaseFirst = new TaskCompletionSource();

        Task<int> first = gate.RunAsync(async () =>
        {
            firstEntered.SetResult();
            await releaseFirst.Task;
            return 1;
        });
        await firstEntered.Task.WaitAsync(Patience);

        Task<int> second = gate.RunAsync(() => 2);

        Assert.That(second.IsCompleted, Is.False, "The second caller must wait while the first is admitted.");

        releaseFirst.SetResult();

        Assert.That(await first.WaitAsync(Patience), Is.EqualTo(1));
        Assert.That(await second.WaitAsync(Patience), Is.EqualTo(2));
    }

    [Test]
    public async Task Work_Started_Inside_An_Admitted_Operation_Is_Admitted_At_Once()
    {
        var gate = new ContentWriteGate();

        var result = await gate.RunAsync(async () => await gate.RunAsync(() => 1).WaitAsync(Patience));

        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public async Task The_Admission_Does_Not_Outlive_The_Operation()
    {
        var gate = new ContentWriteGate();
        await gate.RunAsync(() => 0);

        var holderEntered = new TaskCompletionSource();
        var releaseHolder = new TaskCompletionSource();
        Task<int> holder = gate.RunAsync(async () =>
        {
            holderEntered.SetResult();
            await releaseHolder.Task;
            return 1;
        });
        await holderEntered.Task.WaitAsync(Patience);

        Task<int> next = gate.RunAsync(() => 2);

        Assert.That(next.IsCompleted, Is.False, "A completed operation must not leave its caller admitted.");

        releaseHolder.SetResult();
        await holder.WaitAsync(Patience);
        await next.WaitAsync(Patience);
    }

    [Test]
    public async Task A_Failing_Operation_Releases_The_Admission()
    {
        var gate = new ContentWriteGate();
        Func<int> failing = () => throw new InvalidOperationException();

        Assert.ThrowsAsync<InvalidOperationException>(() => gate.RunAsync(failing));

        Assert.That(await gate.RunAsync(() => 1).WaitAsync(Patience), Is.EqualTo(1));
    }
}
