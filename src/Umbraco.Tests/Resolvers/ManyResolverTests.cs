using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Moq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Tests.TestHelpers;

using NUnit.Framework;

namespace Umbraco.Tests.Resolvers
{
    [TestFixture]
    public class ManyResolverTests
    {
        [SetUp]
        public void Setup()
        {
            ManyResolver.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            ManyResolver.Reset();
        }

        #region Resolvers and Resolved

        public abstract class Resolved
        { }

        public class Resolved1 : Resolved
        { }

        [WeightedPlugin(5)] // default is 10
        public class Resolved2 : Resolved
        { }

        public class Resolved3 : Resolved
        { }

        public class Resolved4 // not! : Resolved
        { }

        public sealed class ManyResolver : ManyObjectsResolverBase<ManyResolver, Resolved>
        {
            public ManyResolver(IServiceProvider serviceProvider, ILogger logger)
                : base(serviceProvider, logger)
            { }

            public ManyResolver(IServiceProvider serviceProvider, ILogger logger, IEnumerable<Type> value)
                : base(serviceProvider, logger, value)
            { }

            public ManyResolver(IServiceProvider serviceProvider, ILogger logger, IEnumerable<Type> value, ObjectLifetimeScope scope)
                : base(serviceProvider, logger, value, scope)
            { }

            public ManyResolver(IServiceProvider serviceProvider, ILogger logger, HttpContextBase httpContext)
                : base(serviceProvider, logger, httpContext)
            { }

            public IEnumerable<Resolved> SortedResolvedObjects { get { return GetSortedValues(); } }
            public IEnumerable<Resolved> ResolvedObjects { get { return Values; } }
        }

        #endregion

        #region Test ManyResolver types collection manipulation

        [Test]
        public void ManyResolverContainsTypes()
        {
            var resolver = new ManyResolver(
                new ActivatorServiceProvider(), Mock.Of<ILogger>(),
                new Type[] { typeof(Resolved1), typeof(Resolved2) });

            Assert.IsTrue(resolver.ContainsType<Resolved1>());
            Assert.IsTrue(resolver.ContainsType<Resolved2>());
            Assert.IsFalse(resolver.ContainsType<Resolved3>());
            //Assert.IsFalse(resolver.ContainsType<Resolved4>()); // does not compile
        }

