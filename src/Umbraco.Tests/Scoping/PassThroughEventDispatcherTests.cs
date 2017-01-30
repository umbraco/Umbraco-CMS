using System;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;

namespace Umbraco.Tests.Scoping
{
    [TestFixture]
    public class PassThroughEventDispatcherTests
    {
        [Test]
        public void TriggersCancelableEvents()
        {
            var counter = 0;

            DoThing1 += (sender, args) => { counter++; };

            var scopeProvider = new ScopeProvider(Mock.Of<IDatabaseFactory2>());
            var events = scopeProvider.AmbientOrNoScope.Events;
            events.DispatchCancelable(DoThing1, this, new CancellableEventArgs());

            Assert.AreEqual(1, counter);
        }

        [Test]
        public void TriggersEvents()
        {
            var counter = 0;

            DoThing1 += (sender, args) => { counter++; };
            DoThing2 += (sender, args) => { counter++; };
            DoThing3 += (sender, args) => { counter++; };

            var scopeProvider = new ScopeProvider(Mock.Of<IDatabaseFactory2>());
            var events = scopeProvider.AmbientOrNoScope.Events;
            events.Dispatch(DoThing1, this, new EventArgs());
            events.Dispatch(DoThing2, this, new EventArgs());
            events.Dispatch(DoThing3, this, new EventArgs());

            Assert.AreEqual(3, counter);
        }

        [Test]
        public void DoesNotQueueEvents()
        {
            var scopeProvider = new ScopeProvider(Mock.Of<IDatabaseFactory2>());
            var events = scopeProvider.AmbientOrNoScope.Events;
            events.Dispatch(DoThing1, this, new EventArgs());
            events.Dispatch(DoThing2, this, new EventArgs());
            events.Dispatch(DoThing3, this, new EventArgs());

            Assert.IsEmpty(events.GetEvents());
        }

        public event EventHandler DoThing1;
        public event EventHandler<EventArgs> DoThing2;
        public event TypedEventHandler<PassThroughEventDispatcherTests, EventArgs> DoThing3;
    }
}