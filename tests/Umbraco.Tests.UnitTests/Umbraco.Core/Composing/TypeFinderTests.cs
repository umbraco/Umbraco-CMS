// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Web.BackOffice.Trees;
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
            new DefaultUmbracoAssemblyProvider(GetType().Assembly, NullLoggerFactory.Instance));
        var typesFound = typeFinder.FindClassesOfTypeWithAttribute<TestEditor, MyTestAttribute>(_assemblies);
        Assert.AreEqual(2, typesFound.Count());
    }

    [Test]
    public void Find_Classes_With_Attribute()
    {
        var typeFinder = new TypeFinder(
            Mock.Of<ILogger<TypeFinder>>(),
            new DefaultUmbracoAssemblyProvider(GetType().Assembly, NullLoggerFactory.Instance));
        var typesFound = typeFinder.FindClassesWithAttribute<TreeAttribute>(_assemblies);
        Assert.AreEqual(0, typesFound.Count()); // 0 classes in _assemblies are marked with [Tree]

        typesFound = typeFinder.FindClassesWithAttribute<TreeAttribute>(new[] { typeof(TreeAttribute).Assembly });
        Assert.AreEqual(23, typesFound.Count()); // + classes in Umbraco.Web are marked with [Tree]

        typesFound = typeFinder.FindClassesWithAttribute<TreeAttribute>();
        Assert.AreEqual(23, typesFound.Count()); // + classes in Umbraco.Web are marked with [Tree]
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
