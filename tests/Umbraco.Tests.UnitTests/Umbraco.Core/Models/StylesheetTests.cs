// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
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
        Assert.AreEqual(1, stylesheet.Properties.Count());
        Assert.AreEqual("Test", stylesheet.Properties.Single().Name);
        Assert.AreEqual("p", stylesheet.Properties.Single().Alias);
        Assert.AreEqual("font-weight:bold;" + Environment.NewLine + "font-family:Arial;", stylesheet.Properties.Single().Value);
    }

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
        var json = JsonConvert.SerializeObject(stylesheet);
        Debug.Print(json);
    }
}
