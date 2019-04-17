using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        [Test]
        [Explicit]
        public void ConcurrentMap()
        {
            var definitions = new MapDefinitionCollection(new IMapDefinition[]
            {
                new MapperDefinition1(),
                new MapperDefinition3(),
            });
            var mapper = new UmbracoMapper(definitions);

            // the mapper currently has a map from Thing1 to Thing2
            // because Thing3 inherits from Thing1, it will map a Thing3 instance,
            // and register a new map from Thing3 to Thing2,
            // thus modifying its internal dictionaries

            // if timing is good, and mapper does have non-concurrent dictionaries, it fails
            // practically, to reproduce, one needs to add a 1s sleep in the mapper's loop
            // hence, this test is explicit

            var thing3 = new Thing3 { Value = "value" };
            var thing4 = new Thing4();
            Exception caught = null;

            void ThreadLoop()
            {
                // keep failing at mapping - and looping through the maps
                for (var i = 0; i < 10; i++)
                {
                    try
                    {
                        mapper.Map<Thing2>(thing4);
                    }
                    catch (Exception e)
                    {
                        caught = e;
                        Console.WriteLine($"{e.GetType().Name} {e.Message}");
                    }
                }

                Console.WriteLine("done");
            }

            var thread = new Thread(ThreadLoop);
            thread.Start();
            Thread.Sleep(1000);

            try
            {
                Console.WriteLine($"{DateTime.Now:O} mapping");
                var thing2 = mapper.Map<Thing2>(thing3);
                Console.WriteLine($"{DateTime.Now:O} mapped");

                Assert.IsNotNull(thing2);
                Assert.AreEqual("value", thing2.Value);
            }
            finally
            {
                thread.Join();
            }
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

        private class Thing4
        { }

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

        private class MapperDefinition3 : IMapDefinition
        {
            public void DefineMaps(UmbracoMapper mapper)
            {
                // just some random things so that the mapper contains things
                mapper.Define<int, object>();
                mapper.Define<string, object>();
                mapper.Define<double, object>();
                mapper.Define<UmbracoMapper, object>();
                mapper.Define<Property, object>();
            }
        }
    }
}
