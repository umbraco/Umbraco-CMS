using System;
using System.Collections.Generic;
using System.Linq;
using LightInject;
using NUnit.Framework;
using Umbraco.Core.DI;

namespace Umbraco.Tests.DI
{
    [TestFixture]
    public class CollectionBuildersTests
    {
        private ServiceContainer _container;

        [SetUp]
        public void Setup()
        {
            Current.Reset();

            _container = new ServiceContainer();
            _container.ConfigureUmbracoCore();
        }

        [TearDown]
        public void TearDown()
        {
            Current.Reset();

            _container.Dispose();
            _container = null;
        }

        #region Test objects

        public abstract class Resolved
        { }

        public class Resolved1 : Resolved
        { }

        [Weight(5)] // default is 10
        public class Resolved2 : Resolved
        { }

        public class Resolved3 : Resolved
        { }

        public class Resolved4 // not! : Resolved
        { }

        private class TestCollectionBuilder : OrderedCollectionBuilderBase<TestCollectionBuilder, TestCollection, Resolved>
        {
            public TestCollectionBuilder(IServiceContainer container) 
                : base(container)
            { }

            protected override TestCollectionBuilder This => this;
        }

        private class TestCollectionBuilderTransient : OrderedCollectionBuilderBase<TestCollectionBuilderTransient, TestCollection, Resolved>
        {
            public TestCollectionBuilderTransient(IServiceContainer container) 
                : base(container)
            { }

            protected override TestCollectionBuilderTransient This => this;

            protected override ILifetime CollectionLifetime => null; // transient
        }

        private class TestCollectionBuilderScope : OrderedCollectionBuilderBase<TestCollectionBuilderScope, TestCollection, Resolved>
        {
            public TestCollectionBuilderScope(IServiceContainer container)
                : base(container)
            { }

            protected override TestCollectionBuilderScope This => this;

            protected override ILifetime CollectionLifetime => new PerScopeLifetime();
        }

        private class TestCollectionBuilderWeighted : WeightedCollectionBuilderBase<TestCollectionBuilderWeighted, TestCollection, Resolved>
        {
            public TestCollectionBuilderWeighted(IServiceContainer container)
                : base(container)
            { }

            protected override TestCollectionBuilderWeighted This => this;
        }

        private class TestCollection : BuilderCollectionBase<Resolved>
        {
            public TestCollection(IEnumerable<Resolved> items) 
                : base(items)
            { }
        }

        #endregion

        [Test]
        public void ContainsTypes()
        {
            var builder = _container.RegisterCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>();

            Assert.IsTrue(builder.Has<Resolved1>());
            Assert.IsTrue(builder.Has<Resolved2>());
            Assert.IsFalse(builder.Has<Resolved3>());
            //Assert.IsFalse(col.ContainsType<Resolved4>()); // does not compile

            var col = builder.CreateCollection();
            AssertCollection(col, typeof(Resolved1), typeof(Resolved2));
        }

