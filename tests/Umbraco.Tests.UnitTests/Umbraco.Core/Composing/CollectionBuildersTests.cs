// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Composing;

/// <summary>
/// Contains unit tests for collection builders (e.g. <see cref="LazyCollectionBuilderBase{TBuilder, TCollection, TItem}"/>) in the <c>Umbraco.Core.Composing</c> namespace.
/// </summary>
[TestFixture]
public class CollectionBuildersTests
{
    /// <summary>
    /// Sets up the test environment before each test.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        var register = TestHelper.GetServiceCollection();
        _composition = new UmbracoBuilder(register, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());
    }

    /// <summary>
    /// Cleans up after each test.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
    }

    private IUmbracoBuilder _composition;

    /// <summary>
    /// Tests that the collection builder correctly identifies contained types.
    /// </summary>
    [Test]
    public void ContainsTypes()
    {
        var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
            .Append<Resolved1>()
            .Append<Resolved2>();

        Assert.IsTrue(builder.Has<Resolved1>());
        Assert.IsTrue(builder.Has<Resolved2>());
        Assert.IsFalse(builder.Has<Resolved3>());
        //// Assert.IsFalse(col.ContainsType<Resolved4>()); // does not compile

        var factory = _composition.CreateServiceProvider();
        var col = builder.CreateCollection(factory);
        AssertCollection(col, typeof(Resolved1), typeof(Resolved2));
    }

    /// <summary>
    /// Tests that the collection builder can be cleared before the collection is created.
    /// </summary>
    [Test]
    public void CanClearBuilderBeforeCollectionIsCreated()
    {
        var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
            .Append<Resolved1>()
            .Append<Resolved2>();

        builder.Clear();
        Assert.IsFalse(builder.Has<Resolved1>());
        Assert.IsFalse(builder.Has<Resolved2>());

        var factory = _composition.CreateServiceProvider();
        var col = builder.CreateCollection(factory);
        AssertCollection(col);
    }

    /// <summary>
    /// Tests that the collection builder cannot be cleared once the collection has been created.
    /// </summary>
    [Test]
    public void CannotClearBuilderOnceCollectionIsCreated()
    {
        var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
            .Append<Resolved1>()
            .Append<Resolved2>();

        var factory = _composition.CreateServiceProvider();
        var col = builder.CreateCollection(factory);

        Assert.Throws<InvalidOperationException>(() => builder.Clear());
    }

    /// <summary>
    /// Tests that items can be appended to the collection builder and verifies their presence.
    /// </summary>
    [Test]
    public void CanAppendToBuilder()
    {
        var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>();
        builder.Append<Resolved1>();
        builder.Append<Resolved2>();

        Assert.IsTrue(builder.Has<Resolved1>());
        Assert.IsTrue(builder.Has<Resolved2>());
        Assert.IsFalse(builder.Has<Resolved3>());

        var factory = _composition.CreateServiceProvider();
        var col = builder.CreateCollection(factory);
        AssertCollection(col, typeof(Resolved1), typeof(Resolved2));
    }

    /// <summary>
    /// Tests that appending to a collection builder after the collection has been created throws an InvalidOperationException.
    /// </summary>
    [Test]
    public void CannotAppendToBuilderOnceCollectionIsCreated()
    {
        var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>();

        var factory = _composition.CreateServiceProvider();
        var col = builder.CreateCollection(factory);

        Assert.Throws<InvalidOperationException>(() => builder.Append<Resolved1>());
    }

    /// <summary>
    /// Tests that appending duplicate items to the builder results in a de-duplicated collection.
    /// </summary>
    [Test]
    public void CanAppendDuplicateToBuilderAndDeDuplicate()
    {
        var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>();
        builder.Append<Resolved1>();
        builder.Append<Resolved1>();

        var factory = _composition.CreateServiceProvider();

        var col = builder.CreateCollection(factory);
        AssertCollection(col, typeof(Resolved1));
    }

    /// <summary>
    /// Tests that appending an invalid type to the collection builder throws an InvalidOperationException.
    /// </summary>
    [Test]
    public void CannotAppendInvalidTypeToBUilder()
    {
        var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>();

        ////builder.Append<Resolved4>(); // does not compile
        Assert.Throws<InvalidOperationException>(() => builder.Append(new[] { typeof(Resolved4) }));
    }

    /// <summary>
    /// Tests that items can be removed from the collection builder.
    /// </summary>
    [Test]
    public void CanRemoveFromBuilder()
    {
        var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
            .Append<Resolved1>()
            .Append<Resolved2>()
            .Remove<Resolved2>();

        Assert.IsTrue(builder.Has<Resolved1>());
        Assert.IsFalse(builder.Has<Resolved2>());
        Assert.IsFalse(builder.Has<Resolved3>());

        var factory = _composition.CreateServiceProvider();
        var col = builder.CreateCollection(factory);
        AssertCollection(col, typeof(Resolved1));
    }

    /// <summary>
    /// Tests that removing a missing item from the collection builder does not affect existing items.
    /// </summary>
    [Test]
    public void CanRemoveMissingFromBuilder()
    {
        var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
            .Append<Resolved1>()
            .Append<Resolved2>()
            .Remove<Resolved3>();

        var factory = _composition.CreateServiceProvider();
        var col = builder.CreateCollection(factory);
        AssertCollection(col, typeof(Resolved1), typeof(Resolved2));
    }

    /// <summary>
    /// Tests that removing an item from the builder after the collection has been created throws an InvalidOperationException.
    /// </summary>
    [Test]
    public void CannotRemoveFromBuilderOnceCollectionIsCreated()
    {
        var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
            .Append<Resolved1>()
            .Append<Resolved2>();

        var factory = _composition.CreateServiceProvider();
        var col = builder.CreateCollection(factory);
        Assert.Throws<InvalidOperationException>(() => builder.Remove<Resolved2>());
    }

    /// <summary>
    /// Tests that items can be inserted into the collection builder and verifies their presence and order.
    /// </summary>
    [Test]
    public void CanInsertIntoBuilder()
    {
        var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
            .Append<Resolved1>()
            .Append<Resolved2>()
            .Insert<Resolved3>();

        Assert.IsTrue(builder.Has<Resolved1>());
        Assert.IsTrue(builder.Has<Resolved2>());
        Assert.IsTrue(builder.Has<Resolved3>());

        var factory = _composition.CreateServiceProvider();
        var col = builder.CreateCollection(factory);
        AssertCollection(col, typeof(Resolved3), typeof(Resolved1), typeof(Resolved2));
    }

    /// <summary>
    /// Verifies that once a collection is created from the builder, inserting new items into the builder throws an InvalidOperationException.
    /// </summary>
    [Test]
    public void CannotInsertIntoBuilderOnceCollectionIsCreated()
    {
        var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
            .Append<Resolved1>()
            .Append<Resolved2>();

        var factory = _composition.CreateServiceProvider();
        var col = builder.CreateCollection(factory);
        Assert.Throws<InvalidOperationException>(() => builder.Insert<Resolved3>());
    }

    /// <summary>
    /// Tests that inserting a duplicate into the builder results in de-duplication.
    /// </summary>
    [Test]
    public void CanInsertDuplicateIntoBuilderAndDeDuplicate()
    {
        var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
            .Append<Resolved1>()
            .Append<Resolved2>()
            .Insert<Resolved2>();

        var factory = _composition.CreateServiceProvider();
        var col = builder.CreateCollection(factory);
        AssertCollection(col, typeof(Resolved2), typeof(Resolved1));
    }

    /// <summary>
    /// Tests that an item can be inserted into an empty collection builder.
    /// </summary>
    [Test]
    public void CanInsertIntoEmptyBuilder()
    {
        var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>();
        builder.Insert<Resolved2>();

        var factory = _composition.CreateServiceProvider();
        var col = builder.CreateCollection(factory);
        AssertCollection(col, typeof(Resolved2));
    }

    /// <summary>
    /// Tests that inserting into a collection builder at an invalid index throws an ArgumentOutOfRangeException.
    /// </summary>
    [Test]
    public void CannotInsertIntoBuilderAtWrongIndex()
    {
        var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
            .Append<Resolved1>()
            .Append<Resolved2>();

        Assert.Throws<ArgumentOutOfRangeException>(() => builder.Insert<Resolved3>(99));

        Assert.Throws<ArgumentOutOfRangeException>(() => builder.Insert<Resolved3>(-1));
    }

    /// <summary>
    /// Tests that an item can be inserted into the builder before a specified existing item.
    /// </summary>
    [Test]
    public void CanInsertIntoBuilderBefore()
    {
        var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
            .Append<Resolved1>()
            .Append<Resolved2>()
            .InsertBefore<Resolved2, Resolved3>();

        Assert.IsTrue(builder.Has<Resolved1>());
        Assert.IsTrue(builder.Has<Resolved2>());
        Assert.IsTrue(builder.Has<Resolved3>());

        var factory = _composition.CreateServiceProvider();
        var col = builder.CreateCollection(factory);
        AssertCollection(col, typeof(Resolved1), typeof(Resolved3), typeof(Resolved2));
    }

    /// <summary>
    /// Tests that an item can be inserted into the collection builder after a specified existing item.
    /// </summary>
    [Test]
    public void CanInsertIntoBuilderAfter()
    {
        var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
            .Append<Resolved1>()
            .Append<Resolved2>()
            .InsertAfter<Resolved1, Resolved3>();

        Assert.IsTrue(builder.Has<Resolved1>());
        Assert.IsTrue(builder.Has<Resolved2>());
        Assert.IsTrue(builder.Has<Resolved3>());

        var factory = _composition.CreateServiceProvider();
        var col = builder.CreateCollection(factory);
        AssertCollection(col, typeof(Resolved1), typeof(Resolved3), typeof(Resolved2));
    }

    /// <summary>
    /// Tests that an item can be inserted into the collection builder after the last existing item.
    /// </summary>
    [Test]
    public void CanInsertIntoBuilderAfterLast()
    {
        var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
            .Append<Resolved1>()
            .Append<Resolved2>()
            .InsertAfter<Resolved2, Resolved3>();

        Assert.IsTrue(builder.Has<Resolved1>());
        Assert.IsTrue(builder.Has<Resolved2>());
        Assert.IsTrue(builder.Has<Resolved3>());

        var factory = _composition.CreateServiceProvider();
        var col = builder.CreateCollection(factory);
        AssertCollection(col, typeof(Resolved1), typeof(Resolved2), typeof(Resolved3));
    }

    /// <summary>
    /// Tests that inserting into the builder before the collection is created throws an InvalidOperationException.
    /// </summary>
    [Test]
    public void CannotInsertIntoBuilderBeforeOnceCollectionIsCreated()
    {
        var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
            .Append<Resolved1>()
            .Append<Resolved2>();

        var factory = _composition.CreateServiceProvider();
        var col = builder.CreateCollection(factory);
        Assert.Throws<InvalidOperationException>(() =>
            builder.InsertBefore<Resolved2, Resolved3>());
    }

    /// <summary>
    /// Tests that inserting a duplicate item into the collection builder before another item works correctly and that duplicates are de-duplicated.
    /// </summary>
    [Test]
    public void CanInsertDuplicateIntoBuilderBeforeAndDeDuplicate()
    {
        var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
            .Append<Resolved1>()
            .Append<Resolved2>()
            .InsertBefore<Resolved1, Resolved2>();

        var factory = _composition.CreateServiceProvider();
        var col = builder.CreateCollection(factory);
        AssertCollection(col, typeof(Resolved2), typeof(Resolved1));
    }

    /// <summary>
    /// Tests that inserting into a builder before a missing item throws an InvalidOperationException.
    /// </summary>
    [Test]
    public void CannotInsertIntoBuilderBeforeMissing()
    {
        var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
            .Append<Resolved1>();

        Assert.Throws<InvalidOperationException>(() =>
            builder.InsertBefore<Resolved2, Resolved3>());
    }

    /// <summary>
    /// Tests that the scope builder creates a scoped collection and
    /// ensures the same collection instance is returned within the same scope.
    /// </summary>
    [Test]
    public void ScopeBuilderCreatesScopedCollection()
    {
        _composition.WithCollectionBuilder<TestCollectionBuilderScope>()
            .Append<Resolved1>()
            .Append<Resolved2>();

        // CreateCollection creates a new collection each time
        // but the container manages the scope, so to test the scope
        // the collection must come from the container.
        var factory = _composition.CreateServiceProvider();

        using (var scope = factory.CreateScope())
        {
            var col1 = scope.ServiceProvider.GetRequiredService<TestCollection>();
            AssertCollection(col1, typeof(Resolved1), typeof(Resolved2));

            var col2 = scope.ServiceProvider.GetRequiredService<TestCollection>();
            AssertCollection(col2, typeof(Resolved1), typeof(Resolved2));

            AssertSameCollection(scope.ServiceProvider, col1, col2);
        }
    }

    /// <summary>
    /// Tests that the transient collection builder creates a new collection instance each time it is requested.
    /// </summary>
    [Test]
    public void TransientBuilderCreatesTransientCollection()
    {
        _composition.WithCollectionBuilder<TestCollectionBuilderTransient>()
            .Append<Resolved1>()
            .Append<Resolved2>();

        // CreateCollection creates a new collection each time
        // but the container manages the scope, so to test the scope
        // the collection must come from the container.
        var factory = _composition.CreateServiceProvider();

        var col1 = factory.GetRequiredService<TestCollection>();
        AssertCollection(col1, typeof(Resolved1), typeof(Resolved2));

        var col2 = factory.GetRequiredService<TestCollection>();
        AssertCollection(col1, typeof(Resolved1), typeof(Resolved2));

        AssertNotSameCollection(col1, col2);
    }

    /// <summary>
    /// Tests that the builder respects the order of types when appending and inserting.
    /// </summary>
    [Test]
    public void BuilderRespectsTypesOrder()
    {
        var builder = _composition.WithCollectionBuilder<TestCollectionBuilderTransient>()
            .Append<Resolved3>()
            .Insert<Resolved1>()
            .InsertBefore<Resolved3, Resolved2>();

        var factory = _composition.CreateServiceProvider();
        var col1 = builder.CreateCollection(factory);
        AssertCollection(col1, typeof(Resolved1), typeof(Resolved2), typeof(Resolved3));
    }

    /// <summary>
    /// Tests that the scope builder respects the container scope by ensuring
    /// that collections resolved within the same scope are the same instance,
    /// and collections resolved in different scopes are different instances.
    /// </summary>
    [Test]
    public void ScopeBuilderRespectsContainerScope()
    {
        _composition.WithCollectionBuilder<TestCollectionBuilderScope>()
            .Append<Resolved1>()
            .Append<Resolved2>();

        // CreateCollection creates a new collection each time
        // but the container manages the scope, so to test the scope
        // the collection must come from the container/
        var serviceProvider = _composition.CreateServiceProvider();

        TestCollection col1A, col1B;
        using (var scope = serviceProvider.CreateScope())
        {
            col1A = scope.ServiceProvider.GetRequiredService<TestCollection>();
            col1B = scope.ServiceProvider.GetRequiredService<TestCollection>();

            AssertCollection(col1A, typeof(Resolved1), typeof(Resolved2));
            AssertCollection(col1B, typeof(Resolved1), typeof(Resolved2));
            AssertSameCollection(serviceProvider, col1A, col1B);
        }

        TestCollection col2;

        using (var scope = serviceProvider.CreateScope())
        {
            col2 = scope.ServiceProvider.GetRequiredService<TestCollection>();

            // NOTE: We must assert here so that the lazy collection is resolved
            // within this service provider scope, else if you resolve the collection
            // after the service provider scope is disposed, you'll get an object
            // disposed error (expected).
            AssertCollection(col2, typeof(Resolved1), typeof(Resolved2));
        }

        AssertNotSameCollection(col1A, col2);
    }

    /// <summary>
    /// Tests that the weighted builder creates a weighted collection with the expected order.
    /// </summary>
    [Test]
    public void WeightedBuilderCreatesWeightedCollection()
    {
        var builder = _composition.WithCollectionBuilder<TestCollectionBuilderWeighted>()
            .Add<Resolved1>()
            .Add<Resolved2>();

        var factory = _composition.CreateServiceProvider();
        var col = builder.CreateCollection(factory);
        AssertCollection(col, typeof(Resolved2), typeof(Resolved1));
    }

    /// <summary>
    /// Tests that the weight of a builder can be set and that the collection respects this weight.
    /// </summary>
    [Test]
    public void WeightedBuilderSetWeight()
    {
        var builder = _composition.WithCollectionBuilder<TestCollectionBuilderWeighted>()
            .Add<Resolved1>()
            .Add<Resolved2>();
        builder.SetWeight<Resolved1>(10);

        var factory = _composition.CreateServiceProvider();
        var col = builder.CreateCollection(factory);
        AssertCollection(col, typeof(Resolved1), typeof(Resolved2));
    }

    private static void AssertCollection(IEnumerable<Resolved> col, params Type[] expected)
    {
        var colA = col.ToArray();
        Assert.AreEqual(expected.Length, colA.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.IsInstanceOf(expected[i], colA[i]);
        }
    }

    private static void AssertSameCollection(IServiceProvider factory, IEnumerable<Resolved> col1, IEnumerable<Resolved> col2)
    {
        Assert.AreSame(col1, col2);

        var col1A = col1.ToArray();
        var col2A = col2.ToArray();

        Assert.AreEqual(col1A.Length, col2A.Length);

        // Ensure each item in each collection is the same but also
        // resolve each item from the factory to ensure it's also the same since
        // it should have the same lifespan.
        for (var i = 0; i < col1A.Length; i++)
        {
            Assert.AreSame(col1A[i], col2A[i]);

            var itemA = factory.GetRequiredService(col1A[i].GetType());
            var itemB = factory.GetRequiredService(col2A[i].GetType());

            Assert.AreSame(itemA, itemB);
        }
    }

    private static void AssertNotSameCollection(IEnumerable<Resolved> col1, IEnumerable<Resolved> col2)
    {
        Assert.AreNotSame(col1, col2);

        var col1A = col1.ToArray();
        var col2A = col2.ToArray();

        Assert.AreEqual(col1A.Length, col2A.Length);

        for (var i = 0; i < col1A.Length; i++)
        {
            Assert.AreNotSame(col1A[i], col2A[i]);
        }
    }

    /// <summary>
    /// Contains tests for verifying the behavior of resolved collection builders in the Umbraco composing system.
    /// </summary>
    public abstract class Resolved
    {
    }

    /// <summary>
    /// Represents a test class for the Resolved1 collection builder used in unit tests.
    /// Contains tests related to the functionality and behavior of the Resolved1 collection builder.
    /// </summary>
    public class Resolved1 : Resolved
    {
    }

    /// <summary>
    /// Contains unit tests for the Resolved2 feature in the CollectionBuilders class.
    /// </summary>
    [Weight(50)] // default is 100
    public class Resolved2 : Resolved
    {
    }

    /// <summary>
    /// Tests dependency resolution in the collection builder for scenario 3.
    /// </summary>
    public class Resolved3 : Resolved
    {
    }

    /// <summary>
    /// Tests the resolution behavior of the fourth collection builder in the CollectionBuildersTests suite.
    /// </summary>
    public class Resolved4 // not! : Resolved
    {
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class TestCollectionBuilder : OrderedCollectionBuilderBase<TestCollectionBuilder, TestCollection, Resolved>
    {
        protected override TestCollectionBuilder This => this;
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class
        TestCollectionBuilderTransient : OrderedCollectionBuilderBase<TestCollectionBuilderTransient, TestCollection,
            Resolved>
    {
        protected override TestCollectionBuilderTransient This => this;

        protected override ServiceLifetime CollectionLifetime => ServiceLifetime.Transient; // transient
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class
        TestCollectionBuilderScope : OrderedCollectionBuilderBase<TestCollectionBuilderScope, TestCollection, Resolved>
    {
        protected override TestCollectionBuilderScope This => this;

        protected override ServiceLifetime CollectionLifetime => ServiceLifetime.Scoped;
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class
        TestCollectionBuilderWeighted : WeightedCollectionBuilderBase<TestCollectionBuilderWeighted, TestCollection,
            Resolved>
    {
        protected override TestCollectionBuilderWeighted This => this;
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class TestCollection : BuilderCollectionBase<Resolved>
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="TestCollection"/> class.
    /// </summary>
    /// <param name="items">A function that returns an enumerable of <see cref="Resolved"/> items.</param>
        public TestCollection(Func<IEnumerable<Resolved>> items)
            : base(items)
        {
        }
    }
}
