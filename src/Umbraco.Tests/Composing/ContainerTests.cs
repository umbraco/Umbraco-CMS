using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.LightInject;

namespace Umbraco.Tests.Composing
{
    [TestFixture]
    public class ContainerTests
    {
        // tests that a container conforms

        private IContainer GetContainer() => LightInjectContainer.Create();

        [Test]
        public void CanGetEnumerable()
        {
            var container = GetContainer();

            container.Register<Thing1>();
            container.Register<Thing2>();
            container.Register<NeedThings>();

            var needThings = container.GetInstance<NeedThings>();
            Assert.AreEqual(2, needThings.Things.Count());
        }

        public abstract class ThingBase { }
        public class Thing1 : ThingBase { }
        public class Thing2 : ThingBase { }

        public class NeedThings
        {
            public NeedThings(IEnumerable<ThingBase> things)
            {
                Things = things;
            }

            public IEnumerable<ThingBase> Things { get; }
        }
    }
}
