// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Mappers;

/// <summary>
/// Contains unit tests for the <see cref="DictionaryTranslationMapper"/> class, verifying its mapping functionality.
/// </summary>
[TestFixture]
public class DictionaryTranslationMapperTest
{
    /// <summary>
    /// Tests that the Key property is correctly mapped to the expected column.
    /// </summary>
    [Test]
    public void Can_Map_Key_Property()
    {
        // Act
        var column =
            new DictionaryTranslationMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Key");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsLanguageText].[UniqueId]"));
    }

    /// <summary>
    /// Tests that the Value property is correctly mapped by the DictionaryTranslationMapper.
    /// </summary>
    [Test]
    public void Can_Map_Value_Property()
    {
        // Act
        var column =
            new DictionaryTranslationMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Value");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsLanguageText].[value]"));
    }
}
