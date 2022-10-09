// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Scoping;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;
using PropertyCollection = Umbraco.Cms.Core.Models.PropertyCollection;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Mapping;

[TestFixture]
public class MappingTests
{
    [SetUp]
    public void MockScopeProvider()
    {
        var scopeMock = new Mock<IScopeProvider>();
        scopeMock.Setup(x => x.CreateScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher>(),
                It.IsAny<IScopedNotificationPublisher>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(Mock.Of<IScope>);

        _scopeProvider = scopeMock.Object;
    }

    private IScopeProvider _scopeProvider;

    [Test]
    public void SimpleMap()
    {
        var definitions = new MapDefinitionCollection(() => new IMapDefinition[] { new MapperDefinition1() });
        var mapper = new UmbracoMapper(definitions, _scopeProvider);

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
        var definitions = new MapDefinitionCollection(() => new IMapDefinition[] { new MapperDefinition1() });
        var mapper = new UmbracoMapper(definitions, _scopeProvider);

        var thing1A = new Thing1 { Value = "valueA" };
        var thing1B = new Thing1 { Value = "valueB" };
        Thing1[] thing1 = { thing1A, thing1B };
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
        var definitions = new MapDefinitionCollection(() => new IMapDefinition[] { new MapperDefinition1() });
        var mapper = new UmbracoMapper(definitions, _scopeProvider);

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
        var definitions = new MapDefinitionCollection(() => new IMapDefinition[] { new MapperDefinition2() });
        var mapper = new UmbracoMapper(definitions, _scopeProvider);

        // can map a PropertyCollection
        var source = new PropertyCollection();
        var target = mapper.Map<IEnumerable<ContentPropertyDto>>(source);
    }

    [Test]
    [Explicit]
    public void ConcurrentMap()
    {
        var definitions = new MapDefinitionCollection(() => new IMapDefinition[]
        {
            new MapperDefinition1(),
            new MapperDefinition3(),
        });
        var mapper = new UmbracoMapper(definitions, _scopeProvider);

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

    [Test]
    public void EnumMap()
    {
        var definitions = new MapDefinitionCollection(() => new IMapDefinition[] { new MapperDefinition4() });
        var mapper = new UmbracoMapper(definitions, _scopeProvider);

        var thing5 = new Thing5 { Fruit1 = Thing5Enum.Apple, Fruit2 = Thing5Enum.Banana, Fruit3 = Thing5Enum.Cherry };

        var thing6 = mapper.Map<Thing5, Thing6>(thing5);

        Assert.IsNotNull(thing6);
        Assert.AreEqual(Thing6Enum.Apple, thing6.Fruit1);
        Assert.AreEqual(Thing6Enum.Banana, thing6.Fruit2);
        Assert.AreEqual(Thing6Enum.Cherry, thing6.Fruit3);
    }

    [Test]
    public void NullPropertyMap()
    {
        var definitions = new MapDefinitionCollection(() => new IMapDefinition[] { new MapperDefinition5() });
        var mapper = new UmbracoMapper(definitions, _scopeProvider);

        var thing7 = new Thing7();

        var thing8 = mapper.Map<Thing7, Thing8>(thing7);

        Assert.IsNotNull(thing8);
        Assert.IsNull(thing8.Things);
    }

    private class Thing1
    {
        public string Value { get; set; }
    }

    private class Thing3 : Thing1
    {
    }

    private class Thing2
    {
        public string Value { get; set; }
    }

    private class Thing4
    {
    }

    private class Thing5
    {
        public Thing5Enum Fruit1 { get; set; }

        public Thing5Enum Fruit2 { get; set; }

        public Thing5Enum Fruit3 { get; set; }
    }

    private enum Thing5Enum
    {
        Apple = 0,
        Banana = 1,
        Cherry = 2,
    }

    private class Thing6
    {
        public Thing6Enum Fruit1 { get; set; }

        public Thing6Enum Fruit2 { get; set; }

        public Thing6Enum Fruit3 { get; set; }
    }

    private enum Thing6Enum
    {
        Apple = 0,
        Banana = 1,
        Cherry = 2,
    }

    private class Thing7
    {
        public IEnumerable<Thing1> Things { get; set; }
    }

    private class Thing8
    {
        public IEnumerable<Thing2> Things { get; set; }
    }

    private class MapperDefinition1 : IMapDefinition
    {
        public void DefineMaps(IUmbracoMapper mapper) =>
            mapper.Define<Thing1, Thing2>((source, context) => new Thing2(), Map);

        private void Map(Thing1 source, Thing2 target, MapperContext context) => target.Value = source.Value;
    }

    private class MapperDefinition2 : IMapDefinition
    {
        public void DefineMaps(IUmbracoMapper mapper) =>
            mapper.Define<IProperty, ContentPropertyDto>((source, context) => new ContentPropertyDto(), Map);

        private static void Map(IProperty source, ContentPropertyDto target, MapperContext context)
        {
        }
    }

    private class MapperDefinition3 : IMapDefinition
    {
        public void DefineMaps(IUmbracoMapper mapper)
        {
            // just some random things so that the mapper contains things
            mapper.Define<int, object>();
            mapper.Define<string, object>();
            mapper.Define<double, object>();
            mapper.Define<UmbracoMapper, object>();
            mapper.Define<Property, object>();
        }
    }

    private class MapperDefinition4 : IMapDefinition
    {
        public void DefineMaps(IUmbracoMapper mapper)
        {
            mapper.Define<Thing5, Thing6>((source, context) => new Thing6(), Map);
            mapper.Define<Thing5Enum, Thing6Enum>(
                (source, context) => (Thing6Enum)source);
        }

        private void Map(Thing5 source, Thing6 target, MapperContext context)
        {
            target.Fruit1 = context.Map<Thing6Enum>(source.Fruit1);
            target.Fruit2 = context.Map<Thing6Enum>(source.Fruit2);
            target.Fruit3 = context.Map<Thing6Enum>(source.Fruit3);
        }
    }

    private class MapperDefinition5 : IMapDefinition
    {
        public void DefineMaps(IUmbracoMapper mapper)
        {
            mapper.Define<Thing1, Thing2>((source, context) => new Thing2(), Map1);
            mapper.Define<Thing7, Thing8>((source, context) => new Thing8(), Map2);
        }

        private void Map1(Thing1 source, Thing2 target, MapperContext context) =>
            target.Value = source.Value;

        private void Map2(Thing7 source, Thing8 target, MapperContext context) =>
            target.Things = context.Map<IEnumerable<Thing2>>(source.Things);
    }
}
