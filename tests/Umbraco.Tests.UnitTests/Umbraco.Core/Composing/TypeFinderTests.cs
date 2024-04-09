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

    [AttributeUsage(AttributeTargets.Class)]
    public class MyTestAttribute : Attribute
    {
    }

    public abstract class TestEditor
    {
    }

    [MyTest]
    public class BenchmarkTestEditor : TestEditor
    {
    }

    [MyTest]
    public class MyOtherTestEditor : TestEditor
    {
    }
}
