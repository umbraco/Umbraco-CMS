// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.IO;
using NUnit.Framework;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Tests.Common.Builders;

[TestFixture]
public class StylesheetBuilderTests
{
    [Test]
    public void Is_Built_Correctly()
    {
        // Arrange
        var testPath = WebPath.PathSeparator + WebPath.Combine("css", "styles.css");
        const string testContent = @"body { color:#000; } .bold {font-weight:bold;}";

        var builder = new StylesheetBuilder();

        // Act
        var stylesheet = builder
            .WithPath(testPath)
            .WithContent(testContent)
            .Build();

        // Assert
        Assert.AreEqual(Path.DirectorySeparatorChar + Path.Combine("css", "styles.css"), stylesheet.Path);
        Assert.AreEqual(testContent, stylesheet.Content);
    }
}
