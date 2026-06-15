// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class StylesheetTests
{
    [SetUp]
    public void SetUp() => _builder = new StylesheetBuilder();

    private StylesheetBuilder _builder;

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
        Assert.That(stylesheet.Properties.Count(), Is.EqualTo(1));
        Assert.That(stylesheet.Properties.Single().Name, Is.EqualTo("Test"));
        Assert.That(stylesheet.Properties.Single().Alias, Is.EqualTo("p"));
        Assert.That(stylesheet.Properties.Single().Value, Is.EqualTo("font-weight:bold;" + Environment.NewLine + "font-family:Arial;"));
    }

    [Test]
    public void Can_Remove_Property()
    {
        // Arrange
        var stylesheet = _builder
            .WithPath("/css/styles.css")
            .WithContent(@"body { color:#000; } /**umb_name:Hello*/p{font-size:2em;} .bold {font-weight:bold;}")
            .Build();
        Assert.That(stylesheet.Properties.Count(), Is.EqualTo(1));

        // Act
        stylesheet.RemoveProperty("Hello");

        // Assert
        Assert.That(stylesheet.Properties.Count(), Is.EqualTo(0));
        Assert.That(stylesheet.Content, Is.EqualTo(@"body { color:#000; }  .bold {font-weight:bold;}"));
    }

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
        Assert.That(prop.Alias, Is.EqualTo("li"));
        Assert.That(prop.Value, Is.EqualTo("font-size:5em;"));
        Assert.That(
            stylesheet.Content, Is.EqualTo("body { color:#000; } /**umb_name:Hello*/" + Environment.NewLine + "li {" + Environment.NewLine +
            "\tfont-size:5em;" + Environment.NewLine + "} .bold {font-weight:bold;}"));
    }

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
        Assert.That(properties.Count(), Is.EqualTo(2));
        Assert.That(properties.First().Name, Is.EqualTo("Hello"));
        Assert.That(properties.First().Value, Is.EqualTo("font-size: 1em;"));
        Assert.That(properties.First().Alias, Is.EqualTo("p"));
        Assert.That(properties.Last().Name, Is.EqualTo("testing123"));
        Assert.That(properties.Last().Value, Is.EqualTo("padding:0px;"));
        Assert.That(properties.Last().Alias, Is.EqualTo("li:first-child"));
    }

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
