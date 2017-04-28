using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Scoping
{
    [TestFixture]
    public class ScopeEventDispatcherTests : BaseUmbracoConfigurationTest
    {
        [SetUp]
        public void Setup()
        {
            // remove all handlers first
            DoThing1 = null;
            DoThing2 = null;
            DoThing3 = null;
        }

        [TestCase(false, true, true)]
        [TestCase(false, true, false)]
        [TestCase(false, false, true)]
        [TestCase(false, false, false)]
        [TestCase(true, true, true)]
        [TestCase(true, true, false)]
        [TestCase(true, false, true)]
        [TestCase(true, false, false)]
        public void EventsHandling(bool passive, bool cancel, bool complete)
        {
            var counter1 = 0;
            var counter2 = 0;

            DoThing1 += (sender, args) => { counter1++; if (cancel) args.Cancel = true; };
            DoThing2 += (sender, args) => { counter2++; };

            var scopeProvider = new ScopeProvider(Mock.Of<IDatabaseFactory2>());
            using (var scope = scopeProvider.CreateScope(eventDispatcher: passive ? new PassiveEventDispatcher() : null))
            {
                var cancelled = scope.Events.DispatchCancelable(DoThing1, this, new SaveEventArgs<string>("test"));
                if (cancelled == false)
                    scope.Events.Dispatch(DoThing2, this, new SaveEventArgs<int>(0));
                if (complete)
                    scope.Complete();
            }

            var expected1 = passive ? 0 : 1;
            Assert.AreEqual(expected1, counter1);

            int expected2;
            if (passive)
                expected2 = 0;
            else
                expected2 = cancel ? 0 : (complete ? 1 : 0);

            Assert.AreEqual(expected2, counter2);
        }

        [Test]
        public void QueueEvents()
        {
            DoThing1 += OnDoThingFail;
            DoThing2 += OnDoThingFail;
            DoThing3 += OnDoThingFail;

            var scopeProvider = new ScopeProvider(Mock.Of<IDatabaseFactory2>());
            using (var scope = scopeProvider.CreateScope(eventDispatcher: new PassiveEventDispatcher()))
            {
                scope.Events.Dispatch(DoThing1, this, new SaveEventArgs<string>("test"));
                scope.Events.Dispatch(DoThing2, this, new SaveEventArgs<int>(0));
                scope.Events.Dispatch(DoThing3, this, new SaveEventArgs<decimal>(0));

                // events have been queued
                Assert.AreEqual(3, scope.Events.GetEvents(EventDefinitionFilter.All).Count());

                var events = scope.Events.GetEvents(EventDefinitionFilter.All).ToArray();

                var knownNames = new[] { "DoThing1", "DoThing2", "DoThing3" };
                var knownArgTypes = new[] { typeof(SaveEventArgs<string>), typeof(SaveEventArgs<int>), typeof(SaveEventArgs<decimal>) };

                for (var i = 0; i < events.Length; i++)
                {
                    Assert.AreEqual(knownNames[i], events[i].EventName);
                    Assert.AreEqual(knownArgTypes[i], events[i].Args.GetType());
                }
            }
        }

        /// <summary>
        /// This will test that when we track events that before we Get the events we normalize all of the
        /// event entities to be the latest one (most current) found amongst the event so that there is 
        /// no 'stale' entities in any of the args
        /// </summary>
        [Test]
        public void LatestEntities()
        {
            DoThingForContent += OnDoThingFail;

            var now = DateTime.Now;
            var contentType = MockedContentTypes.CreateBasicContentType();
            var content1 = MockedContent.CreateBasicContent(contentType);
            content1.Id = 123;
            content1.UpdateDate = now.AddMinutes(1);
            var content2 = MockedContent.CreateBasicContent(contentType);
            content2.Id = 123;
            content1.UpdateDate = now.AddMinutes(2);
            var content3 = MockedContent.CreateBasicContent(contentType);
            content3.Id = 123;
            content1.UpdateDate = now.AddMinutes(3);

            var scopeProvider = new ScopeProvider(Mock.Of<IDatabaseFactory2>());
            using (var scope = scopeProvider.CreateScope(eventDispatcher: new PassiveEventDispatcher()))
            {

                scope.Events.Dispatch(DoThingForContent, this, new SaveEventArgs<IContent>(content1));
                scope.Events.Dispatch(DoThingForContent, this, new SaveEventArgs<IContent>(content2));
                scope.Events.Dispatch(DoThingForContent, this, new SaveEventArgs<IContent>(content3));

                // events have been queued
                var events = scope.Events.GetEvents(EventDefinitionFilter.All).ToArray();
                Assert.AreEqual(3, events.Length);
                
                foreach (var t in events)
                {
                    var args = (SaveEventArgs<IContent>)t.Args;
                    foreach (var entity in args.SavedEntities)
                    {
                        Assert.AreEqual(content3, entity);
                    }
                }
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EventsDispatching_Passive(bool complete)
        {
            DoThing1 += OnDoThingFail;
            DoThing2 += OnDoThingFail;
            DoThing3 += OnDoThingFail;

            var scopeProvider = new ScopeProvider(Mock.Of<IDatabaseFactory2>());
            using (var scope = scopeProvider.CreateScope(eventDispatcher: new PassiveEventDispatcher()))
            {
                scope.Events.Dispatch(DoThing1, this, new SaveEventArgs<string>("test"));
                scope.Events.Dispatch(DoThing2, this, new SaveEventArgs<int>(0));
                scope.Events.Dispatch(DoThing3, this, new SaveEventArgs<decimal>(0));

                // events have been queued
                Assert.AreEqual(3, scope.Events.GetEvents(EventDefinitionFilter.All).Count());

                if (complete)
                    scope.Complete();
            }

            // no event has been raised (else OnDoThingFail would have failed)
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EventsDispatching_Scope(bool complete)
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
            using (var scope = scopeProvider.CreateScope())
            {
                Assert.IsNotNull(scopeProvider.AmbientContext);
                guid = scopeProvider.Context.Enlist("value", Guid.NewGuid, (c, o) => { });

                scope.Events.Dispatch(DoThing1, this, new SaveEventArgs<string>("test"));
                scope.Events.Dispatch(DoThing2, this, new SaveEventArgs<int>(0));
                scope.Events.Dispatch(DoThing3, this, new SaveEventArgs<decimal>(0));

                // events have been queued
                Assert.AreEqual(3, scope.Events.GetEvents(EventDefinitionFilter.All).Count());
                Assert.AreEqual(0, counter);

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

        public static event EventHandler<SaveEventArgs<IContent>> DoThingForContent;

        public static event EventHandler<SaveEventArgs<string>> DoThing1;

        public static event EventHandler<SaveEventArgs<int>> DoThing2;

        public static event TypedEventHandler<ScopeEventDispatcherTests, SaveEventArgs<decimal>> DoThing3;

        public class PassiveEventDispatcher : ScopeEventDispatcherBase
        {
            public PassiveEventDispatcher()
                : base(false)
            { }

            protected override void ScopeExitCompleted()
            {
                // do nothing
            }
        }
    }
}