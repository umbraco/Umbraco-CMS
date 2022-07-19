// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.PublishedCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.NuCache;

[TestFixture]
public class SnapDictionaryTests
{
    [Test]
    public void LiveGenUpdate()
    {
        var d = new SnapDictionary<int, string>();
        d.Test.CollectAuto = false;

        Assert.AreEqual(0, d.Test.GetValues(1).Length);

        // gen 1
        d.Set(1, "one");
        Assert.AreEqual(1, d.Test.GetValues(1).Length);
        d.Clear(1);
        Assert.AreEqual(0, d.Test.GetValues(1).Length); // gone

        Assert.AreEqual(1, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);
        Assert.AreEqual(0, d.Test.FloorGen);
    }

    [Test]
    public void OtherGenUpdate()
    {
        var d = new SnapDictionary<int, string>();
        d.Test.CollectAuto = false;

        Assert.AreEqual(0, d.Test.GetValues(1).Length);
        Assert.AreEqual(0, d.Test.LiveGen);
        Assert.IsFalse(d.Test.NextGen);

        // gen 1
        d.Set(1, "one");
        Assert.AreEqual(1, d.Test.GetValues(1).Length);
        Assert.AreEqual(1, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);

        var s = d.CreateSnapshot();
        Assert.AreEqual(1, s.Gen);
        Assert.AreEqual(1, d.Test.LiveGen);
        Assert.IsFalse(d.Test.NextGen);

        // gen 2
        d.Clear(1);
        Assert.AreEqual(2, d.Test.GetValues(1).Length); // there
        Assert.AreEqual(2, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);

        Assert.AreEqual(0, d.Test.FloorGen);

        GC.KeepAlive(s);
    }

    [Test]
    public void MissingReturnsNull()
    {
        var d = new SnapDictionary<int, string>();
        var s = d.CreateSnapshot();

        Assert.IsNull(s.Get(1));
    }

    [Test]
    public void DeletedReturnsNull()
    {
        var d = new SnapDictionary<int, string>();

        // gen 1
        d.Set(1, "one");

        var s1 = d.CreateSnapshot();
        Assert.AreEqual("one", s1.Get(1));

        // gen 2
        d.Clear(1);

        var s2 = d.CreateSnapshot();
        Assert.IsNull(s2.Get(1));

        Assert.AreEqual("one", s1.Get(1));
    }

    [Retry(5)] // TODO make this test non-flaky.
    [Test]
    public async Task CollectValues()
    {
        var d = new SnapDictionary<int, string>();
        d.Test.CollectAuto = false;

        // gen 1
        d.Set(1, "one");
        Assert.AreEqual(1, d.Test.GetValues(1).Length);
        d.Set(1, "one");
        Assert.AreEqual(1, d.Test.GetValues(1).Length);
        d.Set(1, "uno");
        Assert.AreEqual(1, d.Test.GetValues(1).Length);

        Assert.AreEqual(1, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);

        var s1 = d.CreateSnapshot();

        Assert.AreEqual(1, d.Test.LiveGen);
        Assert.IsFalse(d.Test.NextGen);

        // gen 2
        Assert.AreEqual(1, d.Test.GetValues(1).Length);
        d.Set(1, "one");
        Assert.AreEqual(2, d.Test.GetValues(1).Length);
        d.Set(1, "uno");
        Assert.AreEqual(2, d.Test.GetValues(1).Length);

        Assert.AreEqual(2, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);

        var s2 = d.CreateSnapshot();

        Assert.AreEqual(2, d.Test.LiveGen);
        Assert.IsFalse(d.Test.NextGen);

        // gen 3
        Assert.AreEqual(2, d.Test.GetValues(1).Length);
        d.Set(1, "one");
        Assert.AreEqual(3, d.Test.GetValues(1).Length);
        d.Set(1, "uno");
        Assert.AreEqual(3, d.Test.GetValues(1).Length);

        Assert.AreEqual(3, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);

        var tv = d.Test.GetValues(1);
        Assert.AreEqual(3, tv[0].Gen);
        Assert.AreEqual(2, tv[1].Gen);
        Assert.AreEqual(1, tv[2].Gen);

        Assert.AreEqual(0, d.Test.FloorGen);

        // nothing to collect
        await d.CollectAsync();
        GC.KeepAlive(s1);
        GC.KeepAlive(s2);
        Assert.AreEqual(0, d.Test.FloorGen);
        Assert.AreEqual(3, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);
        Assert.AreEqual(2, d.SnapCount);
        Assert.AreEqual(3, d.Test.GetValues(1).Length);

        // one snapshot to collect
        s1 = null;
        GC.Collect();
        GC.KeepAlive(s2);
        await d.CollectAsync();
        Assert.AreEqual(1, d.Test.FloorGen);
        Assert.AreEqual(3, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);
        Assert.AreEqual(1, d.SnapCount);
        Assert.AreEqual(2, d.Test.GetValues(1).Length);

        // another snapshot to collect
        s2 = null;
        GC.Collect();
        await d.CollectAsync();
        Assert.AreEqual(2, d.Test.FloorGen);
        Assert.AreEqual(3, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);
        Assert.AreEqual(0, d.SnapCount);
        Assert.AreEqual(1, d.Test.GetValues(1).Length);
    }

