// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Models;

    /// <summary>
    /// Contains unit tests for the <see cref="DataType"/> model within the Umbraco CMS infrastructure layer.
    /// </summary>
[TestFixture]
public class DataTypeTests
{
    /// <summary>
    /// Sets up the test environment before each test.
    /// </summary>
    [SetUp]
    public void SetUp() => _builder = new DataTypeBuilder();

    private DataTypeBuilder _builder;

    /// <summary>
    /// Verifies that a <see cref="DataType"/> instance can be deep cloned, ensuring that
    /// the clone is a separate object with all property values equal to the original.
    /// The test checks both individual properties and uses reflection to confirm all properties match.
    /// </summary>
    [Test]
    public void Can_Deep_Clone()
    {
        var dtd = _builder
            .WithId(3123)
            .Build();

        var clone = (DataType)dtd.DeepClone();

        Assert.AreNotSame(clone, dtd);
        Assert.AreEqual(clone, dtd);
        Assert.AreEqual(clone.CreateDate, dtd.CreateDate);
        Assert.AreEqual(clone.CreatorId, dtd.CreatorId);
        Assert.AreEqual(clone.DatabaseType, dtd.DatabaseType);
        Assert.AreEqual(clone.Id, dtd.Id);
        Assert.AreEqual(clone.Key, dtd.Key);
        Assert.AreEqual(clone.Level, dtd.Level);
        Assert.AreEqual(clone.Name, dtd.Name);
        Assert.AreEqual(clone.ParentId, dtd.ParentId);
        Assert.AreEqual(clone.Path, dtd.Path);
        Assert.AreEqual(clone.SortOrder, dtd.SortOrder);
        Assert.AreEqual(clone.Trashed, dtd.Trashed);
        Assert.AreEqual(clone.UpdateDate, dtd.UpdateDate);

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(dtd, null));
        }
    }

    /// <summary>
    /// Tests that the data type can be serialized without throwing an error.
    /// </summary>
    [Test]
    public void Can_Serialize_Without_Error()
    {
        var item = _builder.Build();

        Assert.DoesNotThrow(() => JsonSerializer.Serialize(item));
    }
}
