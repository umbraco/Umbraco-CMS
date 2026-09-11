// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Tests.Common.Builders;

[TestFixture]
public class DataTypeBuilderTests
{
    [Test]
    public void Is_Built_Correctly()
    {
        // Arrange
        const int testId = 3123;
        const string testEditorAlias = "testAlias";

        var builder = new DataTypeBuilder();

        // Act
        var dataType = builder
            .WithId(testId)
            .AddEditor()
            .WithAlias(testEditorAlias)
            .WithDefaultConfiguration(new Dictionary<string, object> { { "value1", "value1" }, { "value2", "value2" } })
            .Done()
            .Build();

        // Assert
        Assert.That(dataType.Id, Is.EqualTo(testId));
        Assert.That(dataType.Editor.Alias, Is.EqualTo(testEditorAlias));
        Assert.That(dataType.Editor.DefaultConfiguration, Has.Count.EqualTo(2));
        Assert.That(dataType.Editor.DefaultConfiguration["value1"], Is.EqualTo("value1"));
    }
}