    [Test]
    public async Task ProperlyCollects()
    {
        var d = new SnapDictionary<int, string>();
        d.Test.CollectAuto = false;

        for (var i = 0; i < 32; i++)
        {
            d.Set(i, i.ToString());
            d.CreateSnapshot().Dispose();
        }

        Assert.AreEqual(32, d.GenCount);
        Assert.AreEqual(0, d.SnapCount); // because we've disposed them

        await d.CollectAsync();
        Assert.AreEqual(32, d.Test.LiveGen);
        Assert.IsFalse(d.Test.NextGen);
        Assert.AreEqual(0, d.GenCount);
        Assert.AreEqual(0, d.SnapCount);
        Assert.AreEqual(32, d.Count);

        for (var i = 0; i < 32; i++)
        {
            d.Set(i, null);
        }

        d.CreateSnapshot().Dispose();

        // because we haven't collected yet, but disposed nevertheless
        Assert.AreEqual(1, d.GenCount);
        Assert.AreEqual(0, d.SnapCount);
        Assert.AreEqual(32, d.Count);

        // once we collect, they are all gone
        // since noone is interested anymore
        await d.CollectAsync();
        Assert.AreEqual(0, d.GenCount);
        Assert.AreEqual(0, d.SnapCount);
        Assert.AreEqual(0, d.Count);
    }

    [Retry(5)] // TODO make this test non-flaky.
    [Test]
    public async Task CollectNulls()
    {
        var d = new SnapDictionary<int, string>();
        d.Test.CollectAuto = false;

        // gen 1
        d.Set(1, "one");
        Assert.AreEqual(1, d.Test.GetValues(1).Length);
        d.Set(1, "one");
        Assert.AreEqual(1, d.Test.GetValues(1).Length);
        d.Set(1, "uno");
        Assert.AreEqual(1, d.Test.GetValues(1).Length);

        Assert.AreEqual(1, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);

        var s1 = d.CreateSnapshot();

        Assert.AreEqual(1, d.Test.LiveGen);
        Assert.IsFalse(d.Test.NextGen);

        // gen 2
        Assert.AreEqual(1, d.Test.GetValues(1).Length);
        d.Set(1, "one");
        Assert.AreEqual(2, d.Test.GetValues(1).Length);
        d.Set(1, "uno");
        Assert.AreEqual(2, d.Test.GetValues(1).Length);

        Assert.AreEqual(2, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);

        var s2 = d.CreateSnapshot();

        Assert.AreEqual(2, d.Test.LiveGen);
        Assert.IsFalse(d.Test.NextGen);

        // gen 3
        Assert.AreEqual(2, d.Test.GetValues(1).Length);
        d.Set(1, "one");
        Assert.AreEqual(3, d.Test.GetValues(1).Length);
        d.Set(1, "uno");
        Assert.AreEqual(3, d.Test.GetValues(1).Length);
        d.Clear(1);
        Assert.AreEqual(3, d.Test.GetValues(1).Length);

        Assert.AreEqual(3, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);

        var tv = d.Test.GetValues(1);
        Assert.AreEqual(3, tv[0].Gen);
        Assert.AreEqual(2, tv[1].Gen);
        Assert.AreEqual(1, tv[2].Gen);

        Assert.AreEqual(0, d.Test.FloorGen);

        // nothing to collect
        await d.CollectAsync();
        GC.KeepAlive(s1);
        GC.KeepAlive(s2);
        Assert.AreEqual(0, d.Test.FloorGen);
        Assert.AreEqual(3, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);
        Assert.AreEqual(2, d.SnapCount);
        Assert.AreEqual(3, d.Test.GetValues(1).Length);

        // one snapshot to collect
        s1 = null;
        GC.Collect();
        GC.KeepAlive(s2);
        await d.CollectAsync();
        Assert.AreEqual(1, d.Test.FloorGen);
        Assert.AreEqual(3, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);
        Assert.AreEqual(1, d.SnapCount);
        Assert.AreEqual(2, d.Test.GetValues(1).Length);

        // another snapshot to collect
        s2 = null;
        GC.Collect();
        await d.CollectAsync();
        Assert.AreEqual(2, d.Test.FloorGen);
        Assert.AreEqual(3, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);
        Assert.AreEqual(0, d.SnapCount);

        // and everything is gone?
        // no, cannot collect the live gen because we'd need to lock
        Assert.AreEqual(1, d.Test.GetValues(1).Length);

        d.CreateSnapshot();
        GC.Collect();
        await d.CollectAsync();

        // poof, gone
        Assert.AreEqual(0, d.Test.GetValues(1).Length);
    }

