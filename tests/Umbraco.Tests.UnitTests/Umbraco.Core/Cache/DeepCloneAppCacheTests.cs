// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Tests.Common;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

[TestFixture]
public class DeepCloneAppCacheTests : RuntimeAppCacheTests
{
    public override void Setup()
    {
        base.Setup();
        _memberCache = new ObjectCacheAppCache();

        _provider = new DeepCloneAppCache(_memberCache);
    }

    private DeepCloneAppCache _provider;
    private ObjectCacheAppCache _memberCache;

    protected override int GetTotalItemCount => _memberCache.MemoryCache.Count;

    internal override IAppCache AppCache => _provider;

    internal override IAppPolicyCache AppPolicyCache => _provider;

    [Test]
    public void Clones_List()
    {
        var original = new DeepCloneableList<TestClone>(ListCloneBehavior.Always) { new(), new(), new() };

        var val = _provider.GetCacheItem("test", () => original);

        Assert.That(val, Has.Count.EqualTo(original.Count));
        foreach (var item in val)
        {
            Assert.That(item.IsClone, Is.True);
        }
    }

    [Test]
    public void Ensures_Cloned_And_Reset()
    {
        var original = new TestClass { Name = "hello" };
        Assert.That(original.IsDirty(), Is.True);

        var val = _provider.GetCacheItem("test", () => original);

        Assert.That(val.CloneId, Is.Not.EqualTo(original.CloneId));
        Assert.That(val.IsDirty(), Is.False);
    }

    [Test]
    public void DoesNotCacheExceptions()
    {
        string value;
        Assert.Throws<Exception>(() => { value = (string)_provider.Get("key", () => GetValue(1)); });
        Assert.Throws<Exception>(() => { value = (string)_provider.Get("key", () => GetValue(2)); });

        // does not throw
        value = (string)_provider.Get("key", () => GetValue(3));
        Assert.That(value, Is.EqualTo("succ3"));

        // cache
        value = (string)_provider.Get("key", () => GetValue(4));
        Assert.That(value, Is.EqualTo("succ3"));
    }

    private static string GetValue(int i)
    {
        Debug.Print("get" + i);
        if (i < 3)
        {
            throw new Exception("fail");
        }

        return "succ" + i;
    }

    private class TestClass : BeingDirtyBase, IDeepCloneable
    {
        private string _name;

        public TestClass() => CloneId = Guid.NewGuid();

        public string Name
        {
            get => _name;

            set => SetPropertyValueAndDetectChanges(value, ref _name, nameof(Name));
        }

        public Guid CloneId { get; set; }

        public object DeepClone()
        {
            var shallowClone = (TestClass)MemberwiseClone();
            DeepCloneHelper.DeepCloneRefProperties(this, shallowClone);
            shallowClone.CloneId = Guid.NewGuid();
            return shallowClone;
        }
    }
}
