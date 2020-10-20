﻿using System.Globalization;
using NUnit.Framework;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Common.Builders.Extensions;

namespace Umbraco.Tests.UnitTests.Umbraco.Tests.Common.Builders
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
            var language = builder
                .WithCultureInfo(expected.Name)
                .Build();

            // Assert
            Assert.AreEqual(expected.Name, language.IsoCode);
            Assert.AreEqual(expected.EnglishName, language.CultureName);
        }
    }
}