    [Test]
    [Retry(5)] // TODO make this test non-flaky.
    public async Task EventuallyCollectNulls()
    {
        var d = new SnapDictionary<int, string>();
        d.Test.CollectAuto = false;

        Assert.AreEqual(0, d.Test.GetValues(1).Length);

        // gen 1
        d.Set(1, "one");
        Assert.AreEqual(1, d.Test.GetValues(1).Length);

        Assert.AreEqual(1, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);

        await d.CollectAsync();
        var tv = d.Test.GetValues(1);
        Assert.AreEqual(1, tv.Length);
        Assert.AreEqual(1, tv[0].Gen);

        var s = d.CreateSnapshot();
        Assert.AreEqual("one", s.Get(1));

        Assert.AreEqual(1, d.Test.LiveGen);
        Assert.IsFalse(d.Test.NextGen);

        Assert.AreEqual(1, d.Count);
        Assert.AreEqual(1, d.SnapCount);
        Assert.AreEqual(1, d.GenCount);

        // gen 2
        d.Clear(1);
        tv = d.Test.GetValues(1);
        Assert.AreEqual(2, tv.Length);
        Assert.AreEqual(2, tv[0].Gen);

        Assert.AreEqual(2, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);

        Assert.AreEqual(1, d.Count);
        Assert.AreEqual(1, d.SnapCount);
        Assert.AreEqual(1, d.GenCount);

        // nothing to collect
        await d.CollectAsync();
        GC.KeepAlive(s);
        Assert.AreEqual(2, d.Test.GetValues(1).Length);

        Assert.AreEqual(1, d.Count);
        Assert.AreEqual(1, d.SnapCount);
        Assert.AreEqual(1, d.GenCount);

        Assert.AreEqual(2, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);

        // collect snapshot
        // don't collect liveGen+
        s = null; // without being disposed
        GC.Collect(); // should release the generation reference
        await d.CollectAsync();

        Assert.AreEqual(1, d.Test.GetValues(1).Length); // "one" value is gone
        Assert.AreEqual(1, d.Count); // still have 1 item
        Assert.AreEqual(0, d.SnapCount); // snapshot is gone
        Assert.AreEqual(0, d.GenCount); // and generation has been dequeued

        // liveGen/nextGen
        s = d.CreateSnapshot();
        s = null;

        // collect liveGen
        GC.Collect();

        Assert.IsTrue(d.Test.GenObjs.TryPeek(out var genObj));
        genObj = null;

        // in Release mode, it works, but in Debug mode, the weak reference is still alive
        // and for some reason we need to do this to ensure it is collected
#if DEBUG
        await d.CollectAsync();
        GC.Collect();
#endif

        Assert.IsTrue(d.Test.GenObjs.TryPeek(out genObj));
        Assert.IsFalse(genObj.WeakGenRef.IsAlive); // snapshot is gone, along with its reference

        await d.CollectAsync();

        Assert.AreEqual(0, d.Test.GetValues(1).Length); // null value is gone
        Assert.AreEqual(0, d.Count); // item is gone
        Assert.AreEqual(0, d.Test.GenObjs.Count);
        Assert.AreEqual(0, d.SnapCount); // snapshot is gone
        Assert.AreEqual(0, d.GenCount); // and generation has been dequeued
    }

    [Test]
    public async Task CollectDisposedSnapshots()
    {
        var d = new SnapDictionary<int, string>();
        d.Test.CollectAuto = false;

        // gen 1
        d.Set(1, "one");
        Assert.AreEqual(1, d.Test.GetValues(1).Length);

        Assert.AreEqual(1, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);

        var s1 = d.CreateSnapshot();

        Assert.AreEqual(1, d.Test.LiveGen);
        Assert.IsFalse(d.Test.NextGen);

        // gen 2
        d.Set(1, "two");
        Assert.AreEqual(2, d.Test.GetValues(1).Length);

        Assert.AreEqual(2, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);

        var s2 = d.CreateSnapshot();

        Assert.AreEqual(2, d.Test.LiveGen);
        Assert.IsFalse(d.Test.NextGen);

        // gen 3
        d.Set(1, "three");
        Assert.AreEqual(3, d.Test.GetValues(1).Length);

        Assert.AreEqual(3, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);

        var s3 = d.CreateSnapshot();

        Assert.AreEqual(3, d.Test.LiveGen);
        Assert.IsFalse(d.Test.NextGen);

        Assert.AreEqual(3, d.SnapCount);

        s1.Dispose();
        await d.CollectAsync();
        Assert.AreEqual(2, d.SnapCount);
        Assert.AreEqual(2, d.Test.GetValues(1).Length);

        s2.Dispose();
        await d.CollectAsync();
        Assert.AreEqual(1, d.SnapCount);
        Assert.AreEqual(1, d.Test.GetValues(1).Length);

        s3.Dispose();
        await d.CollectAsync();
        Assert.AreEqual(0, d.SnapCount);
        Assert.AreEqual(1, d.Test.GetValues(1).Length);
    }

