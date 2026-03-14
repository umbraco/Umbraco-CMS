// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

/// <summary>
/// Contains unit tests for the <see cref="Umbraco.Core.Models.Stylesheet"/> class, verifying its behavior and functionality.
/// </summary>
[TestFixture]
public class StylesheetTests
{
    /// <summary>
    /// Sets up the test environment before each test.
    /// </summary>
    [SetUp]
    public void SetUp() => _builder = new StylesheetBuilder();

    private StylesheetBuilder _builder;

    /// <summary>
    /// Tests that a Stylesheet can be created with the specified path and content.
    /// </summary>
    [Test]
    public void Can_Create_Stylesheet()
    {
        // Arrange
        // Act
        var stylesheet = _builder
            .WithPath("/css/styles.css")
            .WithContent(@"body { color:#000; } .bold {font-weight:bold;}")
            .Build();

        // Assert
        Assert.That(stylesheet.Name, Is.EqualTo("styles.css"));
        Assert.That(stylesheet.Alias, Is.EqualTo("styles"));
    }

    /// <summary>
    /// Tests that a property can be added to a stylesheet and verifies the property is correctly added.
    /// </summary>
    [Test]
    public void Can_Add_Property()
    {
        // Arrange
        var stylesheet = _builder
            .WithPath("/css/styles.css")
            .WithContent(@"body { color:#000; } .bold {font-weight:bold;}")
            .Build();

        // Act
        stylesheet.AddProperty(new StylesheetProperty("Test", "p", "font-weight:bold; font-family:Arial;"));

        // Assert
        Assert.AreEqual(1, stylesheet.Properties.Count());
        Assert.AreEqual("Test", stylesheet.Properties.Single().Name);
        Assert.AreEqual("p", stylesheet.Properties.Single().Alias);
        Assert.AreEqual("font-weight:bold;" + Environment.NewLine + "font-family:Arial;", stylesheet.Properties.Single().Value);
    }

    /// <summary>
    /// Tests that a property can be removed from the stylesheet.
    /// </summary>
    [Test]
    public void Can_Remove_Property()
    {
        // Arrange
        var stylesheet = _builder
            .WithPath("/css/styles.css")
            .WithContent(@"body { color:#000; } /**umb_name:Hello*/p{font-size:2em;} .bold {font-weight:bold;}")
            .Build();
        Assert.AreEqual(1, stylesheet.Properties.Count());

        // Act
        stylesheet.RemoveProperty("Hello");

        // Assert
        Assert.AreEqual(0, stylesheet.Properties.Count());
        Assert.AreEqual(@"body { color:#000; }  .bold {font-weight:bold;}", stylesheet.Content);
    }

    /// <summary>
    /// Verifies that updating a property (alias and value) of a stylesheet correctly modifies both the property and the stylesheet content.
    /// </summary>
    [Test]
    public void Can_Update_Property()
    {
        // Arrange
        var stylesheet = _builder
            .WithPath("/css/styles.css")
            .WithContent(@"body { color:#000; } /**umb_name:Hello*/p{font-size:2em;} .bold {font-weight:bold;}")
            .Build();

        // Act
        var prop = stylesheet.Properties.Single();
        prop.Alias = "li";
        prop.Value = "font-size:5em;";

        // - re-get
        prop = stylesheet.Properties.Single();

        // Assert
        Assert.AreEqual("li", prop.Alias);
        Assert.AreEqual("font-size:5em;", prop.Value);
        Assert.AreEqual(
            "body { color:#000; } /**umb_name:Hello*/" + Environment.NewLine + "li {" + Environment.NewLine +
            "\tfont-size:5em;" + Environment.NewLine + "} .bold {font-weight:bold;}",
            stylesheet.Content);
    }

    /// <summary>
    /// Tests that properties can be correctly retrieved from CSS content in a stylesheet.
    /// </summary>
    [Test]
    public void Can_Get_Properties_From_Css()
    {
        // Arrange
        var stylesheet = _builder
            .WithPath("/css/styles.css")
            .WithContent(
                @"body { color:#000; } .bold {font-weight:bold;} /**umb_name:Hello */ p { font-size: 1em; } /**umb_name:testing123*/ li:first-child {padding:0px;}")
            .Build();

        // Act
        var properties = stylesheet.Properties;

        // Assert
        Assert.AreEqual(2, properties.Count());
        Assert.AreEqual("Hello", properties.First().Name);
        Assert.AreEqual("font-size: 1em;", properties.First().Value);
        Assert.AreEqual("p", properties.First().Alias);
        Assert.AreEqual("testing123", properties.Last().Name);
        Assert.AreEqual("padding:0px;", properties.Last().Value);
        Assert.AreEqual("li:first-child", properties.Last().Alias);
    }

    /// <summary>
    /// Tests that a Stylesheet object can be serialized to JSON without throwing an error.
    /// </summary>
    [Test]
    public void Can_Serialize_Without_Error()
    {
        // Arrange
        var stylesheet = _builder
            .WithPath("/css/styles.css")
            .WithContent(@"@media screen and (min-width: 600px) and (min-width: 900px) {
                                    .class {
                                    background: #666;
                                    }
                                }")
            .Build();

        // Act
        var json = JsonSerializer.Serialize(stylesheet);
        Debug.Print(json);
    }
}
