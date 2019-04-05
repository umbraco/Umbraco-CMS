using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Mapping;

namespace Umbraco.Tests.Mapping
{
    [TestFixture]
    public class MappingTests
    {
        [Test]
        public void SimpleMap()
        {
            var profiles = new MapDefinitionCollection(new IMapDefinition[]
            {
                new Profile1(),
            });
            var mapper = new UmbracoMapper(profiles);

            var thing1 = new Thing1 { Value = "value" };
            var thing2 = mapper.Map<Thing1, Thing2>(thing1);

            Assert.IsNotNull(thing2);
            Assert.AreEqual("value", thing2.Value);

            thing2 = mapper.Map<Thing2>(thing1);

            Assert.IsNotNull(thing2);
            Assert.AreEqual("value", thing2.Value);

            thing2 = new Thing2();
            mapper.Map(thing1, thing2);
            Assert.AreEqual("value", thing2.Value);
        }

        [Test]
        public void EnumerableMap()
        {
            var profiles = new MapDefinitionCollection(new IMapDefinition[]
            {
                new Profile1(),
            });
            var mapper = new UmbracoMapper(profiles);

            var thing1A = new Thing1 { Value = "valueA" };
            var thing1B = new Thing1 { Value = "valueB" };
            var thing1 = new[] { thing1A, thing1B };
            var thing2 = mapper.Map<IEnumerable<Thing1>, IEnumerable<Thing2>>(thing1).ToList();

            Assert.IsNotNull(thing2);
            Assert.AreEqual(2, thing2.Count);
            Assert.AreEqual("valueA", thing2[0].Value);
            Assert.AreEqual("valueB", thing2[1].Value);

            thing2 = mapper.Map<IEnumerable<Thing2>>(thing1).ToList();

            Assert.IsNotNull(thing2);
            Assert.AreEqual(2, thing2.Count);
            Assert.AreEqual("valueA", thing2[0].Value);
            Assert.AreEqual("valueB", thing2[1].Value);
        }

        [Test]
        public void InheritedMap()
        {
            var profiles = new MapDefinitionCollection(new IMapDefinition[]
            {
                new Profile1(),
            });
            var mapper = new UmbracoMapper(profiles);

            var thing3 = new Thing3 { Value = "value" };
            var thing2 = mapper.Map<Thing3, Thing2>(thing3);

            Assert.IsNotNull(thing2);
            Assert.AreEqual("value", thing2.Value);

            thing2 = mapper.Map<Thing2>(thing3);

            Assert.IsNotNull(thing2);
            Assert.AreEqual("value", thing2.Value);

            thing2 = new Thing2();
            mapper.Map(thing3, thing2);
            Assert.AreEqual("value", thing2.Value);
        }

        private class Thing1
        {
            public string Value { get; set; }
        }

        private class Thing3 : Thing1
        { }

        private class Thing2
        {
            public string Value { get; set; }
        }

        private class Profile1 : IMapDefinition
        {
            public void DefineMaps(UmbracoMapper mapper)
            {
                mapper.Define<Thing1, Thing2>((source, context) => new Thing2(), Map);
            }

            private void Map(Thing1 source, Thing2 target, MapperContext context)
            {
                target.Value = source.Value;
            }
        }
    }
}