    [Retry(5)] // TODO make this test non-flaky.
    [Test]
    public async Task CollectGcSnapshots()
    {
        var d = new SnapDictionary<int, string>();
        d.Test.CollectAuto = false;

        // gen 1
        d.Set(1, "one");
        Assert.AreEqual(1, d.Test.GetValues(1).Length);

        Assert.AreEqual(1, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);

        var s1 = d.CreateSnapshot();

        Assert.AreEqual(1, d.Test.LiveGen);
        Assert.IsFalse(d.Test.NextGen);

        // gen 2
        d.Set(1, "two");
        Assert.AreEqual(2, d.Test.GetValues(1).Length);

        Assert.AreEqual(2, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);

        var s2 = d.CreateSnapshot();

        Assert.AreEqual(2, d.Test.LiveGen);
        Assert.IsFalse(d.Test.NextGen);

        // gen 3
        d.Set(1, "three");
        Assert.AreEqual(3, d.Test.GetValues(1).Length);

        Assert.AreEqual(3, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);

        var s3 = d.CreateSnapshot();

        Assert.AreEqual(3, d.Test.LiveGen);
        Assert.IsFalse(d.Test.NextGen);

        Assert.AreEqual(3, d.SnapCount);

        s1 = s2 = s3 = null;

        await d.CollectAsync();
        Assert.AreEqual(3, d.SnapCount);
        Assert.AreEqual(3, d.Test.GetValues(1).Length);

        GC.Collect();
        await d.CollectAsync();
        Assert.AreEqual(0, d.SnapCount);
        Assert.AreEqual(1, d.Test.GetValues(1).Length);
    }

    [Retry(5)] // TODO make this test non-flaky.
    [Test]
    public async Task RandomTest1()
    {
        var d = new SnapDictionary<int, string>();
        d.Test.CollectAuto = false;

        d.Set(1, "one");
        d.Set(2, "two");

        var s1 = d.CreateSnapshot();
        var v1 = s1.Get(1);
        Assert.AreEqual("one", v1);

        d.Set(1, "uno");

        var s2 = d.CreateSnapshot();
        var v2 = s2.Get(1);
        Assert.AreEqual("uno", v2);

        v1 = s1.Get(1);
        Assert.AreEqual("one", v1);

        Assert.AreEqual(2, d.SnapCount);

        s1 = null;
        GC.Collect();
        await d.CollectAsync();

        GC.Collect();
        await d.CollectAsync();

        Assert.AreEqual(1, d.SnapCount);
        v2 = s2.Get(1);
        Assert.AreEqual("uno", v2);

        s2 = null;
        GC.Collect();
        await d.CollectAsync();

        Assert.AreEqual(0, d.SnapCount);
    }

    [Retry(5)] // TODO make this test non-flaky.
    [Test]
    public async Task RandomTest2()
    {
        var d = new SnapDictionary<int, string>();
        d.Test.CollectAuto = false;

        d.Set(1, "one");
        d.Set(2, "two");

        var s1 = d.CreateSnapshot();
        var v1 = s1.Get(1);
        Assert.AreEqual("one", v1);

        d.Clear(1);

        var s2 = d.CreateSnapshot();
        var v2 = s2.Get(1);
        Assert.AreEqual(null, v2);

        v1 = s1.Get(1);
        Assert.AreEqual("one", v1);

        Assert.AreEqual(2, d.SnapCount);

        s1 = null;
        GC.Collect();
        await d.CollectAsync();

        GC.Collect();
        await d.CollectAsync();

        Assert.AreEqual(1, d.SnapCount);
        v2 = s2.Get(1);
        Assert.AreEqual(null, v2);

        s2 = null;
        GC.Collect();
        await d.CollectAsync();

        Assert.AreEqual(0, d.SnapCount);
    }

