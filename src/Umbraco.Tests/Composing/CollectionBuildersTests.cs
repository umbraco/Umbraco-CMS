using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Composing;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Tests.Components;

namespace Umbraco.Tests.Composing
{
    [TestFixture]
    public class CollectionBuildersTests
    {
        private Composition _composition;

        [SetUp]
        public void Setup()
        {
            Current.Reset();

            var register = RegisterFactory.Create();
            _composition = new Composition(register, new TypeLoader(), Mock.Of<IProfilingLogger>(), ComponentTests.MockRuntimeState(RuntimeLevel.Run));
        }

        [TearDown]
        public void TearDown()
        {
            Current.Reset();
        }

        [Test]
        public void ContainsTypes()
        {
            var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>();

            Assert.IsTrue(builder.Has<Resolved1>());
            Assert.IsTrue(builder.Has<Resolved2>());
            Assert.IsFalse(builder.Has<Resolved3>());
            //Assert.IsFalse(col.ContainsType<Resolved4>()); // does not compile

            var factory = _composition.CreateFactory();
            var col = builder.CreateCollection(factory);
            AssertCollection(col, typeof(Resolved1), typeof(Resolved2));
        }

        [Test]
        public void CanClearBuilderBeforeCollectionIsCreated()
        {
            var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>();

            builder.Clear();
            Assert.IsFalse(builder.Has<Resolved1>());
            Assert.IsFalse(builder.Has<Resolved2>());

            var factory = _composition.CreateFactory();
            var col = builder.CreateCollection(factory);
            AssertCollection(col);
        }

        [Test]
        public void CannotClearBuilderOnceCollectionIsCreated()
        {
            var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>();

            var factory = _composition.CreateFactory();
            var col = builder.CreateCollection(factory);

            Assert.Throws<InvalidOperationException>(() => builder.Clear());
        }

        [Test]
        public void CanAppendToBuilder()
        {
            var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>();
            builder.Append<Resolved1>();
            builder.Append<Resolved2>();

            Assert.IsTrue(builder.Has<Resolved1>());
            Assert.IsTrue(builder.Has<Resolved2>());
            Assert.IsFalse(builder.Has<Resolved3>());

            var factory = _composition.CreateFactory();
            var col = builder.CreateCollection(factory);
            AssertCollection(col, typeof(Resolved1), typeof(Resolved2));
        }

        [Test]
        public void CannotAppendToBuilderOnceCollectionIsCreated()
        {
            var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>();

            var factory = _composition.CreateFactory();
            var col = builder.CreateCollection(factory);

            Assert.Throws<InvalidOperationException>(() =>
                builder.Append<Resolved1>()
            );
        }

        [Test]
        public void CanAppendDuplicateToBuilderAndDeDuplicate()
        {
            var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>();
            builder.Append<Resolved1>();
            builder.Append<Resolved1>();

            var factory = _composition.CreateFactory();

            var col = builder.CreateCollection(factory);
            AssertCollection(col, typeof(Resolved1));
        }

        [Test]
        public void CannotAppendInvalidTypeToBUilder()
        {
            var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>();

            //builder.Append<Resolved4>(); // does not compile
            Assert.Throws<InvalidOperationException>(() =>
                    builder.Append(new[] { typeof (Resolved4) }) // throws
            );
        }

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

            var factory = _composition.CreateFactory();
            var col = builder.CreateCollection(factory);
            AssertCollection(col, typeof(Resolved1));
        }

        [Test]
        public void CanRemoveMissingFromBuilder()
        {
            var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>()
                .Remove<Resolved3>();

            var factory = _composition.CreateFactory();
            var col = builder.CreateCollection(factory);
            AssertCollection(col, typeof(Resolved1), typeof(Resolved2));
        }

        [Test]
        public void CannotRemoveFromBuilderOnceCollectionIsCreated()
        {
            var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>();

            var factory = _composition.CreateFactory();
            var col = builder.CreateCollection(factory);
            Assert.Throws<InvalidOperationException>(() =>
                builder.Remove<Resolved2>() // throws
            );
        }

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