        [Test]
        public void Clear()
        {
            var builder = _container.RegisterCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>();

            builder.Clear();
            Assert.IsFalse(builder.Has<Resolved1>());
            Assert.IsFalse(builder.Has<Resolved2>());

            var col = builder.CreateCollection();
            AssertCollection(col);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ClearOnceResolved()
        {
            var builder = _container.RegisterCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>();

            var col = builder.CreateCollection();
            builder.Clear();
        }

        [Test]
        public void Append()
        {
            var builder = _container.RegisterCollectionBuilder<TestCollectionBuilder>();
            builder.Append<Resolved1>();
            builder.Append<Resolved2>();

            Assert.IsTrue(builder.Has<Resolved1>());
            Assert.IsTrue(builder.Has<Resolved2>());
            Assert.IsFalse(builder.Has<Resolved3>());

            var col = builder.CreateCollection();
            AssertCollection(col, typeof(Resolved1), typeof(Resolved2));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AppendOnceResolved()
        {
            var builder = _container.RegisterCollectionBuilder<TestCollectionBuilder>();

            var col = builder.CreateCollection();
            builder.Append<Resolved1>();
        }

        [Test]
        public void AppendDuplicate()
        {
            var builder = _container.RegisterCollectionBuilder<TestCollectionBuilder>();
            builder.Append<Resolved1>();
            builder.Append<Resolved1>();

            var col = builder.CreateCollection();
            AssertCollection(col, typeof(Resolved1));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AppendInvalid()
        {
            var builder = _container.RegisterCollectionBuilder<TestCollectionBuilder>();
            //builder.Append<Resolved4>(); // does not compile
            builder.Append(new[] { typeof(Resolved4) }); // throws
        }

        [Test]
        public void Remove()
        {
            var builder = _container.RegisterCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>()
                .Remove<Resolved2>();

            Assert.IsTrue(builder.Has<Resolved1>());
            Assert.IsFalse(builder.Has<Resolved2>());
            Assert.IsFalse(builder.Has<Resolved3>());

            var col = builder.CreateCollection();
            AssertCollection(col, typeof(Resolved1));
        }

        [Test]
        public void RemoveAbsent()
        {
            var builder = _container.RegisterCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>()
                .Remove<Resolved3>();

            var col = builder.CreateCollection();
            AssertCollection(col, typeof(Resolved1), typeof(Resolved2));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RemoveOnceResolved()
        {
            var builder = _container.RegisterCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>();

            var col = builder.CreateCollection();
            builder.Remove<Resolved2>(); // throws
        }

        [Test]
        public void Insert()
        {
            var builder = _container.RegisterCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>()
                .Insert<Resolved3>();

            Assert.IsTrue(builder.Has<Resolved1>());
            Assert.IsTrue(builder.Has<Resolved2>());
            Assert.IsTrue(builder.Has<Resolved3>());

            var col = builder.CreateCollection();
            AssertCollection(col, typeof(Resolved3), typeof(Resolved1), typeof(Resolved2));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InsertOnceResolved()
        {
            var builder = _container.RegisterCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>();

            var col = builder.CreateCollection();
            builder.Insert<Resolved3>(); // throws
        }

        [Test]
        public void CanInsertDuplicate()
        {
            var builder = _container.RegisterCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>()
                .Insert<Resolved2>();

            var col = builder.CreateCollection();
            AssertCollection(col, typeof(Resolved2), typeof(Resolved1));
        }

        [Test]
        public void InsertInEmpty()
        {
            var builder = _container.RegisterCollectionBuilder<TestCollectionBuilder>();
            builder.Insert<Resolved2>();

            var col = builder.CreateCollection();
            AssertCollection(col, typeof(Resolved2));
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InsertAtWrongIndex1()
        {
            var builder = _container.RegisterCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>();

            builder.Insert<Resolved3>(99); // throws
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InsertAtWrongIndex2()
        {
            var builder = _container.RegisterCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>();

            builder.Insert<Resolved3>(-1); // throws
        }

        [Test]
        public void InsertBefore()
        {
            var builder = _container.RegisterCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>()
                .InsertBefore<Resolved2, Resolved3>();

            Assert.IsTrue(builder.Has<Resolved1>());
            Assert.IsTrue(builder.Has<Resolved2>());
            Assert.IsTrue(builder.Has<Resolved3>());

            var col = builder.CreateCollection();
            AssertCollection(col, typeof(Resolved1), typeof(Resolved3), typeof(Resolved2));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InsertBeforeOnceResolved()
        {
            var builder = _container.RegisterCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>();

            var col = builder.CreateCollection();
            builder.InsertBefore<Resolved2, Resolved3>();
        }

        [Test]
        public void InsertBeforeDuplicate()
        {
            var builder = _container.RegisterCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>()
                .InsertBefore<Resolved1, Resolved2>();

            var col = builder.CreateCollection();
            AssertCollection(col, typeof(Resolved2), typeof(Resolved1));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InsertBeforeAbsent()
        {
            var builder = _container.RegisterCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .InsertBefore<Resolved2, Resolved3>();
        }

        [Test]
        public void ScopeIsApplication()
        {
            _container.RegisterCollectionBuilder<TestCollectionBuilder>()
                .Append<Resolved1>()
                .Append<Resolved2>();

            // CreateCollection creates a new collection each time
            // but the container manages the scope, so to test the scope
            // the collection must come from the container

            var col1 = _container.GetInstance<TestCollection>();
            AssertCollection(col1, typeof(Resolved1), typeof(Resolved2));

            var col2 = _container.GetInstance<TestCollection>();
            AssertCollection(col2, typeof(Resolved1), typeof(Resolved2));

            AssertSameCollection(col1, col2);
        }

        [Test]
        public void ScopeIsTransient()
        {
            _container.RegisterCollectionBuilder<TestCollectionBuilderTransient>()
                .Append<Resolved1>()
                .Append<Resolved2>();

            // CreateCollection creates a new collection each time
            // but the container manages the scope, so to test the scope
            // the collection must come from the container

            var col1 = _container.GetInstance<TestCollection>();
            AssertCollection(col1, typeof(Resolved1), typeof(Resolved2));

            var col2 = _container.GetInstance<TestCollection>();
            AssertCollection(col1, typeof(Resolved1), typeof(Resolved2));

            AssertNotSameCollection(col1, col2);
        }

        [Test]
        public void OrderOfTypes()
        {
            var builder = _container.RegisterCollectionBuilder<TestCollectionBuilderTransient>()
                .Append<Resolved3>()
                .Insert<Resolved1>()
                .InsertBefore<Resolved3, Resolved2>();

            var col1 = builder.CreateCollection();
            AssertCollection(col1, typeof(Resolved1), typeof(Resolved2), typeof(Resolved3));
        }

        [Test]
        public void ScopeIsScope()
        {
            _container.RegisterCollectionBuilder<TestCollectionBuilderScope>()
                .Append<Resolved1>()
                .Append<Resolved2>();

            // CreateCollection creates a new collection each time
            // but the container manages the scope, so to test the scope
            // the collection must come from the container

            var scope1 = _container.BeginScope();

            var col1A = _container.GetInstance<TestCollection>();
            AssertCollection(col1A, typeof(Resolved1), typeof(Resolved2));
            var col1B = _container.GetInstance<TestCollection>();
            AssertCollection(col1B, typeof(Resolved1), typeof(Resolved2));

            AssertSameCollection(col1A, col1B);

            _container.EndCurrentScope();
            var scope2 = _container.BeginScope();

            var col2 = _container.GetInstance<TestCollection>();
            AssertCollection(col2, typeof(Resolved1), typeof(Resolved2));

            AssertNotSameCollection(col1A, col2);

            _container.EndCurrentScope();
        }

        [Test]
        public void Weights()
        {
            var builder = _container.RegisterCollectionBuilder<TestCollectionBuilderWeighted>()
               .Add<Resolved1>()
               .Add<Resolved2>();

            var col = builder.CreateCollection();
            AssertCollection(col, typeof(Resolved2), typeof(Resolved1));
        }

        private static void AssertCollection(IEnumerable<Resolved> col, params Type[] expected)
        {
            var colA = col.ToArray();
            Assert.AreEqual(expected.Length, colA.Length);
            for (var i = 0; i < expected.Length; i++)
                Assert.IsInstanceOf(expected[i], colA[i]);
        }

        private static void AssertSameCollection(IEnumerable<Resolved> col1, IEnumerable<Resolved> col2)
        {
            Assert.AreSame(col1, col2);

            var col1A = col1.ToArray();
            var col2A = col2.ToArray();

            Assert.AreEqual(col1A.Length, col2A.Length);
            for (var i = 0; i < col1A.Length; i++)
                Assert.AreSame(col1A[i], col2A[i]);
        }

        private static void AssertNotSameCollection(IEnumerable<Resolved> col1, IEnumerable<Resolved> col2)
        {
            Assert.AreNotSame(col1, col2);

            var col1A = col1.ToArray();
            var col2A = col2.ToArray();

            Assert.AreEqual(col1A.Length, col2A.Length);
            for (var i = 0; i < col1A.Length; i++)
                Assert.AreNotSame(col1A[i], col2A[i]);
        }
    }
}
