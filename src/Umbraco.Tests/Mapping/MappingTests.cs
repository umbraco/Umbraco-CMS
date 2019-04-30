﻿using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Tests.Mapping
{
    [TestFixture]
    public class MappingTests
    {
        [Test]
        public void SimpleMap()
        {
            var definitions = new MapDefinitionCollection(new IMapDefinition[]
            {
                new MapperDefinition1(),
            });
            var mapper = new UmbracoMapper(definitions);

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
            var definitions = new MapDefinitionCollection(new IMapDefinition[]
            {
                new MapperDefinition1(),
            });
            var mapper = new UmbracoMapper(definitions);

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

            thing2 = mapper.MapEnumerable<Thing1, Thing2>(thing1).ToList();

            Assert.IsNotNull(thing2);
            Assert.AreEqual(2, thing2.Count);
            Assert.AreEqual("valueA", thing2[0].Value);
            Assert.AreEqual("valueB", thing2[1].Value);
        }

        [Test]
        public void InheritedMap()
        {
            var definitions = new MapDefinitionCollection(new IMapDefinition[]
            {
                new MapperDefinition1(),
            });
            var mapper = new UmbracoMapper(definitions);

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

        [Test]
        public void CollectionsMap()
        {
            var definitions = new MapDefinitionCollection(new IMapDefinition[]
            {
                new MapperDefinition2(),
            });
            var mapper = new UmbracoMapper(definitions);

            // can map a PropertyCollection
            var source = new PropertyCollection();
            var target = mapper.Map<IEnumerable<ContentPropertyDto>>(source);
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

        private class MapperDefinition1 : IMapDefinition
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

        private class MapperDefinition2 : IMapDefinition
        {
            public void DefineMaps(UmbracoMapper mapper)
            {
                mapper.Define<Property, ContentPropertyDto>((source, context) => new ContentPropertyDto(), Map);
            }

            private static void Map(Property source, ContentPropertyDto target, MapperContext context)
            { }
        }
    }
}
