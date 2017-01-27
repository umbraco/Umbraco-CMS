using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Scoping;

namespace Umbraco.Tests.Scoping
{

    [TestFixture]
    public class ScopeEventDispatcherTests
    {
        [SetUp]
        public void Setup()
        {
            // remove all handlers first
            DoThing1 = null;
            DoThing2 = null;
            DoThing3 = null;
        }

        [Test]
        public void Does_Not_Support_Event_Cancellation()
        {
            // fixme - we... should not have this at all

            var provider = new PetaPocoUnitOfWorkProvider(new ScopeProvider(Mock.Of<IDatabaseFactory2>()));
            using (var uow = provider.GetUnitOfWork())
            {
                Assert.IsFalse(uow.Events.SupportsEventCancellation);
            }
        }

        [Test]
        public void Can_Get_Event_Info()
        {
            DoThing1 += OnDoThingFail;
            DoThing2 += OnDoThingFail;
            DoThing3 += OnDoThingFail;

            var scopeProvider = new ScopeProvider(Mock.Of<IDatabaseFactory2>());
            using (var scope = scopeProvider.CreateScope())
            {
                Assert.IsInstanceOf<ScopeEventDispatcher>(scope.Events);
                var dispatcher = (ScopeEventDispatcher)scope.Events;

                dispatcher.PassThrough = false;
                dispatcher.RaiseEvents = false;

                scope.Events.Dispatch(DoThing1, this, new SaveEventArgs<string>("test"));
                scope.Events.Dispatch(DoThing2, this, new SaveEventArgs<int>(0));
                scope.Events.Dispatch(DoThing3, this, new SaveEventArgs<decimal>(0));

                // events have been queued
                Assert.AreEqual(3, scope.Events.GetEvents().Count());

                var events = scope.Events.GetEvents().ToArray();

                var knownNames = new[] { "DoThing1", "DoThing2", "DoThing3" };
                var knownArgTypes = new[] { typeof (SaveEventArgs<string>), typeof (SaveEventArgs<int>), typeof (SaveEventArgs<decimal>) };

                for (var i = 0; i < events.Length; i++)
                {
                    Assert.AreEqual(knownNames[i], events[i].EventName);
                    Assert.AreEqual(knownArgTypes[i], events[i].Args.GetType());
                }
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TriggerEvents(bool complete)
        {
            var counter = 0;

            DoThing1 += (sender, args) => { counter++; };
            DoThing2 += (sender, args) => { counter++; };
            DoThing3 += (sender, args) => { counter++; };

            var scopeProvider = new ScopeProvider(Mock.Of<IDatabaseFactory2>());
            using (var scope = scopeProvider.CreateScope())
            {
                Assert.IsInstanceOf<ScopeEventDispatcher>(scope.Events);
                var dispatcher = (ScopeEventDispatcher)scope.Events;

                dispatcher.PassThrough = true;
                dispatcher.RaiseEvents = true;

                scope.Events.Dispatch(DoThing1, this, new SaveEventArgs<string>("test"));
                scope.Events.Dispatch(DoThing2, this, new SaveEventArgs<int>(0));
                scope.Events.Dispatch(DoThing3, this, new SaveEventArgs<decimal>(0));

                // events have not been queued
                Assert.IsEmpty(scope.Events.GetEvents());

                // events have been raised
                Assert.AreEqual(3, counter);

                if (complete)
                    scope.Complete();
            }

            // nothing has changed
            Assert.AreEqual(3, counter);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void QueueAndDiscardEvents(bool complete)
        {
            DoThing1 += OnDoThingFail;
            DoThing2 += OnDoThingFail;
            DoThing3 += OnDoThingFail;

            var scopeProvider = new ScopeProvider(Mock.Of<IDatabaseFactory2>());
            using (var scope = scopeProvider.CreateScope())
            {
                Assert.IsInstanceOf<ScopeEventDispatcher>(scope.Events);
                var dispatcher = (ScopeEventDispatcher) scope.Events;

                dispatcher.PassThrough = false;
                dispatcher.RaiseEvents = false;

                scope.Events.Dispatch(DoThing1, this, new SaveEventArgs<string>("test"));
                scope.Events.Dispatch(DoThing2, this, new SaveEventArgs<int>(0));
                scope.Events.Dispatch(DoThing3, this, new SaveEventArgs<decimal>(0));

                // events have been queued
                Assert.AreEqual(3, scope.Events.GetEvents().Count());

                if (complete)
                    scope.Complete();
            }

            // no event has been raised (else OnDoThingFail would have failed)
        }

        [TestCase(true)]
        [TestCase(false)]
        public void QueueAndRaiseEvents(bool complete)
        {
            var counter = 0;

            DoThing1 += (sender, args) => { counter++; };
            DoThing2 += (sender, args) => { counter++; };
            DoThing3 += (sender, args) => { counter++; };

            var scopeProvider = new ScopeProvider(Mock.Of<IDatabaseFactory2>());
            using (var scope = scopeProvider.CreateScope())
            {
                Assert.IsInstanceOf<ScopeEventDispatcher>(scope.Events);
                var dispatcher = (ScopeEventDispatcher)scope.Events;

                dispatcher.PassThrough = false;
                dispatcher.RaiseEvents = true;

                scope.Events.Dispatch(DoThing1, this, new SaveEventArgs<string>("test"));
                scope.Events.Dispatch(DoThing2, this, new SaveEventArgs<int>(0));
                scope.Events.Dispatch(DoThing3, this, new SaveEventArgs<decimal>(0));

                // events have been queued
                Assert.AreEqual(3, scope.Events.GetEvents().Count());

                if (complete)
                    scope.Complete();
            }

            if (complete)
            {
                // events have been raised
                Assert.AreEqual(3, counter);
            }
            else
            {
                // else, no event has been raised
                // fixme - fails at the moment because ... we always trigger events?
                Assert.AreEqual(0, counter);
            }
        }

        private static void OnDoThingFail(object sender, EventArgs eventArgs)
        {
            Assert.Fail();
        }

        public static event EventHandler<SaveEventArgs<string>> DoThing1;

        public static event EventHandler<SaveEventArgs<int>> DoThing2;

        public static event TypedEventHandler<ScopeEventDispatcherTests, SaveEventArgs<decimal>> DoThing3;
    }    
}