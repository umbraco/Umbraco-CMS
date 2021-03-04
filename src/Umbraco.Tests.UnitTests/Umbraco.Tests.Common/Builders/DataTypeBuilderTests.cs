// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Tests.Common.Builders
{
    [TestFixture]
    public class DataTypeBuilderTests
    {
        [Test]
        public void Is_Built_Correctly()
        {
            // Arrange
            const int testId = 3123;

            var builder = new DataTypeBuilder();

            // Act
            DataType dataType = builder
                .WithId(testId)
                .Build();

            // Assert
            Assert.AreEqual(testId, dataType.Id);
        }
    }
}
