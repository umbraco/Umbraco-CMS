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
    public class ResolutionTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void TearDown()
        {
            BaseResolver.Reset();
            BaseResolver2.Reset();
            BaseResolver3.Reset();
        }

        #region Resolvers and Resolved

        class BaseResolver : ResolverBase<BaseResolver>
        { }

        class BaseResolver2 : ResolverBase<BaseResolver2>
        { }

        class BaseResolver3 : ResolverBase<BaseResolver3>
        { }

        #endregion

        #region Test Resolution

        [Test]
        public void ResolutionCanFreeze()
        {
            Resolution.Freeze();
            Assert.IsTrue(Resolution.IsFrozen);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResolutionCannotFreezeAgain()
        {
            Resolution.Freeze();
            Resolution.Freeze(); // throws
        }

        [Test]
        public void ResolutionCanReset()
        {
            Resolution.Freeze();
            Resolution.Reset();
            Assert.IsFalse(Resolution.IsFrozen);
        }

        [Test]
        public void ResolutionCanConfigureBeforeFreeze()
        {
            using (Resolution.Configuration)
            { }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResolutionCannotConfigureOnceFrozen()
        {
            Resolution.Freeze();

            using (Resolution.Configuration) // throws
            { }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResolutionCanDetectIfNotFrozen()
        {
            using (Resolution.Reader()) // throws
            {}
        }

        [Test]
        public void ResolutionCanEnsureIsFrozen()
        {
            Resolution.Freeze();
            using (Resolution.Reader()) // ok
            {}
        }

        [Test]
        public void ResolutionFrozenEventTriggers()
        {
            int count = 0;
            object copySender = null;
            EventArgs copyArgs = null;
            Resolution.Frozen += (sender, args) =>
            {
                copySender = sender;
                copyArgs = args;
                count++;
            };
            Assert.AreEqual(0, count);
            Resolution.Freeze();

            Assert.AreEqual(1, count);
            Assert.IsNull(copySender);
            Assert.IsNull(copyArgs);
        }

        [Test]
        public void ResolutionResetClearsFrozenEvent()
        {
            int count = 0;
            Resolution.Frozen += (sender, args) =>
            {
                count++;
            };
            Resolution.Freeze();
            Assert.AreEqual(1, count);

            Resolution.Reset();

            Resolution.Freeze();
            Assert.AreEqual(1, count);
        }

        #endregion

        #region Test ResolverBase

        // unused

        [Test]
        public void BaseResolverCurrentCanBeNullOnFreeze()
        {
            // does not throw, even though BaseResolver.Current is not initialized
            Resolution.Freeze();
        }

        // set

        [Test]
        public void BaseResolverCurrentCanSetBeforeFreeze()
        {
            BaseResolver.Current = new BaseResolver();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BaseResolverCurrentCannotSetToNull()
        {
            BaseResolver.Current = null; // throws
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BaseResolverCurrentCannotSetAgain()
        {
            BaseResolver.Current = new BaseResolver();

            // cannot set again
            BaseResolver.Current = new BaseResolver(); // throws
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BaseResolverCurrentCannotSetOnceFrozen()
        {
            Resolution.Freeze();

            // cannot set once frozen
            BaseResolver.Current = new BaseResolver(); // throws
        }

        // get

        // ignore: see BaseResolverCurrentCanGetBeforeFreeze
        //[Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BaseResolverCurrentCannotGetBeforeFreeze()
        {
            BaseResolver.Current = new BaseResolver();

            // cannot get before freeze
            var resolver = BaseResolver.Current; // throws
        }

        [Test]
        public void BaseResolverCurrentCanGetBeforeFreeze()
        {
            BaseResolver.Current = new BaseResolver();
            var resolver = BaseResolver.Current;
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BaseResolverCurrentCannotGetIfNotSet()
        {
            Resolution.Freeze();

            // cannot get before freeze
            var resolver = BaseResolver.Current; // throws
        }

        [Test]
        public void BaseResolverCurrentCanGetOnceFrozen()
        {
            BaseResolver.Current = new BaseResolver();
            Resolution.Freeze();
            var resolver = BaseResolver.Current;
        }

        #endregion

        [Test]
        public void Resolver_Collection_Is_Updated()
        {
            BaseResolver.Current = new BaseResolver();
            BaseResolver2.Current = new BaseResolver2();
            BaseResolver3.Current = new BaseResolver3();
            Assert.AreEqual(3, ResolverCollection.Count);
        }

        [Test]
        public void Resolver_Collection_Is_Reset()
        {
            BaseResolver.Current = new BaseResolver();
            BaseResolver2.Current = new BaseResolver2();
            BaseResolver3.Current = new BaseResolver3();
            
            ResolverCollection.ResetAll();

            Assert.AreEqual(0, ResolverCollection.Count);
            Assert.Throws<InvalidOperationException>(() =>
                {
                    var c = BaseResolver.Current;
                });
            Assert.Throws<InvalidOperationException>(() =>
            {
                var c = BaseResolver2.Current;
            });
            Assert.Throws<InvalidOperationException>(() =>
            {
                var c = BaseResolver3.Current;
            });

            //this should not error!
            BaseResolver.Current = new BaseResolver();
            BaseResolver2.Current = new BaseResolver2();
            BaseResolver3.Current = new BaseResolver3();

            Assert.Pass();
        }
    }
}
