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
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Composing;

/// <summary>
///     Tests for lazy collection builder.
/// </summary>
/// <remarks>
///     Lazy collection builder does not throw on duplicate, just uses distinct types
///     so we don't have a test for duplicates as we had with resolvers in v7.
/// </remarks>
[TestFixture]
public class LazyCollectionBuilderTests
{
    private IServiceCollection CreateRegister() => TestHelper.GetServiceCollection();

    /// <summary>
    /// Tests that the LazyCollectionBuilder correctly handles adding and resolving types,
    /// ensuring that the collection contains the expected transient objects.
    /// </summary>
    [Test]
    public void LazyCollectionBuilderHandlesTypes()
    {
        var container = CreateRegister();
        var composition = new UmbracoBuilder(container, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

        composition.WithCollectionBuilder<TestCollectionBuilder>()
            .Add<TransientObject3>()
            .Add<TransientObject2>()
            .Add<TransientObject3>()
            .Add<TransientObject1>();

        var factory = composition.CreateServiceProvider();

        var values = factory.GetRequiredService<TestCollection>();

        Assert.AreEqual(3, values.Count());
        Assert.IsTrue(values.Select(x => x.GetType())
            .ContainsAll(new[] { typeof(TransientObject1), typeof(TransientObject2), typeof(TransientObject3) }));

        var other = factory.GetRequiredService<TestCollection>();
        Assert.AreNotSame(values, other); // transient
        var o1 = other.FirstOrDefault(x => x is TransientObject1);
        Assert.IsFalse(values.Contains(o1)); // transient
    }

    /// <summary>
    /// Tests that the LazyCollectionBuilder correctly handles multiple producers,
    /// ensuring that the resulting collection contains the expected unique transient objects,
    /// and that each resolved collection instance is distinct.
    /// </summary>
    [Test]
    public void LazyCollectionBuilderHandlesProducers()
    {
        var container = CreateRegister();
        var composition = new UmbracoBuilder(container, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

        composition.WithCollectionBuilder<TestCollectionBuilder>()
            .Add(() => new[] { typeof(TransientObject3), typeof(TransientObject2) })
            .Add(() => new[] { typeof(TransientObject3), typeof(TransientObject2) })
            .Add(() => new[] { typeof(TransientObject1) });

        var factory = composition.CreateServiceProvider();

        var values = factory.GetRequiredService<TestCollection>();

        Assert.AreEqual(3, values.Count());
        Assert.IsTrue(values.Select(x => x.GetType())
            .ContainsAll(new[] { typeof(TransientObject1), typeof(TransientObject2), typeof(TransientObject3) }));

        var other = factory.GetRequiredService<TestCollection>();
        Assert.AreNotSame(values, other); // transient
        var o1 = other.FirstOrDefault(x => x is TransientObject1);
        Assert.IsFalse(values.Contains(o1)); // transient
    }

    /// <summary>
    /// Verifies that <see cref="LazyCollectionBuilderBase{TBuilder, TCollection, TItem}"/> correctly handles adding both types and producer functions,
    /// and that the resulting collection contains the expected transient objects.
    /// Also ensures that each retrieval yields a new collection instance (transient behavior).
    /// </summary>
    [Test]
    public void LazyCollectionBuilderHandlesTypesAndProducers()
    {
        var container = CreateRegister();
        var composition = new UmbracoBuilder(container, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

        composition.WithCollectionBuilder<TestCollectionBuilder>()
            .Add<TransientObject3>()
            .Add<TransientObject2>()
            .Add<TransientObject3>()
            .Add(() => new[] { typeof(TransientObject1) });

        var factory = composition.CreateServiceProvider();

        var values = factory.GetRequiredService<TestCollection>();

        Assert.AreEqual(3, values.Count());
        Assert.IsTrue(values.Select(x => x.GetType())
            .ContainsAll(new[] { typeof(TransientObject1), typeof(TransientObject2), typeof(TransientObject3) }));

        var other = factory.GetRequiredService<TestCollection>();
        Assert.AreNotSame(values, other); // transient
        var o1 = other.FirstOrDefault(x => x is TransientObject1);
        Assert.IsFalse(values.Contains(o1)); // transient
    }

    /// <summary>
    /// Tests that the LazyCollectionBuilder throws an InvalidOperationException when illegal types
    /// (types that do not implement the required interface) are added to the collection.
    /// </summary>
    [Test]
    public void LazyCollectionBuilderThrowsOnIllegalTypes()
    {
        var container = CreateRegister();
        var composition = new UmbracoBuilder(container, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

        composition.WithCollectionBuilder<TestCollectionBuilder>()
            .Add<TransientObject3>()

            // illegal, does not implement the interface!
            ////.Add<TransientObject4>()

            // legal so far...
            .Add(() => new[] { typeof(TransientObject4) });

        Assert.Throws<InvalidOperationException>(() =>
        {
            // but throws here when trying to register the types, right before creating the factory
            var factory = composition.CreateServiceProvider();
        });
    }

    /// <summary>
    /// Tests that the LazyCollectionBuilder can exclude specified types from the collection.
    /// </summary>
    [Test]
    public void LazyCollectionBuilderCanExcludeTypes()
    {
        var container = CreateRegister();
        var composition = new UmbracoBuilder(container, Mock.Of<IConfiguration>(), TestHelper.GetMockedTypeLoader());

        composition.WithCollectionBuilder<TestCollectionBuilder>()
            .Add<TransientObject3>()
            .Add(() => new[] { typeof(TransientObject3), typeof(TransientObject2), typeof(TransientObject1) })
            .Exclude<TransientObject3>();

        var factory = composition.CreateServiceProvider();

        var values = factory.GetRequiredService<TestCollection>();

        Assert.AreEqual(2, values.Count());
        Assert.IsFalse(values.Select(x => x.GetType())
            .Contains(typeof(TransientObject3)));
        Assert.IsTrue(values.Select(x => x.GetType())
            .ContainsAll(new[] { typeof(TransientObject1), typeof(TransientObject2) }));

        var other = factory.GetRequiredService<TestCollection>();
        Assert.AreNotSame(values, other); // transient
        var o1 = other.FirstOrDefault(x => x is TransientObject1);
        Assert.IsFalse(values.Contains(o1)); // transient
    }

    private interface ITestInterface
    {
    }

    private class TransientObject1 : ITestInterface
    {
    }

    private class TransientObject2 : ITestInterface
    {
    }

    private class TransientObject3 : ITestInterface
    {
    }

    private class TransientObject4
    {
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class
        TestCollectionBuilder : LazyCollectionBuilderBase<TestCollectionBuilder, TestCollection, ITestInterface>
    {
        protected override TestCollectionBuilder This => this;

        protected override ServiceLifetime CollectionLifetime => ServiceLifetime.Transient; // transient
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class TestCollection : BuilderCollectionBase<ITestInterface>
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="TestCollection"/> class.
    /// </summary>
    /// <param name="items">A function that returns the collection of <see cref="ITestInterface"/> items.</param>
        public TestCollection(Func<IEnumerable<ITestInterface>> items)
            : base(items)
        {
        }
    }
}
