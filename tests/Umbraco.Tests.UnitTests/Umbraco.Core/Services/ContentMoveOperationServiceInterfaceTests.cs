using System.Reflection;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class ContentMoveOperationServiceInterfaceTests
{
    [Test]
    public void Interface_Exists_And_Is_Public()
    {
        var interfaceType = typeof(IContentMoveOperationService);

        Assert.That(interfaceType, Is.Not.Null);
        Assert.That(interfaceType.IsInterface, Is.True);
        Assert.That(interfaceType.IsPublic, Is.True);
    }

    [Test]
    public void Interface_Extends_IService()
    {
        var interfaceType = typeof(IContentMoveOperationService);

        Assert.That(typeof(IService).IsAssignableFrom(interfaceType), Is.True);
    }

    [Test]
    [TestCase("Move", new[] { typeof(IContent), typeof(int), typeof(int) })]
    [TestCase("EmptyRecycleBin", new[] { typeof(int) })]
    [TestCase("RecycleBinSmells", new Type[] { })]
    [TestCase("Copy", new[] { typeof(IContent), typeof(int), typeof(bool), typeof(int) })]
    [TestCase("Copy", new[] { typeof(IContent), typeof(int), typeof(bool), typeof(bool), typeof(int) })]
    public void Interface_Has_Required_Method(string methodName, Type[] parameterTypes)
    {
        var interfaceType = typeof(IContentMoveOperationService);
        var method = interfaceType.GetMethod(methodName, parameterTypes);

        Assert.That(method, Is.Not.Null, $"Method {methodName} should exist with specified parameters");
    }

    [Test]
    public void Interface_Has_Sort_Methods()
    {
        var interfaceType = typeof(IContentMoveOperationService);

        // Sort with IEnumerable<IContent>
        var sortContentMethod = interfaceType.GetMethods()
            .FirstOrDefault(m => m.Name == "Sort" &&
                m.GetParameters().Length == 2 &&
                m.GetParameters()[0].ParameterType.IsGenericType);

        // Sort with IEnumerable<int>
        var sortIdsMethod = interfaceType.GetMethods()
            .FirstOrDefault(m => m.Name == "Sort" &&
                m.GetParameters().Length == 2 &&
                m.GetParameters()[0].ParameterType == typeof(IEnumerable<int>));

        Assert.That(sortContentMethod, Is.Not.Null, "Sort(IEnumerable<IContent>, int) should exist");
        Assert.That(sortIdsMethod, Is.Not.Null, "Sort(IEnumerable<int>, int) should exist");
    }

    [Test]
    public void Implementation_Inherits_ContentServiceBase()
    {
        var implementationType = typeof(ContentMoveOperationService);
        var baseType = typeof(ContentServiceBase);

        Assert.That(baseType.IsAssignableFrom(implementationType), Is.True);
    }
}