            var factory = _composition.CreateFactory();
            var col = builder.CreateCollection(factory);
            AssertCollection(col, typeof(Resolved3), typeof(Resolved1), typeof(Resolved2));
        }

        [Test]
        public void CannotInsertIntoBuilderOnceCollectionIsCreated()
        {
            var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>();

            var factory = _composition.CreateFactory();
            var col = builder.CreateCollection(factory);
            Assert.Throws<InvalidOperationException>(() =>
                builder.Insert<Resolved3>() // throws
            );
        }

        [Test]
        public void CanInsertDuplicateIntoBuilderAndDeDuplicate()
        {
            var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>()
                .Insert<Resolved2>();

            var factory = _composition.CreateFactory();
            var col = builder.CreateCollection(factory);
            AssertCollection(col, typeof(Resolved2), typeof(Resolved1));
        }

        [Test]
        public void CanInsertIntoEmptyBuilder()
        {
            var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>();
            builder.Insert<Resolved2>();

            var factory = _composition.CreateFactory();
            var col = builder.CreateCollection(factory);
            AssertCollection(col, typeof(Resolved2));
        }

        [Test]
        public void CannotInsertIntoBuilderAtWrongIndex()
        {
            var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                builder.Insert<Resolved3>(99) // throws
            );

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                builder.Insert<Resolved3>(-1) // throws
            );
        }

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

            var factory = _composition.CreateFactory();
            var col = builder.CreateCollection(factory);
            AssertCollection(col, typeof(Resolved1), typeof(Resolved3), typeof(Resolved2));
        }

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

            var factory = _composition.CreateFactory();
            var col = builder.CreateCollection(factory);
            AssertCollection(col, typeof(Resolved1), typeof(Resolved3), typeof(Resolved2));
        }

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

            var factory = _composition.CreateFactory();
            var col = builder.CreateCollection(factory);
            AssertCollection(col, typeof(Resolved1), typeof(Resolved2), typeof(Resolved3));
        }

        [Test]
        public void CannotInsertIntoBuilderBeforeOnceCollectionIsCreated()
        {
            var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>();

            var factory = _composition.CreateFactory();
            var col = builder.CreateCollection(factory);
            Assert.Throws<InvalidOperationException>(() =>
                builder.InsertBefore<Resolved2, Resolved3>()
            );
        }

        [Test]
        public void CanInsertDuplicateIntoBuilderBeforeAndDeDuplicate()
        {
            var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>()
                .InsertBefore<Resolved1, Resolved2>();

            var factory = _composition.CreateFactory();
            var col = builder.CreateCollection(factory);
            AssertCollection(col, typeof(Resolved2), typeof(Resolved1));
        }

        [Test]
        public void CannotInsertIntoBuilderBeforeMissing()
        {
            var builder = _composition.WithCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>();

            Assert.Throws<InvalidOperationException>(() =>
                builder.InsertBefore<Resolved2, Resolved3>()
            );
        }

        [Test]
        public void ScopeBuilderCreatesScopedCollection()
        {
            _composition.WithCollectionBuilder<TestCollectionBuilderScope>()
                .Append<Resolved1>()
                .Append<Resolved2>();

            // CreateCollection creates a new collection each time
            // but the container manages the scope, so to test the scope
            // the collection must come from the container

            var factory = _composition.CreateFactory();

            using (factory.BeginScope())
            {
                var col1 = factory.GetInstance<TestCollection>();
                AssertCollection(col1, typeof(Resolved1), typeof(Resolved2));

                var col2 = factory.GetInstance<TestCollection>();
                AssertCollection(col2, typeof(Resolved1), typeof(Resolved2));

                AssertSameCollection(factory, col1, col2);
            }

        }

        [Test]
        public void TransientBuilderCreatesTransientCollection()
        {
            _composition.WithCollectionBuilder<TestCollectionBuilderTransient>()
                .Append<Resolved1>()
                .Append<Resolved2>();

            // CreateCollection creates a new collection each time
            // but the container manages the scope, so to test the scope
            // the collection must come from the container

            var factory = _composition.CreateFactory();

            var col1 = factory.GetInstance<TestCollection>();
            AssertCollection(col1, typeof(Resolved1), typeof(Resolved2));

            var col2 = factory.GetInstance<TestCollection>();
            AssertCollection(col1, typeof(Resolved1), typeof(Resolved2));

            AssertNotSameCollection(col1, col2);
        }

        [Test]
        public void BuilderRespectsTypesOrder()
        {
            var builder = _composition.WithCollectionBuilder<TestCollectionBuilderTransient>()
                .Append<Resolved3>()
                .Insert<Resolved1>()
                .InsertBefore<Resolved3, Resolved2>();

            var factory = _composition.CreateFactory();
            var col1 = builder.CreateCollection(factory);
            AssertCollection(col1, typeof(Resolved1), typeof(Resolved2), typeof(Resolved3));
        }

        [Test]
        public void ScopeBuilderRespectsContainerScope()
        {
            _composition.WithCollectionBuilder<TestCollectionBuilderScope>()
                .Append<Resolved1>()
                .Append<Resolved2>();

            // CreateCollection creates a new collection each time
            // but the container manages the scope, so to test the scope
            // the collection must come from the container

            TestCollection col1A, col1B;

            var factory = _composition.CreateFactory();

            using (factory.BeginScope())
            {
                col1A = factory.GetInstance<TestCollection>();
                col1B = factory.GetInstance<TestCollection>();

                AssertCollection(col1A, typeof(Resolved1), typeof(Resolved2));
                AssertCollection(col1B, typeof(Resolved1), typeof(Resolved2));
                AssertSameCollection(factory, col1A, col1B);
            }

            TestCollection col2;

            using (factory.BeginScope())
            {
                col2 = factory.GetInstance<TestCollection>();
            }

            AssertCollection(col2, typeof(Resolved1), typeof(Resolved2));
            AssertNotSameCollection(col1A, col2);
        }

        [Test]
        public void WeightedBuilderCreatesWeightedCollection()
        {
            var builder = _composition.WithCollectionBuilder<TestCollectionBuilderWeighted>()
               .Add<Resolved1>()
               .Add<Resolved2>();

            var factory = _composition.CreateFactory();
            var col = builder.CreateCollection(factory);
            AssertCollection(col, typeof(Resolved2), typeof(Resolved1));
        }

        [Test]
        public void WeightedBuilderSetWeight()
        {
            var builder = _composition.WithCollectionBuilder<TestCollectionBuilderWeighted>()
                .Add<Resolved1>()
                .Add<Resolved2>();
            builder.SetWeight<Resolved1>(10);

            var factory = _composition.CreateFactory();
            var col = builder.CreateCollection(factory);
            AssertCollection(col, typeof(Resolved1), typeof(Resolved2));
        }

        #region Assertions

        private static void AssertCollection(IEnumerable<Resolved> col, params Type[] expected)
        {
            var colA = col.ToArray();
            Assert.AreEqual(expected.Length, colA.Length);
            for (var i = 0; i < expected.Length; i++)
                Assert.IsInstanceOf(expected[i], colA[i]);
        }

        private static void AssertSameCollection(IFactory factory, IEnumerable<Resolved> col1, IEnumerable<Resolved> col2)
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

                var itemA = factory.GetInstance(col1A[i].GetType());
                var itemB = factory.GetInstance(col2A[i].GetType());

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

        #endregion

        #region Test Objects

        public abstract class Resolved
        { }

        public class Resolved1 : Resolved
        { }

        [Weight(50)] // default is 100
        public class Resolved2 : Resolved
        { }

        public class Resolved3 : Resolved
        { }

        public class Resolved4 // not! : Resolved
        { }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestCollectionBuilder : OrderedCollectionBuilderBase<TestCollectionBuilder, TestCollection, Resolved>
        {
            protected override TestCollectionBuilder This => this;
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestCollectionBuilderTransient : OrderedCollectionBuilderBase<TestCollectionBuilderTransient, TestCollection, Resolved>
        {
            protected override TestCollectionBuilderTransient This => this;

            protected override Lifetime CollectionLifetime => Lifetime.Transient; // transient
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestCollectionBuilderScope : OrderedCollectionBuilderBase<TestCollectionBuilderScope, TestCollection, Resolved>
        {
            protected override TestCollectionBuilderScope This => this;

            protected override Lifetime CollectionLifetime => Lifetime.Scope;
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestCollectionBuilderWeighted : WeightedCollectionBuilderBase<TestCollectionBuilderWeighted, TestCollection, Resolved>
        {
            protected override TestCollectionBuilderWeighted This => this;
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestCollection : BuilderCollectionBase<Resolved>
        {
            public TestCollection(IEnumerable<Resolved> items)
                : base(items)
            { }
        }

        #endregion
    }
}
