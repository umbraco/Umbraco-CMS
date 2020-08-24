using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Tests.Common.Builders;
using static Umbraco.Core.Configuration.Models.RequestHandlerSettings;

namespace Umbraco.Tests.UnitTests.Umbraco.Tests.Common.Builders
{
    [TestFixture]
    public class RequestHandlerSettingsBuilderTests
    {
        [Test]
        public void Is_Built_Correctly()
        {
            // Arrange
            const bool addTrailingSlash = true;
            const bool convertUrlsToAscii = true;
            var charCollection = new List<IChar> { new CharItem { Char = "a", Replacement = "b" } };

            var builder = new RequestHandlerSettingsBuilder();

            // Act
            var requestHandlerSettings = builder
                .WithAddTrailingSlash(addTrailingSlash)
                .WithConvertUrlsToAscii(convertUrlsToAscii)
                .Build();

            // Assert
            Assert.AreEqual(addTrailingSlash, requestHandlerSettings.AddTrailingSlash);
            Assert.AreEqual(convertUrlsToAscii, requestHandlerSettings.ConvertUrlsToAscii);
            Assert.AreEqual("a-b", string.Join(",", requestHandlerSettings.CharCollection.Select(x => $"{x.Char}-{x.Replacement}")));
        }
    }
}
