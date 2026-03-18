// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Reflection;
using System.Xml;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Composing;

/// <summary>
///     Tests for typefinder
/// </summary>
[TestFixture]
public class TypeFinderTests
{
    /// <summary>
    /// Initializes the test by setting up the assemblies to be used.
    /// </summary>
    [SetUp]
    public void Initialize() => _assemblies = new[]
    {
        GetType().Assembly,
        typeof(Guid).Assembly,
        typeof(Assert).Assembly,
        typeof(NameTable).Assembly,
        typeof(TypeFinder).Assembly,
    };

    /// <summary>
    ///     List of assemblies to scan
    /// </summary>
    private Assembly[] _assemblies;

    /// <summary>
    /// Tests finding classes of a specified type that have a specific attribute.
    /// </summary>
    [Test]
    public void Find_Class_Of_Type_With_Attribute()
    {
        var typeFinder = new TypeFinder(
            Mock.Of<ILogger<TypeFinder>>(),
            new DefaultUmbracoAssemblyProvider(GetType().Assembly, NullLoggerFactory.Instance),
            null);
        var typesFound = typeFinder.FindClassesOfTypeWithAttribute<TestEditor, MyTestAttribute>(_assemblies);
        Assert.AreEqual(2, typesFound.Count());
    }

    /// <summary>
    /// A custom attribute defined specifically for use in unit tests within the <see cref="TypeFinderTests"/> class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MyTestAttribute : Attribute
    {
    }

    /// <summary>
    /// Tests the functionality of the editor type within the TypeFinder, ensuring correct identification and behavior of editor classes.
    /// </summary>
    public abstract class TestEditor
    {
    }

    /// <summary>
    /// Represents a mock benchmark test editor class used for unit testing within the TypeFinderTests.
    /// </summary>
    [MyTest]
    public class BenchmarkTestEditor : TestEditor
    {
    }

    /// <summary>
    /// Represents a test editor type for unit testing purposes.
    /// </summary>
    [MyTest]
    public class MyOtherTestEditor : TestEditor
    {
    }
}
