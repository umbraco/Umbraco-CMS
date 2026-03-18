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

/// <summary>
/// Contains unit tests for verifying the behavior of the DeepCloneAppCache class and its deep cloning functionality.
/// </summary>
[TestFixture]
public class DeepCloneAppCacheTests : RuntimeAppCacheTests
{
    /// <summary>
    /// Sets up the test environment for <see cref="DeepCloneAppCacheTests"/>,
    /// initializing the member cache and the deep clone app cache provider.
    /// </summary>
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

    /// <summary>
    /// Verifies that when a list of DeepCloneable objects is cached, the cache provider returns a deep-cloned list
    /// where each item is a clone of the original.
    /// </summary>
    [Test]
    public void Clones_List()
    {
        var original = new DeepCloneableList<TestClone>(ListCloneBehavior.Always) { new(), new(), new() };

        var val = _provider.GetCacheItem("test", () => original);

        Assert.AreEqual(original.Count, val.Count);
        foreach (var item in val)
        {
            Assert.IsTrue(item.IsClone);
        }
    }

    /// <summary>
    /// Tests that when an item is retrieved from the cache, it is deep-cloned and its state is reset, ensuring the cached instance is not the same as the original and is not marked as dirty.
    /// </summary>
    [Test]
    public void Ensures_Cloned_And_Reset()
    {
        var original = new TestClass { Name = "hello" };
        Assert.IsTrue(original.IsDirty());

        var val = _provider.GetCacheItem("test", () => original);

        Assert.AreNotEqual(original.CloneId, val.CloneId);
        Assert.IsFalse(val.IsDirty());
    }

    /// <summary>
    /// Tests that exceptions are not cached by the DeepCloneAppCache provider.
    /// It verifies that exceptions thrown during value retrieval do not get cached,
    /// allowing subsequent calls to attempt retrieval again until a successful value is cached.
    /// </summary>
    [Test]
    public void DoesNotCacheExceptions()
    {
        string value;
        Assert.Throws<Exception>(() => { value = (string)_provider.Get("key", () => GetValue(1)); });
        Assert.Throws<Exception>(() => { value = (string)_provider.Get("key", () => GetValue(2)); });

        // does not throw
        value = (string)_provider.Get("key", () => GetValue(3));
        Assert.AreEqual("succ3", value);

        // cache
        value = (string)_provider.Get("key", () => GetValue(4));
        Assert.AreEqual("succ3", value);
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

    /// <summary>
    /// Initializes a new instance of the <see cref="TestClass"/> class.
    /// </summary>
        public TestClass() => CloneId = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the name of the <see cref="TestClass"/> instance.
    /// </summary>
        public string Name
        {
            get => _name;

            set => SetPropertyValueAndDetectChanges(value, ref _name, nameof(Name));
        }

    /// <summary>
    /// Gets or sets the clone identifier.
    /// </summary>
        public Guid CloneId { get; set; }

    /// <summary>
    /// Creates and returns a deep clone of this <see cref="TestClass"/> instance, including all reference-type properties.
    /// </summary>
    /// <returns>
    /// A new <see cref="TestClass"/> object that is a deep copy of the current instance, with a new <c>CloneId</c> value.
    /// </returns>
        public object DeepClone()
        {
            var shallowClone = (TestClass)MemberwiseClone();
            DeepCloneHelper.DeepCloneRefProperties(this, shallowClone);
            shallowClone.CloneId = Guid.NewGuid();
            return shallowClone;
        }
    }
}
