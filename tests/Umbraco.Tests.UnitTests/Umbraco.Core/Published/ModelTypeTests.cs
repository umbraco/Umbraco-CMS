// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
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
        Assert.AreNotEqual(ModelType.For("alias1"), ModelType.For("alias1"));

        Assert.IsTrue(ModelType.Equals(ModelType.For("alias1"), ModelType.For("alias1")));
        Assert.IsFalse(ModelType.Equals(ModelType.For("alias1"), ModelType.For("alias2")));

        Assert.IsTrue(ModelType.Equals(typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias1")), typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias1"))));
        Assert.IsFalse(ModelType.Equals(typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias1")), typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias2"))));

        Assert.IsTrue(
            ModelType.Equals(ModelType.For("alias1").MakeArrayType(), ModelType.For("alias1").MakeArrayType()));
        Assert.IsFalse(ModelType.Equals(ModelType.For("alias1").MakeArrayType(), ModelType.For("alias2").MakeArrayType()));
    }

    [Test]
    public void TypeToStringTests()
    {
        var type = typeof(int);
        Assert.AreEqual("System.Int32", type.ToString());
        Assert.AreEqual("System.Int32[]", type.MakeArrayType().ToString());
        Assert.AreEqual("System.Collections.Generic.IEnumerable`1[System.Int32[]]", typeof(IEnumerable<>).MakeGenericType(type.MakeArrayType()).ToString());
    }

    [Test]
    public void TypeFullNameTests()
    {
        var type = typeof(int);
        Assert.AreEqual("System.Int32", type.FullName);
        Assert.AreEqual("System.Int32[]", type.MakeArrayType().FullName);

        // Note the inner assembly qualified name
        Assert.AreEqual(
            "System.Collections.Generic.IEnumerable`1[[System.Int32[], System.Private.CoreLib, Version=6.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]",
            typeof(IEnumerable<>).MakeGenericType(type.MakeArrayType()).FullName);
    }

    [Test]
    public void ModelTypeMapTests()
    {
        var map = new Dictionary<string, Type>
        {
            { "alias1", typeof(PublishedSnapshotTestObjects.TestElementModel1) },
            { "alias2", typeof(PublishedSnapshotTestObjects.TestElementModel2) },
        };

        Assert.AreEqual(
            "Umbraco.Cms.Tests.Common.Published.PublishedSnapshotTestObjects+TestElementModel1",
            ModelType.Map(ModelType.For("alias1"), map).ToString());
        Assert.AreEqual(
            "Umbraco.Cms.Tests.Common.Published.PublishedSnapshotTestObjects+TestElementModel1[]",
            ModelType.Map(ModelType.For("alias1").MakeArrayType(), map).ToString());
        Assert.AreEqual(
            "System.Collections.Generic.IEnumerable`1[Umbraco.Cms.Tests.Common.Published.PublishedSnapshotTestObjects+TestElementModel1]",
            ModelType.Map(typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias1")), map).ToString());
        Assert.AreEqual(
            "System.Collections.Generic.IEnumerable`1[Umbraco.Cms.Tests.Common.Published.PublishedSnapshotTestObjects+TestElementModel1[]]",
            ModelType.Map(typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias1").MakeArrayType()), map)
                .ToString());
    }
}
