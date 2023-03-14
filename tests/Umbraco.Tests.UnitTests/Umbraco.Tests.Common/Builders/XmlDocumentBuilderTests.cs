// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Tests.Common.Builders;

[TestFixture]
public class XmlDocumentBuilderTests
{
    [Test]
    public void Is_Built_Correctly()
    {
        // Arrange
        const string content =
            @"<?xml version=""1.0"" encoding=""utf-8""?><root id=""-1""></root>";

        var builder = new XmlDocumentBuilder();

        // Act
        var xml = builder
            .WithContent(content)
            .Build();

        // Assert
        Assert.AreEqual(content, xml.OuterXml);
    }
}
