using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence;
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

        [TestCase(EventsDispatchMode.PassThrough, true, true)]
        [TestCase(EventsDispatchMode.PassThrough, true, false)]
        [TestCase(EventsDispatchMode.PassThrough, false, true)]
        [TestCase(EventsDispatchMode.PassThrough, false, false)]
        public void PassThroughCancelable(EventsDispatchMode mode, bool cancel, bool complete)
        {
            var counter1 = 0;
            var counter2 = 0;

            DoThing1 += (sender, args) => { counter1++; if (cancel) args.Cancel = true; };
            DoThing2 += (sender, args) => { counter2++; };

            var scopeProvider = new ScopeProvider(Mock.Of<IDatabaseFactory2>());
            using (var scope = scopeProvider.CreateScope(dispatchMode: mode))
            {
                var cancelled = scope.Events.DispatchCancelable(DoThing1, this, new SaveEventArgs<string>("test"));
                if (cancelled == false)
                    scope.Events.Dispatch(DoThing2, this, new SaveEventArgs<int>(0));
                if (complete)
                    scope.Complete();
            }

            var expected1 = mode == EventsDispatchMode.Passive ? 0 : 1;
            Assert.AreEqual(expected1, counter1);

            var expected2 = -1;
            switch (mode)
            {
                case EventsDispatchMode.PassThrough:
                    expected2 = cancel ? 0 : 1;
                    break;
                case EventsDispatchMode.Scope:
                    expected2 = cancel ? 0 : (complete ? 1 : 0);
                    break;
                case EventsDispatchMode.Passive:
                    expected2 = 0;
                    break;
            }
            Assert.AreEqual(expected2, counter2);
        }

        [Test]
        public void QueueEvents()
        {
            DoThing1 += OnDoThingFail;
            DoThing2 += OnDoThingFail;
            DoThing3 += OnDoThingFail;

            var scopeProvider = new ScopeProvider(Mock.Of<IDatabaseFactory2>());
            using (var scope = scopeProvider.CreateScope(dispatchMode: EventsDispatchMode.Passive))
            {
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
        public void EventsDispatchmode_PassThrough(bool complete)
        {
            var counter = 0;

            DoThing1 += (sender, args) => { counter++; };
            DoThing2 += (sender, args) => { counter++; };
            DoThing3 += (sender, args) => { counter++; };

            var scopeProvider = new ScopeProvider(Mock.Of<IDatabaseFactory2>());
            using (var scope = scopeProvider.CreateScope(dispatchMode: EventsDispatchMode.PassThrough))
            {
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
        public void EventsDispatchMode_Passive(bool complete)
        {
            DoThing1 += OnDoThingFail;
            DoThing2 += OnDoThingFail;
            DoThing3 += OnDoThingFail;

            var scopeProvider = new ScopeProvider(Mock.Of<IDatabaseFactory2>());
            using (var scope = scopeProvider.CreateScope(dispatchMode: EventsDispatchMode.Passive))
            {
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
        public void EventsDispatchMode_Scope(bool complete)
        {
            var counter = 0;
            IScope ambientScope = null;
            ScopeContext ambientContext = null;
            Guid value = Guid.Empty;

            var scopeProvider = new ScopeProvider(Mock.Of<IDatabaseFactory2>());

            DoThing1 += (sender, args) => { counter++; };
            DoThing2 += (sender, args) => { counter++; };
            DoThing3 += (sender, args) =>
            {
                ambientScope = scopeProvider.AmbientScope;
                ambientContext = scopeProvider.AmbientContext;
                value = scopeProvider.Context.Enlist("value", Guid.NewGuid, (c, o) => { });
                counter++;
            };

            Guid guid;
            using (var scope = scopeProvider.CreateScope(dispatchMode: EventsDispatchMode.Scope))
            {
                Assert.IsNotNull(scopeProvider.AmbientContext);
                guid = scopeProvider.Context.Enlist("value", Guid.NewGuid, (c, o) => { });

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
                Assert.IsNull(ambientScope); // scope was gone
                Assert.IsNotNull(ambientContext); // but not context
                Assert.AreEqual(guid, value); // so we got the same value!
            }
            else
            {
                // else, no event has been raised
                Assert.AreEqual(0, counter);
            }

            // everything's gone
            Assert.IsNull(scopeProvider.AmbientScope);
            Assert.IsNull(scopeProvider.AmbientContext);
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