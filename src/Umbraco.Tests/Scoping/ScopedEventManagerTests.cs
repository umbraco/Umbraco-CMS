using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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
        [SetUp]
        public void Setup()
        {
            //remove all handlers first
            DoThing1 = null;
            DoThing2 = null;
            DoThing3 = null;
        }

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
        public void Can_Get_Event_Info()
        {
            DoThing1 += OnDoThingFail;
            DoThing2 += OnDoThingFail;
            DoThing3 += OnDoThingFail;

            var provider = new PetaPocoUnitOfWorkProvider(new ScopeProvider(Mock.Of<IDatabaseFactory2>()));
            using (var uow = provider.GetUnitOfWork())
            {
                uow.EventManager.QueueEvent(DoThing1, this, new SaveEventArgs<string>("test"));
                uow.EventManager.QueueEvent(DoThing2, this, new SaveEventArgs<int>(0));
                uow.EventManager.QueueEvent(DoThing3, this, new SaveEventArgs<decimal>(0));
                
                var e = uow.EventManager.GetEvents().ToArray();
                var knownNames = new [] {"DoThing1", "DoThing2", "DoThing3"};
                var knownArgTypes = new [] { typeof(SaveEventArgs<string>), typeof(SaveEventArgs<int>), typeof(SaveEventArgs<decimal>) };

                for (int i = 0; i < e.Length; i++)
                {
                    Assert.AreEqual(knownNames[i], e[i].EventName);
                    Assert.AreEqual(knownArgTypes[i], e[i].Args.GetType());
                }
            }
        }

        [Test]
        public void Does_Not_Immediately_Raise_Events()
        {
            DoThing1 += OnDoThingFail;
            DoThing2 += OnDoThingFail;
            DoThing3 += OnDoThingFail;

            var provider = new PetaPocoUnitOfWorkProvider(new ScopeProvider(Mock.Of<IDatabaseFactory2>()));
            using (var uow = provider.GetUnitOfWork())
            {
                uow.EventManager.QueueEvent(DoThing1, this, new SaveEventArgs<string>("test"));
                uow.EventManager.QueueEvent(DoThing2, this, new SaveEventArgs<int>(0));
                uow.EventManager.QueueEvent(DoThing3, this, new SaveEventArgs<decimal>(0));

                Assert.Pass();
            }
        }

        [Test]
        public void Can_Raise_Events_Later()
        {
            var counter = 0;

            DoThing1 += (sender, args) =>
            {
                counter++;
            };

            DoThing2 += (sender, args) =>
            {
                counter++;
            };

            DoThing3 += (sender, args) =>
            {
                counter++;
            };

            var provider = new PetaPocoUnitOfWorkProvider(new ScopeProvider(Mock.Of<IDatabaseFactory2>()));
            using (var uow = provider.GetUnitOfWork())
            {
                uow.EventManager.QueueEvent(DoThing1, this, new SaveEventArgs<string>("test"));
                uow.EventManager.QueueEvent(DoThing2, this, new SaveEventArgs<int>(0));
                uow.EventManager.QueueEvent(DoThing3, this, new SaveEventArgs<decimal>(0));

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


        public static event EventHandler<SaveEventArgs<string>> DoThing1;

        public static event EventHandler<SaveEventArgs<int>> DoThing2;

        public static event TypedEventHandler<ScopedEventManagerTests, SaveEventArgs<decimal>> DoThing3;
    }
    
}