    [Test]
    public void WriteLockingFirstSnapshot()
    {
        var d = new SnapDictionary<int, string>();
        d.Test.CollectAuto = false;

        // gen 1
        d.Set(1, "one");
        Assert.AreEqual(1, d.Test.GetValues(1).Length);

        Assert.AreEqual(1, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);

        using (d.GetScopedWriteLock(GetScopeProvider()))
        {
            var s1 = d.CreateSnapshot();

            Assert.AreEqual(0, s1.Gen);
            Assert.AreEqual(1, d.Test.LiveGen);
            Assert.IsTrue(d.Test.NextGen);
            Assert.IsNull(s1.Get(1));
        }

        var s2 = d.CreateSnapshot();

        Assert.AreEqual(1, s2.Gen);
        Assert.AreEqual(1, d.Test.LiveGen);
        Assert.IsFalse(d.Test.NextGen);
        Assert.AreEqual("one", s2.Get(1));
    }

    [Test]
    public void WriteLocking()
    {
        var d = new SnapDictionary<int, string>();
        d.Test.CollectAuto = false;

        // gen 1
        d.Set(1, "one");
        Assert.AreEqual(1, d.Test.GetValues(1).Length);

        Assert.AreEqual(1, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);

        var s1 = d.CreateSnapshot();

        Assert.AreEqual(1, s1.Gen);
        Assert.AreEqual(1, d.Test.LiveGen);
        Assert.IsFalse(d.Test.NextGen);
        Assert.AreEqual("one", s1.Get(1));

        // gen 2
        Assert.AreEqual(1, d.Test.GetValues(1).Length);
        d.Set(1, "uno");
        Assert.AreEqual(2, d.Test.GetValues(1).Length);

        Assert.AreEqual(2, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);

        var s2 = d.CreateSnapshot();

        Assert.AreEqual(2, s2.Gen);
        Assert.AreEqual(2, d.Test.LiveGen);
        Assert.IsFalse(d.Test.NextGen);
        Assert.AreEqual("uno", s2.Get(1));

        using (d.GetScopedWriteLock(GetScopeProvider()))
        {
            // gen 3
            Assert.AreEqual(2, d.Test.GetValues(1).Length);
            d.SetLocked(1, "ein");
            Assert.AreEqual(3, d.Test.GetValues(1).Length);

            Assert.AreEqual(3, d.Test.LiveGen);
            Assert.IsTrue(d.Test.NextGen);

            var s3 = d.CreateSnapshot();

            Assert.AreEqual(2, s3.Gen);
            Assert.AreEqual(3, d.Test.LiveGen);
            Assert.IsTrue(d.Test.NextGen); // has NOT changed when (non) creating snapshot
            Assert.AreEqual("uno", s3.Get(1));
        }

        var s4 = d.CreateSnapshot();

        Assert.AreEqual(3, s4.Gen);
        Assert.AreEqual(3, d.Test.LiveGen);
        Assert.IsFalse(d.Test.NextGen);
        Assert.AreEqual("ein", s4.Get(1));
    }

    [Test]
    public void NestedWriteLocking1()
    {
        var d = new SnapDictionary<int, string>();
        var t = d.Test;
        t.CollectAuto = false;

        Assert.AreEqual(0, d.CreateSnapshot().Gen);

        // no scope context: writers nest, last one to be disposed commits
        var scopeProvider = GetScopeProvider();

        using (var w1 = d.GetScopedWriteLock(scopeProvider))
        {
            Assert.AreEqual(1, t.LiveGen);
            Assert.IsTrue(t.IsLocked);
            Assert.IsTrue(t.NextGen);

            Assert.Throws<InvalidOperationException>(() =>
            {
                using (var w2 = d.GetScopedWriteLock(scopeProvider))
                {
                }
            });

            Assert.AreEqual(1, t.LiveGen);
            Assert.IsTrue(t.IsLocked);
            Assert.IsTrue(t.NextGen);

            Assert.AreEqual(0, d.CreateSnapshot().Gen);
        }

        Assert.AreEqual(1, t.LiveGen);
        Assert.IsFalse(t.IsLocked);
        Assert.IsTrue(t.NextGen);

        Assert.AreEqual(1, d.CreateSnapshot().Gen);
    }

    [Test]
    public void NestedWriteLocking2()
    {
        var d = new SnapDictionary<int, string>();
        d.Test.CollectAuto = false;

        Assert.AreEqual(0, d.CreateSnapshot().Gen);

        // scope context: writers enlist
        var scopeContext = new ScopeContext();
        var scopeProvider = GetScopeProvider(scopeContext);

        using (var w1 = d.GetScopedWriteLock(scopeProvider))
        {
            // This one is interesting, although we don't allow recursive locks, since this is
            // using the same ScopeContext/key, the lock acquisition is only done once.
            using (var w2 = d.GetScopedWriteLock(scopeProvider))
            {
                Assert.AreSame(w1, w2);

                d.SetLocked(1, "one");
            }
        }
    }

