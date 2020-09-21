using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Tests.Common.Builders;

namespace Umbraco.Tests.UnitTests.Umbraco.Tests.Common.Builders
{
    [TestFixture]
    public class ContentSettingsBuilderTests
    {
        [Test]
        public void Is_Built_Correctly()
        {
            // Arrange
            const string autoFillImagePropertyAlias = "x";
            const string autoFillImagePropertyWidthFieldAlias = "w";
            const string autoFillImagePropertyHeightFieldAlias = "h";
            const string autoFillImagePropertyLengthFieldAlias = "l";
            const string autoFillImagePropertyExtensionFieldAlias = "e";
            const int errorPageContentId = 100;
            var errorPageContentKey = Guid.NewGuid();
            const string errorPageContentXPath = "/aaa/bbb";
            const string errorPageCulture = "en-GB";

            var builder = new ContentSettingsBuilder();

            // Act
            var contentSettings = builder
                .AddImaging()
                    .AddAutoFillImageProperty()
                        .WithAlias(autoFillImagePropertyAlias)
                        .WithWidthFieldAlias(autoFillImagePropertyWidthFieldAlias)
                        .WithHeightFieldAlias(autoFillImagePropertyHeightFieldAlias)
                        .WithLengthFieldAlias(autoFillImagePropertyLengthFieldAlias)
                        .WithExtensionFieldAlias(autoFillImagePropertyExtensionFieldAlias)
                        .Done()
                    .Done()
                .AddErrorPage()
                    .WithContentId(errorPageContentId)
                    .WithContentKey(errorPageContentKey)
                    .WithContentXPath(errorPageContentXPath)
                    .WithCulture(errorPageCulture)
                    .Done()
                .Build();

            // Assert
            Assert.AreEqual(1, contentSettings.Imaging.AutoFillImageProperties.Count());

            var autoFillImageProperty = contentSettings.Imaging.AutoFillImageProperties.First();
            Assert.AreEqual(autoFillImagePropertyAlias, autoFillImageProperty.Alias);
            Assert.AreEqual(autoFillImagePropertyWidthFieldAlias, autoFillImageProperty.WidthFieldAlias);
            Assert.AreEqual(autoFillImagePropertyHeightFieldAlias, autoFillImageProperty.HeightFieldAlias);
            Assert.AreEqual(autoFillImagePropertyLengthFieldAlias, autoFillImageProperty.LengthFieldAlias);
            Assert.AreEqual(autoFillImagePropertyExtensionFieldAlias, autoFillImageProperty.ExtensionFieldAlias);

            Assert.AreEqual(1, contentSettings.Error404Collection.Count());

            var errorPage = contentSettings.Error404Collection.First();
            Assert.AreEqual(errorPageContentId, errorPage.ContentId);
            Assert.AreEqual(errorPageContentKey, errorPage.ContentKey);
            Assert.AreEqual(errorPageContentXPath, errorPage.ContentXPath);
            Assert.AreEqual(errorPageCulture, errorPage.Culture);
        }
    }
}
