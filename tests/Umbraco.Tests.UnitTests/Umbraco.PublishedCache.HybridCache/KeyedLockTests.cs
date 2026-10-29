using NUnit.Framework;
using Umbraco.Cms.Infrastructure.HybridCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

[TestFixture]
public class KeyedLockTests
{
    [Test]
    public void Enter_SameKey_WaitsUntilTheHolderLeaves()
    {
        var keyedLock = new KeyedLock<Guid>();
        Guid key = Guid.NewGuid();
        var secondEntered = new ManualResetEventSlim();

        IDisposable first = keyedLock.Enter(key);
        Task second = Task.Run(() =>
        {
            using (keyedLock.Enter(key))
            {
                secondEntered.Set();
            }
        });

        Assert.That(secondEntered.Wait(TimeSpan.FromMilliseconds(300)), Is.False, "The second thread must wait while the first holds the key.");
        first.Dispose();
        Assert.Multiple(() =>
        {
            Assert.That(secondEntered.Wait(TimeSpan.FromSeconds(5)), Is.True);
            Assert.That(second.Wait(TimeSpan.FromSeconds(5)), Is.True);
        });
    }

    [Test]
    public void Enter_DifferentKeys_DoNotWaitForEachOther()
    {
        var keyedLock = new KeyedLock<Guid>();

        using (keyedLock.Enter(Guid.NewGuid()))
        {
            Task other = Task.Run(() => keyedLock.Enter(Guid.NewGuid()).Dispose());
            Assert.That(other.Wait(TimeSpan.FromSeconds(5)), Is.True);
        }
    }

    [Test]
    public void Exit_OfTheLastUser_RemovesTheKey()
    {
        var keyedLock = new KeyedLock<Guid>();
        Guid key = Guid.NewGuid();
        var secondEntered = new ManualResetEventSlim();
        var release = new ManualResetEventSlim();

        IDisposable first = keyedLock.Enter(key);
        Task second = Task.Run(() =>
        {
            using (keyedLock.Enter(key))
            {
                secondEntered.Set();
                release.Wait();
            }
        });

        Assert.That(keyedLock.Count, Is.EqualTo(1), "A held key counts once, however many threads wait for it.");
        first.Dispose();
        Assert.That(secondEntered.Wait(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(keyedLock.Count, Is.EqualTo(1), "The key stays while the second thread holds it.");
        release.Set();
        Assert.That(second.Wait(TimeSpan.FromSeconds(5)), Is.True);
        Assert.That(keyedLock.Count, Is.EqualTo(0), "The key is removed once the last user leaves.");
    }
}