    [Test]
    public void NestedWriteLocking3()
    {
        var d = new SnapDictionary<int, string>();
        var t = d.Test;
        t.CollectAuto = false;

        Assert.AreEqual(0, d.CreateSnapshot().Gen);

        var scopeContext = new ScopeContext();
        var scopeProvider1 = GetScopeProvider();
        var scopeProvider2 = GetScopeProvider(scopeContext);

        using (var w1 = d.GetScopedWriteLock(scopeProvider1))
        {
            Assert.AreEqual(1, t.LiveGen);
            Assert.IsTrue(t.IsLocked);
            Assert.IsTrue(t.NextGen);

            Assert.Throws<InvalidOperationException>(() =>
            {
                using (var w2 = d.GetScopedWriteLock(scopeProvider2))
                {
                }
            });
        }
    }

    [Test]
    public void WriteLocking2()
    {
        var d = new SnapDictionary<int, string>();
        d.Test.CollectAuto = false;

        // gen 1
        d.Set(1, "one");
        Assert.AreEqual(1, d.Test.GetValues(1).Length);

        Assert.AreEqual(1, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);

        var s1 = d.CreateSnapshot();

        Assert.AreEqual(1, s1.Gen);
        Assert.AreEqual(1, d.Test.LiveGen);
        Assert.IsFalse(d.Test.NextGen);
        Assert.AreEqual("one", s1.Get(1));

        // gen 2
        Assert.AreEqual(1, d.Test.GetValues(1).Length);
        d.Set(1, "uno");
        Assert.AreEqual(2, d.Test.GetValues(1).Length);

        Assert.AreEqual(2, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);

        var s2 = d.CreateSnapshot();

        Assert.AreEqual(2, s2.Gen);
        Assert.AreEqual(2, d.Test.LiveGen);
        Assert.IsFalse(d.Test.NextGen);
        Assert.AreEqual("uno", s2.Get(1));

        var scopeProvider = GetScopeProvider();
        using (d.GetScopedWriteLock(scopeProvider))
        {
            // gen 3
            Assert.AreEqual(2, d.Test.GetValues(1).Length);
            d.SetLocked(1, "ein");
            Assert.AreEqual(3, d.Test.GetValues(1).Length);

            Assert.AreEqual(3, d.Test.LiveGen);
            Assert.IsTrue(d.Test.NextGen);

            var s3 = d.CreateSnapshot();

            Assert.AreEqual(2, s3.Gen);
            Assert.AreEqual(3, d.Test.LiveGen);
            Assert.IsTrue(d.Test.NextGen); // has NOT changed when (non) creating snapshot
            Assert.AreEqual("uno", s3.Get(1));
        }

        var s4 = d.CreateSnapshot();

        Assert.AreEqual(3, s4.Gen);
        Assert.AreEqual(3, d.Test.LiveGen);
        Assert.IsFalse(d.Test.NextGen);
        Assert.AreEqual("ein", s4.Get(1));
    }

    [Test]
    public void WriteLocking3()
    {
        var d = new SnapDictionary<int, string>();
        d.Test.CollectAuto = false;

        // gen 1
        d.Set(1, "one");
        var s1 = d.CreateSnapshot();
        Assert.AreEqual(1, s1.Gen);
        Assert.AreEqual("one", s1.Get(1));

        d.Set(1, "uno");
        var s2 = d.CreateSnapshot();
        Assert.AreEqual(2, s2.Gen);
        Assert.AreEqual("uno", s2.Get(1));

        var scopeProvider = GetScopeProvider();
        using (d.GetScopedWriteLock(scopeProvider))
        {
            // creating a snapshot in a write-lock does NOT return the "current" content
            // it uses the previous snapshot, so new snapshot created only on release
            d.SetLocked(1, "ein");
            var s3 = d.CreateSnapshot();
            Assert.AreEqual(2, s3.Gen);
            Assert.AreEqual("uno", s3.Get(1));

            // but live snapshot contains changes
            var ls = d.Test.LiveSnapshot;
            Assert.AreEqual("ein", ls.Get(1));
            Assert.AreEqual(3, ls.Gen);
        }

        var s4 = d.CreateSnapshot();
        Assert.AreEqual(3, s4.Gen);
        Assert.AreEqual("ein", s4.Get(1));
    }

