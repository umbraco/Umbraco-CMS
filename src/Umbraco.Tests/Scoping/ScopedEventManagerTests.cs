using System;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Scoping;

namespace Umbraco.Tests.Scoping
{
    [TestFixture]
    public class ScopedEventManagerTests
    {

        [Test]
        public void Does_Not_Support_Event_Cancellation()
        {
            var provider = new PetaPocoUnitOfWorkProvider(new ScopeProvider(Mock.Of<IDatabaseFactory2>()));
            using (var uow = provider.GetUnitOfWork())
            {
                Assert.IsFalse(uow.EventManager.SupportsEventCancellation);
            }
        }

        [Test]
        public void Does_Not_Immediately_Raise_Events()
        {
            this.DoThing1 += OnDoThingFail;
            this.DoThing2 += OnDoThingFail;
            this.DoThing3 += OnDoThingFail;

            var provider = new PetaPocoUnitOfWorkProvider(new ScopeProvider(Mock.Of<IDatabaseFactory2>()));
            using (var uow = provider.GetUnitOfWork())
            {
                uow.EventManager.TrackEvent(DoThing1, this, new EventArgs());
                uow.EventManager.TrackEvent(DoThing2, this, new EventArgs());
                uow.EventManager.TrackEvent(DoThing3, this, new EventArgs());

                Assert.Pass();
            }
        }

        [Test]
        public void Can_Raise_Events_Later()
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

            var provider = new PetaPocoUnitOfWorkProvider(new ScopeProvider(Mock.Of<IDatabaseFactory2>()));
            using (var uow = provider.GetUnitOfWork())
            {
                uow.EventManager.TrackEvent(DoThing1, this, new EventArgs());
                uow.EventManager.TrackEvent(DoThing2, this, new EventArgs());
                uow.EventManager.TrackEvent(DoThing3, this, new EventArgs());

                Assert.AreEqual(0, counter);

                foreach (var e in uow.EventManager.GetEvents())
                {
                    e.RaiseEvent();
                }

                Assert.AreEqual(3, counter);
            }
        }

        private void OnDoThingFail(object sender, EventArgs eventArgs)
        {
            Assert.Fail();
        }

        public event EventHandler DoThing1;
        public event EventHandler<EventArgs> DoThing2;
        public event TypedEventHandler<ScopedEventManagerTests, EventArgs> DoThing3;
    }
}