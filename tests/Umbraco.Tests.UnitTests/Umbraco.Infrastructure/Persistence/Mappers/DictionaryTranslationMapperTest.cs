// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Mappers;

[TestFixture]
public class DictionaryTranslationMapperTest
{
    [Test]
    public void Can_Map_Key_Property()
    {
        // Act
        var column =
            new DictionaryTranslationMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Key");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsLanguageText].[UniqueId]"));
    }

    [Test]
    public void Can_Map_Language_Property()
    {
        // Act
        var column =
            new DictionaryTranslationMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Language");

        // Assert
        Assert.That(column, Is.EqualTo("[cmsLanguageText].[languageId]"));
    }

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
