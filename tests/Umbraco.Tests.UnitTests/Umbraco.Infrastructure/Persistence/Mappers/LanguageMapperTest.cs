// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Mappers;

/// <summary>
/// Contains unit tests for the <see cref="LanguageMapper"/> class, verifying its mapping functionality and behavior.
/// </summary>
[TestFixture]
public class LanguageMapperTest
{
    /// <summary>
    /// Tests that the Id property is correctly mapped to the database column.
    /// </summary>
    [Test]
    public void Can_Map_Id_Property()
    {
        // Act
        var column = new LanguageMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Id");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoLanguage].[id]"));
    }

    /// <summary>
    /// Tests that the IsoCode property is correctly mapped to the database column.
    /// </summary>
    [Test]
    public void Can_Map_IsoCode_Property()
    {
        // Act
        var column = new LanguageMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("IsoCode");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoLanguage].[languageISOCode]"));
    }

    /// <summary>
    /// Tests that the CultureName property is correctly mapped to the database column.
    /// </summary>
    [Test]
    public void Can_Map_CultureName_Property()
    {
        // Act
        var column = new LanguageMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("CultureName");

        // Assert
        Assert.That(column, Is.EqualTo("[umbracoLanguage].[languageCultureName]"));
    }
}
