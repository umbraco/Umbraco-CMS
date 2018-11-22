using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Umbraco.Core;
using Umbraco.Core.ObjectResolution;
using NUnit.Framework;

namespace Umbraco.Tests.Resolvers
{
    [TestFixture]
    public class SingleResolverTests
    {
        [SetUp]
        public void Setup()
        {
            SingleResolver.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            SingleResolver.Reset();
        }

        #region Resolvers and Resolved

        public class Resolved
        { }

        public sealed class SingleResolver : SingleObjectResolverBase<SingleResolver, Resolved>
        {
            public SingleResolver()
                : base()
            { }

            public SingleResolver(bool canBeNull)
                : base(canBeNull)
            { }

            public SingleResolver(Resolved value)
                : base(value)
            { }

            public SingleResolver(Resolved value, bool canBeNull)
                : base(value, canBeNull)
            { }

            public Resolved Resolved { get { return Value; } set { Value = value; } }
        }

        #endregion

        #region Test SingleResolver

        [Test]
        public void SingleResolverCanSetValueBeforeFreeze()
        {
            var resolver = new SingleResolver();
            resolver.Resolved = new Resolved();
        }

        [Test] // is this normal?
        public void SingleResolverCanInitializeWithNullValueEvenIfCannotBeNull()
        {
            var resolver = new SingleResolver(null, false);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SingleResolverCannotSetValueToNullIfCannotBeNull()
        {
            var resolver = new SingleResolver();
            resolver.Resolved = null; // throws
        }

        [Test]
        public void SingleResolverCanSetValueToNullIfCanBeNull()
        {
            var resolver = new SingleResolver(true);
            resolver.Resolved = null;
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SingleResolverCannotSetValueOnceFrozen()
        {
            var resolver = new SingleResolver();
            Resolution.Freeze();
            resolver.Resolved = new Resolved(); // throws
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SingleResolverCannotGetValueBeforeFreeze()
        {
            var resolver = new SingleResolver();
            resolver.Resolved = new Resolved();
            var resolved = resolver.Resolved; // throws
        }

        [Test]
        public void SingleResolverCanGetValueOnceFrozen()
        {
            var resolver = new SingleResolver();
            var resolved = resolver.Resolved = new Resolved();
            Resolution.Freeze();
            Assert.AreSame(resolved, resolver.Resolved);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SingleResolverCannotGetNullValueIfCannotBeNull()
        {
            var resolver = new SingleResolver();
            Resolution.Freeze();
            var resolved = resolver.Resolved; // throws
        }

        [Test]
        public void SingleResolverCanGetNullValueIfCanBeNull()
        {
            var resolver = new SingleResolver(true);
            Resolution.Freeze();
            Assert.IsNull(resolver.Resolved);
        }

        #endregion
    }
}
