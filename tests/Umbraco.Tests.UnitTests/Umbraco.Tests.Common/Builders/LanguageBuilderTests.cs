// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Tests.Common.Builders
{
    [TestFixture]
    public class LanguageBuilderTests
    {
        [Test]
        public void Is_Built_Correctly()
        {
            // Arrange
            var builder = new LanguageBuilder();

            var expected = CultureInfo.GetCultureInfo("en-GB");

            // Act
            ILanguage language = builder
                .WithCultureInfo(expected.Name)
                .Build();

            // Assert
            Assert.AreEqual(expected.Name, language.IsoCode);
            Assert.AreEqual(expected.EnglishName, language.CultureName);
        }
    }
}
