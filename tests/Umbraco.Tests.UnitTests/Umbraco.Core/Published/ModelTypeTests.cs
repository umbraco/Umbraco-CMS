// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Tests.Common.Published;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Published;

[TestFixture]
public class ModelTypeTests
{
    [Test]
    public void ModelTypeEqualityTests()
    {
        Assert.That(ModelType.For("alias1"), Is.Not.EqualTo(ModelType.For("alias1")));

        Assert.That(ModelType.Equals(ModelType.For("alias1"), ModelType.For("alias1")), Is.True);
        Assert.That(ModelType.Equals(ModelType.For("alias1"), ModelType.For("alias2")), Is.False);

        Assert.That(ModelType.Equals(typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias1")), typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias1"))), Is.True);
        Assert.That(ModelType.Equals(typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias1")), typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias2"))), Is.False);

        Assert.That(
            ModelType.Equals(ModelType.For("alias1").MakeArrayType(), ModelType.For("alias1").MakeArrayType()), Is.True);
        Assert.That(ModelType.Equals(ModelType.For("alias1").MakeArrayType(), ModelType.For("alias2").MakeArrayType()), Is.False);
    }

    [Test]
    public void TypeToStringTests()
    {
        var type = typeof(int);
        Assert.That(type.ToString(), Is.EqualTo("System.Int32"));
        Assert.That(type.MakeArrayType().ToString(), Is.EqualTo("System.Int32[]"));
        Assert.That(typeof(IEnumerable<>).MakeGenericType(type.MakeArrayType()).ToString(), Is.EqualTo("System.Collections.Generic.IEnumerable`1[System.Int32[]]"));
    }

    [Test]
    public void TypeFullNameTests()
    {
        var type = typeof(int);
        Assert.That(type.FullName, Is.EqualTo("System.Int32"));
        Assert.That(type.MakeArrayType().FullName, Is.EqualTo("System.Int32[]"));

        // Note the inner assembly qualified name
        Assert.That(
            typeof(IEnumerable<>).MakeGenericType(type.MakeArrayType()).FullName, Is.EqualTo($"System.Collections.Generic.IEnumerable`1[[System.Int32[], System.Private.CoreLib, Version={typeof(IEnumerable).Assembly.GetName().Version}, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]"));
    }

    [Test]
    public void ModelTypeMapTests()
    {
        var map = new Dictionary<string, Type>
        {
            { "alias1", typeof(PublishedSnapshotTestObjects.TestElementModel1) },
            { "alias2", typeof(PublishedSnapshotTestObjects.TestElementModel2) },
        };

        Assert.That(
            ModelType.Map(ModelType.For("alias1"), map).ToString(), Is.EqualTo("Umbraco.Cms.Tests.Common.Published.PublishedSnapshotTestObjects+TestElementModel1"));
        Assert.That(
            ModelType.Map(ModelType.For("alias1").MakeArrayType(), map).ToString(), Is.EqualTo("Umbraco.Cms.Tests.Common.Published.PublishedSnapshotTestObjects+TestElementModel1[]"));
        Assert.That(
            ModelType.Map(typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias1")), map).ToString(), Is.EqualTo("System.Collections.Generic.IEnumerable`1[Umbraco.Cms.Tests.Common.Published.PublishedSnapshotTestObjects+TestElementModel1]"));
        Assert.That(
            ModelType.Map(typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias1").MakeArrayType()), map)
                .ToString(), Is.EqualTo("System.Collections.Generic.IEnumerable`1[Umbraco.Cms.Tests.Common.Published.PublishedSnapshotTestObjects+TestElementModel1[]]"));
    }
}