    [Test]
    public void ScopeLocking1()
    {
        var d = new SnapDictionary<int, string>();
        d.Test.CollectAuto = false;

        // gen 1
        d.Set(1, "one");
        var s1 = d.CreateSnapshot();
        Assert.AreEqual(1, s1.Gen);
        Assert.AreEqual("one", s1.Get(1));

        d.Set(1, "uno");
        var s2 = d.CreateSnapshot();
        Assert.AreEqual(2, s2.Gen);
        Assert.AreEqual("uno", s2.Get(1));

        var scopeContext = new ScopeContext();
        var scopeProvider = GetScopeProvider(scopeContext);
        using (d.GetScopedWriteLock(scopeProvider))
        {
            // creating a snapshot in a write-lock does NOT return the "current" content
            // it uses the previous snapshot, so new snapshot created only on release
            d.SetLocked(1, "ein");
            var s3 = d.CreateSnapshot();
            Assert.AreEqual(2, s3.Gen);
            Assert.AreEqual("uno", s3.Get(1));

            // but live snapshot contains changes
            var ls = d.Test.LiveSnapshot;
            Assert.AreEqual("ein", ls.Get(1));
            Assert.AreEqual(3, ls.Gen);
        }

        var s4 = d.CreateSnapshot();
        Assert.AreEqual(2, s4.Gen);
        Assert.AreEqual("uno", s4.Get(1));

        scopeContext.ScopeExit(true);

        var s5 = d.CreateSnapshot();
        Assert.AreEqual(3, s5.Gen);
        Assert.AreEqual("ein", s5.Get(1));
    }

    [Test]
    public void ScopeLocking2()
    {
        var d = new SnapDictionary<int, string>();
        var t = d.Test;
        t.CollectAuto = false;

        // gen 1
        d.Set(1, "one");
        var s1 = d.CreateSnapshot();
        Assert.AreEqual(1, s1.Gen);
        Assert.AreEqual("one", s1.Get(1));

        d.Set(1, "uno");
        var s2 = d.CreateSnapshot();
        Assert.AreEqual(2, s2.Gen);
        Assert.AreEqual("uno", s2.Get(1));

        Assert.AreEqual(2, t.LiveGen);
        Assert.IsFalse(t.NextGen);

        var scopeContext = new ScopeContext();
        var scopeProvider = GetScopeProvider(scopeContext);
        using (d.GetScopedWriteLock(scopeProvider))
        {
            // creating a snapshot in a write-lock does NOT return the "current" content
            // it uses the previous snapshot, so new snapshot created only on release
            d.SetLocked(1, "ein");
            var s3 = d.CreateSnapshot();
            Assert.AreEqual(2, s3.Gen);
            Assert.AreEqual("uno", s3.Get(1));

            // we made some changes, so a next gen is required
            Assert.AreEqual(3, t.LiveGen);
            Assert.IsTrue(t.NextGen);
            Assert.IsTrue(t.IsLocked);

            // but live snapshot contains changes
            var ls = t.LiveSnapshot;
            Assert.AreEqual("ein", ls.Get(1));
            Assert.AreEqual(3, ls.Gen);
        }

        // nothing is committed until scope exits
        Assert.AreEqual(3, t.LiveGen);
        Assert.IsTrue(t.NextGen);
        Assert.IsTrue(t.IsLocked);

        // no changes until exit
        var s4 = d.CreateSnapshot();
        Assert.AreEqual(2, s4.Gen);
        Assert.AreEqual("uno", s4.Get(1));

        scopeContext.ScopeExit(false);

        // now things have changed
        Assert.AreEqual(2, t.LiveGen);
        Assert.IsFalse(t.NextGen);
        Assert.IsFalse(t.IsLocked);

        // no changes since not completed
        var s5 = d.CreateSnapshot();
        Assert.AreEqual(2, s5.Gen);
        Assert.AreEqual("uno", s5.Get(1));
    }

    [Test]
    public void GetAll()
    {
        var d = new SnapDictionary<int, string>();
        d.Test.CollectAuto = false;

        Assert.AreEqual(0, d.Test.GetValues(1).Length);

        d.Set(1, "one");
        d.Set(2, "two");
        d.Set(3, "three");
        d.Set(4, "four");

        var s1 = d.CreateSnapshot();
        var all = s1.GetAll().ToArray();
        Assert.AreEqual(4, all.Length);
        Assert.AreEqual("one", all[0]);
        Assert.AreEqual("four", all[3]);

        d.Set(1, "uno");
        var s2 = d.CreateSnapshot();

        all = s1.GetAll().ToArray();
        Assert.AreEqual(4, all.Length);
        Assert.AreEqual("one", all[0]);
        Assert.AreEqual("four", all[3]);

        all = s2.GetAll().ToArray();
        Assert.AreEqual(4, all.Length);
        Assert.AreEqual("uno", all[0]);
        Assert.AreEqual("four", all[3]);
    }

