// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging.Abstractions;
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

/// <summary>
/// Contains unit tests for verifying the mapping configurations and behaviors in the Umbraco infrastructure.
/// </summary>
[TestFixture]
public class MappingTests
{
    /// <summary>
    /// Initializes a mock implementation of <see cref="IScopeProvider"/> for use in unit tests,
    /// configuring it to return a mock <see cref="IScope"/> when a scope is created. This setup
    /// is used to isolate tests from actual database or scope logic.
    /// </summary>
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

    /// <summary>
    /// Tests simple mapping functionality of the UmbracoMapper.
    /// </summary>
    [Test]
    public void SimpleMap()
    {
        var definitions = new MapDefinitionCollection(() => new IMapDefinition[] { new MapperDefinition1() });
        var mapper = new UmbracoMapper(definitions, _scopeProvider, NullLogger<UmbracoMapper>.Instance);

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

    /// <summary>
    /// Tests that enumerable collections of one type can be mapped to enumerable collections of another type
    /// using the UmbracoMapper, and verifies that the mapped values are correct.
    /// </summary>
    [Test]
    public void EnumerableMap()
    {
        var definitions = new MapDefinitionCollection(() => new IMapDefinition[] { new MapperDefinition1() });
        var mapper = new UmbracoMapper(definitions, _scopeProvider, NullLogger<UmbracoMapper>.Instance);

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

    /// <summary>
    /// Verifies that <see cref="UmbracoMapper"/> correctly maps properties from an inherited source type (<see cref="Thing3"/>) to a base destination type (<see cref="Thing2"/>), ensuring inherited members are handled as expected.
    /// </summary>
    [Test]
    public void InheritedMap()
    {
        var definitions = new MapDefinitionCollection(() => new IMapDefinition[] { new MapperDefinition1() });
        var mapper = new UmbracoMapper(definitions, _scopeProvider, NullLogger<UmbracoMapper>.Instance);

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

    /// <summary>
    /// Tests the thread safety of the UmbracoMapper by performing concurrent mapping operations.
    /// This test ensures that the mapper can handle concurrent access and map definitions correctly without throwing exceptions.
    /// </summary>
    [Test]
    [Explicit]
    public void ConcurrentMap()
    {
        var definitions = new MapDefinitionCollection(() => new IMapDefinition[]
        {
            new MapperDefinition1(),
            new MapperDefinition3(),
        });
        var mapper = new UmbracoMapper(definitions, _scopeProvider, NullLogger<UmbracoMapper>.Instance);

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

    /// <summary>
    /// Tests that enum values are correctly mapped from a <see cref="Thing5"/> instance to a <see cref="Thing6"/> instance
    /// using <see cref="UmbracoMapper"/>. Verifies that each enum property is mapped to the corresponding value.
    /// </summary>
    [Test]
    public void EnumMap()
    {
        var definitions = new MapDefinitionCollection(() => new IMapDefinition[] { new MapperDefinition4() });
        var mapper = new UmbracoMapper(definitions, _scopeProvider, NullLogger<UmbracoMapper>.Instance);

        var thing5 = new Thing5 { Fruit1 = Thing5Enum.Apple, Fruit2 = Thing5Enum.Banana, Fruit3 = Thing5Enum.Cherry };

        var thing6 = mapper.Map<Thing5, Thing6>(thing5);

        Assert.IsNotNull(thing6);
        Assert.AreEqual(Thing6Enum.Apple, thing6.Fruit1);
        Assert.AreEqual(Thing6Enum.Banana, thing6.Fruit2);
        Assert.AreEqual(Thing6Enum.Cherry, thing6.Fruit3);
    }

    /// <summary>
    /// Tests that mapping handles null properties correctly.
    /// </summary>
    [Test]
    public void NullPropertyMap()
    {
        var definitions = new MapDefinitionCollection(() => new IMapDefinition[] { new MapperDefinition5() });
        var mapper = new UmbracoMapper(definitions, _scopeProvider, NullLogger<UmbracoMapper>.Instance);

        var thing7 = new Thing7();

        var thing8 = mapper.Map<Thing7, Thing8>(thing7);

        Assert.IsNotNull(thing8);
        Assert.IsNull(thing8.Things);
    }

    private class Thing1
    {
    /// <summary>
    /// Gets or sets the value.
    /// </summary>
        public string Value { get; set; }
    }

    private class Thing3 : Thing1
    {
    }

    private class Thing2
    {
    /// <summary>
    /// Gets or sets the string value associated with this <see cref="Thing2"/> instance.
    /// </summary>
        public string Value { get; set; }
    }

    private class Thing4
    {
    }

    private class Thing5
    {
    /// <summary>
    /// Gets or sets the fruit type represented by the <see cref="Thing5Enum"/> enumeration.
    /// </summary>
        public Thing5Enum Fruit1 { get; set; }

    /// <summary>
    /// Gets or sets the value of the Fruit2 property, which is of type <see cref="Thing5Enum"/>.
    /// </summary>
        public Thing5Enum Fruit2 { get; set; }

    /// <summary>
    /// Gets or sets the value of Fruit3 as a <see cref="Thing5Enum"/>.
    /// </summary>
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
    /// <summary>
    /// Gets or sets the value of Fruit1 as a <see cref="Thing6Enum"/>.
    /// </summary>
        public Thing6Enum Fruit1 { get; set; }

    /// <summary>
    /// Gets or sets the value of the Fruit2 property as a Thing6Enum.
    /// </summary>
        public Thing6Enum Fruit2 { get; set; }

    /// <summary>
    /// Gets or sets the Fruit3 value of type Thing6Enum.
    /// </summary>
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
    /// <summary>
    /// Gets or sets the collection of <see cref="Thing1"/> instances associated with this <see cref="Thing7"/>.
    /// </summary>
        public IEnumerable<Thing1> Things { get; set; }
    }

    private class Thing8
    {
    /// <summary>
    /// Gets or sets the collection of Thing2 objects.
    /// </summary>
        public IEnumerable<Thing2> Things { get; set; }
    }

    private class MapperDefinition1 : IMapDefinition
    {
    /// <summary>
    /// Defines the mapping configurations using the provided mapper.
    /// </summary>
    /// <param name="mapper">The mapper to define mappings on.</param>
        public void DefineMaps(IUmbracoMapper mapper) =>
            mapper.Define<Thing1, Thing2>((source, context) => new Thing2(), Map);

        private void Map(Thing1 source, Thing2 target, MapperContext context) => target.Value = source.Value;
    }

    private class MapperDefinition3 : IMapDefinition
    {
    /// <summary>
    /// Defines a set of type mappings on the provided <see cref="IUmbracoMapper"/> instance for testing purposes.
    /// This method registers mappings from several source types (int, string, double, UmbracoMapper, Property) to <c>object</c>.
    /// </summary>
    /// <param name="mapper">The <see cref="IUmbracoMapper"/> instance on which to define the mappings.</param>
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
    /// <summary>
    /// Defines the object mappings for <see cref="Thing5"/> to <see cref="Thing6"/> and <see cref="Thing5Enum"/> to <see cref="Thing6Enum"/> using the provided mapper.
    /// </summary>
    /// <param name="mapper">The <see cref="IUmbracoMapper"/> instance on which to define the mappings.</param>
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
    /// <summary>
    /// Defines the mapping configurations using the provided IUmbracoMapper.
    /// </summary>
    /// <param name="mapper">The mapper to define mappings on.</param>
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
