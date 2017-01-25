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
    public class NoScopedEventManagerTests
    {

        [Test]
        public void Does_Support_Event_Cancellation()
        {
            var scopeProvider = new ScopeProvider(Mock.Of<IDatabaseFactory2>());
            Assert.IsTrue(scopeProvider.AmbientOrNoScope.Events.SupportsEventCancellation);
        }

        [Test]
        public void Does_Immediately_Raise_Events()
        {
            var counter = 0;

            this.DoThing1 += (sender, args) =>
            {
                counter++;
            };

            this.DoThing2 += (sender, args) =>
            {
                counter++;
            };

            this.DoThing3 += (sender, args) =>
            {
                counter++;
            };

            var scopeProvider = new ScopeProvider(Mock.Of<IDatabaseFactory2>());
            scopeProvider.AmbientOrNoScope.Events.QueueEvent(DoThing1, this, new EventArgs());
            scopeProvider.AmbientOrNoScope.Events.QueueEvent(DoThing2, this, new EventArgs());
            scopeProvider.AmbientOrNoScope.Events.QueueEvent(DoThing3, this, new EventArgs());

            Assert.AreEqual(3, counter);

        }

        [Test]
        public void Can_Not_Raise_Events_Later()
        {
            var scopeProvider = new ScopeProvider(Mock.Of<IDatabaseFactory2>());
            scopeProvider.AmbientOrNoScope.Events.QueueEvent(DoThing1, this, new EventArgs());
            scopeProvider.AmbientOrNoScope.Events.QueueEvent(DoThing2, this, new EventArgs());
            scopeProvider.AmbientOrNoScope.Events.QueueEvent(DoThing3, this, new EventArgs());

            Assert.AreEqual(0, scopeProvider.AmbientOrNoScope.Events.GetEvents().Count());
        }

        public event EventHandler DoThing1;
        public event EventHandler<EventArgs> DoThing2;
        public event TypedEventHandler<NoScopedEventManagerTests, EventArgs> DoThing3;
    }
}