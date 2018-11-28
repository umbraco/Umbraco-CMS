using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Components;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Services;

namespace Umbraco.Tests.Scoping
{
    [TestFixture]
    public class ScopeEventDispatcherTests //: BaseUmbracoConfigurationTest
    {
        private TestObjects _testObjects;

        [SetUp]
        public void Setup()
        {
            // remove all handlers first
            DoThing1 = null;
            DoThing2 = null;
            DoThing3 = null;

            var register = RegisterFactory.Create();

            var composition = new Composition(register, new TypeLoader(), Mock.Of<IProfilingLogger>(), RuntimeLevel.Run);

            _testObjects = new TestObjects(register);

            register.RegisterSingleton(factory => new FileSystems(factory, factory.TryGetInstance<ILogger>()));
            composition.GetCollectionBuilder<MapperCollectionBuilder>();

            Current.Factory = register.CreateFactory();

            SettingsForTests.Reset(); // ensure we have configuration
        }

        [TearDown]
        public void TearDown()
        {
            Current.Reset();

            SettingsForTests.Reset();
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

            var scopeProvider = _testObjects.GetScopeProvider(Mock.Of<ILogger>());
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

            var scopeProvider = _testObjects.GetScopeProvider(Mock.Of<ILogger>());
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

        [Test]
        public void SupersededEvents()
        {
            DoSaveForContent += OnDoThingFail;
            DoDeleteForContent += OnDoThingFail;
            DoForTestArgs += OnDoThingFail;
            DoForTestArgs2 += OnDoThingFail;

            var contentType = MockedContentTypes.CreateBasicContentType();

            var content1 = MockedContent.CreateBasicContent(contentType);
            content1.Id = 123;

            var content2 = MockedContent.CreateBasicContent(contentType);
            content2.Id = 456;

            var content3 = MockedContent.CreateBasicContent(contentType);
            content3.Id = 789;

            var scopeProvider = _testObjects.GetScopeProvider(Mock.Of<ILogger>());
            using (var scope = scopeProvider.CreateScope(eventDispatcher: new PassiveEventDispatcher()))
            {

                //content1 will be filtered from the args
                scope.Events.Dispatch(DoSaveForContent, this, new SaveEventArgs<IContent>(new[]{ content1 , content3}));
                scope.Events.Dispatch(DoDeleteForContent, this, new DeleteEventArgs<IContent>(content1), "DoDeleteForContent");
                scope.Events.Dispatch(DoSaveForContent, this, new SaveEventArgs<IContent>(content2));
                //this entire event will be filtered
                scope.Events.Dispatch(DoForTestArgs, this, new TestEventArgs(content1));
                scope.Events.Dispatch(DoForTestArgs2, this, new TestEventArgs2(content1));

                // events have been queued
                var events = scope.Events.GetEvents(EventDefinitionFilter.All).ToArray();
                Assert.AreEqual(4, events.Length);

                Assert.AreEqual(typeof(SaveEventArgs<IContent>), events[0].Args.GetType());
                Assert.AreEqual(1, ((SaveEventArgs<IContent>)events[0].Args).SavedEntities.Count());
                Assert.AreEqual(content3.Id, ((SaveEventArgs<IContent>)events[0].Args).SavedEntities.First().Id);

                Assert.AreEqual(typeof(DeleteEventArgs<IContent>), events[1].Args.GetType());
                Assert.AreEqual(content1.Id, ((DeleteEventArgs<IContent>) events[1].Args).DeletedEntities.First().Id);

                Assert.AreEqual(typeof(SaveEventArgs<IContent>), events[2].Args.GetType());
                Assert.AreEqual(content2.Id, ((SaveEventArgs<IContent>)events[2].Args).SavedEntities.First().Id);

                Assert.AreEqual(typeof(TestEventArgs2), events[3].Args.GetType());
            }
        }

        [Test]
        public void SupersededEvents2()
        {
            Test_Unpublished += OnDoThingFail;
            Test_Deleted += OnDoThingFail;

            var contentService = Mock.Of<IContentService>();
            var content = Mock.Of<IContent>();

            var scopeProvider = _testObjects.GetScopeProvider(Mock.Of<ILogger>());
            using (var scope = scopeProvider.CreateScope(eventDispatcher: new PassiveEventDispatcher()))
            {
                scope.Events.Dispatch(Test_Unpublished, contentService, new PublishEventArgs<IContent>(new [] { content }), "Unpublished");
                scope.Events.Dispatch(Test_Deleted, contentService, new DeleteEventArgs<IContent>(new [] { content }), "Deleted");

                // see U4-10764
                var events = scope.Events.GetEvents(EventDefinitionFilter.All).ToArray();
                Assert.AreEqual(2, events.Length);
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
            DoSaveForContent += OnDoThingFail;

            var now = DateTime.Now;
            var contentType = MockedContentTypes.CreateBasicContentType();
            var content1 = MockedContent.CreateBasicContent(contentType);
            content1.Id = 123;
            content1.UpdateDate = now.AddMinutes(1);
            var content2 = MockedContent.CreateBasicContent(contentType);
            content2.Id = 123;
            content2.UpdateDate = now.AddMinutes(2);
            var content3 = MockedContent.CreateBasicContent(contentType);
            content3.Id = 123;
            content3.UpdateDate = now.AddMinutes(3);

            var scopeProvider = _testObjects.GetScopeProvider(Mock.Of<ILogger>());
            using (var scope = scopeProvider.CreateScope(eventDispatcher: new PassiveEventDispatcher()))
            {
                scope.Events.Dispatch(DoSaveForContent, this, new SaveEventArgs<IContent>(content1));
                scope.Events.Dispatch(DoSaveForContent, this, new SaveEventArgs<IContent>(content2));
                scope.Events.Dispatch(DoSaveForContent, this, new SaveEventArgs<IContent>(content3));

                // events have been queued
                var events = scope.Events.GetEvents(EventDefinitionFilter.All).ToArray();
                Assert.AreEqual(3, events.Length);

                foreach (var t in events)
                {
                    var args = (SaveEventArgs<IContent>)t.Args;
                    foreach (var entity in args.SavedEntities)
                    {
                        Assert.AreEqual(content3, entity);
                        Assert.IsTrue(object.ReferenceEquals(content3, entity));
                    }
                }
            }
        }

        [Test]
        public void FirstIn()
        {
            DoSaveForContent += OnDoThingFail;

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

            var scopeProvider = _testObjects.GetScopeProvider(Mock.Of<ILogger>());
            using (var scope = scopeProvider.CreateScope(eventDispatcher: new PassiveEventDispatcher()))
            {
                scope.Events.Dispatch(DoSaveForContent, this, new SaveEventArgs<IContent>(content1));
                scope.Events.Dispatch(DoSaveForContent, this, new SaveEventArgs<IContent>(content2));
                scope.Events.Dispatch(DoSaveForContent, this, new SaveEventArgs<IContent>(content3));

                // events have been queued
                var events = scope.Events.GetEvents(EventDefinitionFilter.FirstIn).ToArray();
                Assert.AreEqual(1, events.Length);
                Assert.AreEqual(content1, ((SaveEventArgs<IContent>) events[0].Args).SavedEntities.First());
                Assert.IsTrue(object.ReferenceEquals(content1, ((SaveEventArgs<IContent>)events[0].Args).SavedEntities.First()));
                Assert.AreEqual(content1.UpdateDate, ((SaveEventArgs<IContent>) events[0].Args).SavedEntities.First().UpdateDate);
            }
        }

        [Test]
        public void LastIn()
        {
            DoSaveForContent += OnDoThingFail;

            var now = DateTime.Now;
            var contentType = MockedContentTypes.CreateBasicContentType();
            var content1 = MockedContent.CreateBasicContent(contentType);
            content1.Id = 123;
            content1.UpdateDate = now.AddMinutes(1);
            var content2 = MockedContent.CreateBasicContent(contentType);
            content2.Id = 123;
            content2.UpdateDate = now.AddMinutes(2);
            var content3 = MockedContent.CreateBasicContent(contentType);
            content3.Id = 123;
            content3.UpdateDate = now.AddMinutes(3);

            var scopeProvider = _testObjects.GetScopeProvider(Mock.Of<ILogger>());
            using (var scope = scopeProvider.CreateScope(eventDispatcher: new PassiveEventDispatcher()))
            {
                scope.Events.Dispatch(DoSaveForContent, this, new SaveEventArgs<IContent>(content1));
                scope.Events.Dispatch(DoSaveForContent, this, new SaveEventArgs<IContent>(content2));
                scope.Events.Dispatch(DoSaveForContent, this, new SaveEventArgs<IContent>(content3));

                // events have been queued
                var events = scope.Events.GetEvents(EventDefinitionFilter.LastIn).ToArray();
                Assert.AreEqual(1, events.Length);
                Assert.AreEqual(content3, ((SaveEventArgs<IContent>)events[0].Args).SavedEntities.First());
                Assert.IsTrue(object.ReferenceEquals(content3, ((SaveEventArgs<IContent>)events[0].Args).SavedEntities.First()));
                Assert.AreEqual(content3.UpdateDate, ((SaveEventArgs<IContent>)events[0].Args).SavedEntities.First().UpdateDate);
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EventsDispatching_Passive(bool complete)
        {
            DoThing1 += OnDoThingFail;
            DoThing2 += OnDoThingFail;
            DoThing3 += OnDoThingFail;

            var scopeProvider = _testObjects.GetScopeProvider(Mock.Of<ILogger>());
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

            var scopeProvider = _testObjects.GetScopeProvider(Mock.Of<ILogger>()) as ScopeProvider;

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

        public static event EventHandler<SaveEventArgs<IContent>> DoSaveForContent;
        public static event EventHandler<DeleteEventArgs<IContent>> DoDeleteForContent;
        public static event EventHandler<TestEventArgs> DoForTestArgs;
        public static event EventHandler<TestEventArgs2> DoForTestArgs2;
        public static event EventHandler<SaveEventArgs<string>> DoThing1;
        public static event EventHandler<SaveEventArgs<int>> DoThing2;

        public static event TypedEventHandler<ScopeEventDispatcherTests, SaveEventArgs<decimal>> DoThing3;

        public static event TypedEventHandler<IContentService, PublishEventArgs<IContent>> Test_Unpublished;
        public static event TypedEventHandler<IContentService, DeleteEventArgs<IContent>> Test_Deleted;

        public class TestEventArgs : CancellableObjectEventArgs
        {
            public TestEventArgs(object eventObject) : base(eventObject)
            {
            }

            public object MyEventObject
            {
                get { return EventObject; }
            }
        }

        [SupersedeEvent(typeof(TestEventArgs))]
        public class TestEventArgs2 : CancellableObjectEventArgs
        {
            public TestEventArgs2(object eventObject) : base(eventObject)
            {
            }

            public object MyEventObject
            {
                get { return EventObject; }
            }
        }

        public class PassiveEventDispatcher : QueuingEventDispatcherBase
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