    [Test]
    public void DontPanic()
    {
        var d = new SnapDictionary<int, string>();
        d.Test.CollectAuto = false;

        Assert.IsNull(d.Test.GenObj);

        // gen 1
        d.Set(1, "one");
        Assert.IsTrue(d.Test.NextGen);
        Assert.AreEqual(1, d.Test.LiveGen);
        Assert.IsNull(d.Test.GenObj);

        var s1 = d.CreateSnapshot();
        Assert.IsFalse(d.Test.NextGen);
        Assert.AreEqual(1, d.Test.LiveGen);
        Assert.IsNotNull(d.Test.GenObj);
        Assert.AreEqual(1, d.Test.GenObj.Gen);

        Assert.AreEqual(1, s1.Gen);
        Assert.AreEqual("one", s1.Get(1));

        d.Set(1, "uno");
        Assert.IsTrue(d.Test.NextGen);
        Assert.AreEqual(2, d.Test.LiveGen);
        Assert.IsNotNull(d.Test.GenObj);
        Assert.AreEqual(1, d.Test.GenObj.Gen);

        var scopeContext = new ScopeContext();
        var scopeProvider = GetScopeProvider(scopeContext);

        // scopeProvider.Context == scopeContext -> writer is scoped
        // writer is scope contextual and scoped
        //  when disposed, nothing happens
        //  when the context exists, the writer is released
        using (d.GetScopedWriteLock(scopeProvider))
        {
            d.SetLocked(1, "ein");
            Assert.IsTrue(d.Test.NextGen);
            Assert.AreEqual(3, d.Test.LiveGen);
            Assert.IsNotNull(d.Test.GenObj);
            Assert.AreEqual(2, d.Test.GenObj.Gen);
        }

        // writer has not released
        Assert.IsTrue(d.Test.IsLocked);
        Assert.IsNotNull(d.Test.GenObj);
        Assert.AreEqual(2, d.Test.GenObj.Gen);

        // nothing changed
        Assert.IsTrue(d.Test.NextGen);
        Assert.AreEqual(3, d.Test.LiveGen);

        // panic!
        var s2 = d.CreateSnapshot();

        Assert.IsTrue(d.Test.IsLocked);
        Assert.IsNotNull(d.Test.GenObj);
        Assert.AreEqual(2, d.Test.GenObj.Gen);
        Assert.AreEqual(3, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);

        // release writer
        scopeContext.ScopeExit(true);

        Assert.IsFalse(d.Test.IsLocked);
        Assert.IsNotNull(d.Test.GenObj);
        Assert.AreEqual(2, d.Test.GenObj.Gen);
        Assert.AreEqual(3, d.Test.LiveGen);
        Assert.IsTrue(d.Test.NextGen);

        var s3 = d.CreateSnapshot();

        Assert.IsFalse(d.Test.IsLocked);
        Assert.IsNotNull(d.Test.GenObj);
        Assert.AreEqual(3, d.Test.GenObj.Gen);
        Assert.AreEqual(3, d.Test.LiveGen);
        Assert.IsFalse(d.Test.NextGen);
    }

    private ICoreScopeProvider GetScopeProvider(ScopeContext scopeContext = null)
    {
        var scopeProvider = Mock.Of<ICoreScopeProvider>();
        Mock.Get(scopeProvider)
            .Setup(x => x.Context).Returns(scopeContext);
        return scopeProvider;
    }
}

/// <summary>
///     Used for tests so that we don't have to wrap every Set/Clear call in locks
/// </summary>
public static class SnapDictionaryExtensions
{
    internal static void Set<TKey, TValue>(this SnapDictionary<TKey, TValue> d, TKey key, TValue value)
        where TValue : class
    {
        using (d.GetScopedWriteLock(GetScopeProvider()))
        {
            d.SetLocked(key, value);
        }
    }

    internal static void Clear<TKey, TValue>(this SnapDictionary<TKey, TValue> d)
        where TValue : class
    {
        using (d.GetScopedWriteLock(GetScopeProvider()))
        {
            d.ClearLocked();
        }
    }

    internal static void Clear<TKey, TValue>(this SnapDictionary<TKey, TValue> d, TKey key)
        where TValue : class
    {
        using (d.GetScopedWriteLock(GetScopeProvider()))
        {
            d.ClearLocked(key);
        }
    }

    private static ICoreScopeProvider GetScopeProvider()
    {
        var scopeProvider = Mock.Of<ICoreScopeProvider>();
        Mock.Get(scopeProvider)
            .Setup(x => x.Context).Returns(() => null);
        return scopeProvider;
    }
}