        [Test]
        public void ManyResolverCanClearBeforeFreeze()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>());
            resolver.AddType<Resolved1>();
            resolver.AddType<Resolved2>();
            resolver.Clear();
            Assert.IsFalse(resolver.ContainsType<Resolved1>());
            Assert.IsFalse(resolver.ContainsType<Resolved2>());
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ManyResolverCannotClearOnceFrozen()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>());
            resolver.AddType<Resolved1>();
            resolver.AddType<Resolved2>();
            Resolution.Freeze();
            resolver.Clear();
        }

        [Test]
        public void ManyResolverCanAddTypeBeforeFreeze()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>());
            resolver.AddType<Resolved1>();
            resolver.AddType<Resolved2>();

            Assert.IsTrue(resolver.ContainsType<Resolved1>());
            Assert.IsTrue(resolver.ContainsType<Resolved2>());
            Assert.IsFalse(resolver.ContainsType<Resolved3>());
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ManyResolverCannotAddTypeOnceFrozen()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>());
            Resolution.Freeze();
            resolver.AddType<Resolved1>();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ManyResolverCannotAddTypeAgain()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>());
            resolver.AddType<Resolved1>();
            resolver.AddType<Resolved1>();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ManyResolverCannotAddInvalidType()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>());
            //resolver.AddType<Resolved4>(); // does not compile
            resolver.AddType(typeof(Resolved4)); // throws
        }

        [Test]
        public void ManyResolverCanRemoveTypeBeforeFreeze()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>(), new Type[] { typeof(Resolved1), typeof(Resolved2) });
            resolver.RemoveType<Resolved2>();

            Assert.IsTrue(resolver.ContainsType<Resolved1>());
            Assert.IsFalse(resolver.ContainsType<Resolved2>());
            Assert.IsFalse(resolver.ContainsType<Resolved3>());
        }

        [Test]
        public void ManyResolverCanRemoveAbsentType()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>(), new Type[] { typeof(Resolved1), typeof(Resolved2) });
            resolver.RemoveType<Resolved3>();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ManyResolverCannotRemoveInvalidType()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>(), new Type[] { typeof(Resolved1), typeof(Resolved2) });
            //resolver.RemoveType<Resolved4>(); // does not compile
            resolver.RemoveType(typeof(Resolved4)); // throws
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ManyResolverCannotRemoveTypeOnceFrozen()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>(), new Type[] { typeof(Resolved1), typeof(Resolved2) });
            Resolution.Freeze();
            resolver.RemoveType<Resolved2>(); // throws
        }

        [Test]
        public void ManyResolverCanInsertTypeBeforeFreeze()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>(), new Type[] { typeof(Resolved1), typeof(Resolved2) });
            resolver.InsertType<Resolved3>(0);

            Assert.IsTrue(resolver.ContainsType<Resolved1>());
            Assert.IsTrue(resolver.ContainsType<Resolved2>());
            Assert.IsTrue(resolver.ContainsType<Resolved3>());
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ManyResolverCannotInsertTypeOnceFrozen()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>(), new Type[] { typeof(Resolved1), typeof(Resolved2) });
            Resolution.Freeze();
            resolver.InsertType<Resolved3>(0);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ManyResolverCannotInsertTypeAgain()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>(), new Type[] { typeof(Resolved1), typeof(Resolved2) });
            resolver.InsertType<Resolved2>(0);
        }

        [Test]
        public void ManyResolverCanInsertInEmptyList()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>());
            resolver.InsertType<Resolved2>();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ManyResolverCannotInsertInvalidType()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>(), new Type[] { typeof(Resolved1), typeof(Resolved2) });
            //resolver.InsertType<Resolved4>(0); // does not compile
            resolver.InsertType(0, typeof(Resolved4)); // throws
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ManyResolverCannotInsertTypeAtWrongIndex()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>(), new Type[] { typeof(Resolved1), typeof(Resolved2) });
            resolver.InsertType(99, typeof(Resolved3)); // throws
        }

        //

        [Test]
        public void ManyResolverCanInsertBeforeTypeBeforeFreeze()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>(), new Type[] { typeof(Resolved1), typeof(Resolved2) });
            resolver.InsertTypeBefore<Resolved2, Resolved3>();

            Assert.IsTrue(resolver.ContainsType<Resolved1>());
            Assert.IsTrue(resolver.ContainsType<Resolved2>());
            Assert.IsTrue(resolver.ContainsType<Resolved3>());
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ManyResolverCannotInsertBeforeTypeOnceFrozen()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>(), new Type[] { typeof(Resolved1), typeof(Resolved2) });
            Resolution.Freeze();
            resolver.InsertTypeBefore<Resolved2, Resolved3>();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ManyResolverCannotInsertBeforeTypeAgain()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>(), new Type[] { typeof(Resolved1), typeof(Resolved2) });
            resolver.InsertTypeBefore<Resolved2, Resolved1>();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ManyResolverCannotInsertBeforeAbsentType()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>(), new Type[] { typeof(Resolved1) });
            resolver.InsertTypeBefore<Resolved2, Resolved3>();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ManyResolverCannotInsertBeforeInvalidType()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>(), new Type[] { typeof(Resolved1), typeof(Resolved2) });
            //resolver.InsertTypeBefore<Resolved2, Resolved4>(); // does not compile
            resolver.InsertTypeBefore(typeof(Resolved2), typeof(Resolved4)); // throws
        }

        #endregion

        #region Test ManyResolver resolution

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ManyResolverCannotGetValuesBeforeFreeze()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>(), new Type[] { typeof(Resolved1), typeof(Resolved2) });
            var values = resolver.ResolvedObjects;
        }

        [Test]
        public void ManyResolverCannotGetValuesBeforeFreezeUnless()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>(), new Type[] { typeof(Resolved1), typeof(Resolved2) });
            resolver.CanResolveBeforeFrozen = true;
            var values = resolver.ResolvedObjects;
        }

        [Test]
        public void ManyResolverCanGetValuesOnceFrozen()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>(), new Type[] { typeof(Resolved1), typeof(Resolved2) });
            Resolution.Freeze();
            var values = resolver.ResolvedObjects;

            Assert.AreEqual(2, values.Count());
            Assert.IsInstanceOf<Resolved1>(values.ElementAt(0));
            Assert.IsInstanceOf<Resolved2>(values.ElementAt(1));
        }

        [Test]
        public void ManyResolverDefaultLifetimeScopeIsApplication()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>(), new Type[] { typeof(Resolved1), typeof(Resolved2) });
            Resolution.Freeze();
            var values = resolver.ResolvedObjects;

            Assert.AreEqual(2, values.Count());
            Assert.IsInstanceOf<Resolved1>(values.ElementAt(0));
            Assert.IsInstanceOf<Resolved2>(values.ElementAt(1));

            var values2 = resolver.ResolvedObjects;
            Assert.AreEqual(2, values2.Count());
            Assert.AreSame(values.ElementAt(0), values2.ElementAt(0));
            Assert.AreSame(values.ElementAt(1), values2.ElementAt(1));
        }

        [Test]
        public void ManyResolverTransientLifetimeScope()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>(), new Type[] { typeof(Resolved1), typeof(Resolved2) }, ObjectLifetimeScope.Transient);
            Resolution.Freeze();
            var values = resolver.ResolvedObjects;

            Assert.AreEqual(2, values.Count());
            Assert.IsInstanceOf<Resolved1>(values.ElementAt(0));
            Assert.IsInstanceOf<Resolved2>(values.ElementAt(1));

            var values2 = resolver.ResolvedObjects;
            Assert.AreEqual(2, values2.Count());
            Assert.AreNotSame(values.ElementAt(0), values2.ElementAt(0));
            Assert.AreNotSame(values.ElementAt(1), values2.ElementAt(1));
        }

        [Test]
        public void ManyResolverDefaultOrderOfTypes()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>());
            resolver.AddType<Resolved3>();
            resolver.InsertType<Resolved1>(0);
            resolver.InsertTypeBefore<Resolved3, Resolved2>();
            Resolution.Freeze();
            var values = resolver.ResolvedObjects;

            Assert.AreEqual(3, values.Count());
            Assert.IsInstanceOf<Resolved1>(values.ElementAt(0));
            Assert.IsInstanceOf<Resolved2>(values.ElementAt(1));
            Assert.IsInstanceOf<Resolved3>(values.ElementAt(2));
        }

        [Test]
        public void ManyResolverHttpRequestLifetimeScope()
        {
            var httpContextFactory = new FakeHttpContextFactory("~/Home");
            var httpContext = httpContextFactory.HttpContext;
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>(), httpContext);

            resolver.AddType<Resolved1>();
            resolver.AddType<Resolved2>();
            Resolution.Freeze();

            var values = resolver.ResolvedObjects;
            Assert.AreEqual(2, values.Count());
            Assert.IsInstanceOf<Resolved1>(values.ElementAt(0));
            Assert.IsInstanceOf<Resolved2>(values.ElementAt(1));

            var values2 = resolver.ResolvedObjects;
            Assert.AreEqual(2, values2.Count());
            Assert.AreSame(values.ElementAt(0), values2.ElementAt(0));
            Assert.AreSame(values.ElementAt(1), values2.ElementAt(1));

            httpContextFactory.HttpContext.Items.Clear(); // new context

            var values3 = resolver.ResolvedObjects;
            Assert.AreEqual(2, values3.Count());
            Assert.AreNotSame(values.ElementAt(0), values3.ElementAt(0));
            Assert.AreNotSame(values.ElementAt(1), values3.ElementAt(1));
        }

        [Test]
        public void ManyResolverWeightedResolution()
        {
            var resolver = new ManyResolver(new ActivatorServiceProvider(), Mock.Of<ILogger>(), new Type[] { typeof(Resolved1), typeof(Resolved2) });
            Resolution.Freeze();

            var values = resolver.SortedResolvedObjects;
            Assert.AreEqual(2, values.Count());
            Assert.IsInstanceOf<Resolved2>(values.ElementAt(0));
            Assert.IsInstanceOf<Resolved1>(values.ElementAt(1));
        }

        #endregion
    }
}
