// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

/// <summary>
/// Contains unit tests for the <see cref="DataType"/> model in the Umbraco.Core.Models namespace.
/// </summary>
[TestFixture]
public class DataTypeTests
{
    /// <summary>
    /// Tests updating properties on a DataType and verifies that the dirty properties are tracked correctly.
    /// </summary>
    [Test]
    public void Can_Update_And_Verify_Dirty_Properties()
    {
        var dataType = new DataTypeBuilder()
            .WithName("Test Data Type")
            .WithDatabaseType(ValueStorageType.Ntext)
            .Build();

        dataType.ResetDirtyProperties();

        dataType.DatabaseType = ValueStorageType.Nvarchar;
        dataType.EditorUiAlias = "Test.EditorUiAlias";

        var dirtyProperties = dataType.GetDirtyProperties().OrderBy(x => x).ToList();

        Assert.IsTrue(dataType.IsPropertyDirty(nameof(dataType.DatabaseType)));
        Assert.IsTrue(dataType.IsPropertyDirty(nameof(dataType.EditorUiAlias)));

        Assert.AreEqual(2, dirtyProperties.Count);
        Assert.AreEqual($"{nameof(dataType.DatabaseType)},{nameof(dataType.EditorUiAlias)}", string.Join(",", dirtyProperties));
    }
}
