// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Services;

[TestFixture]
public class LocalizedTextServiceTests
{
    private static readonly ILoggerFactory s_loggerFactory = NullLoggerFactory.Instance;

    [Test]
    public void Using_Dictionary_Gets_All_Stored_Values()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        var txtService = new LocalizedTextService(
            new Dictionary<CultureInfo, Lazy<IDictionary<string, IDictionary<string, string>>>>
            {
                {
                    culture,
                    new Lazy<IDictionary<string, IDictionary<string, string>>>(() =>
                        new Dictionary<string, IDictionary<string, string>>
                        {
                            {
                                "testArea1",
                                new Dictionary<string, string> { { "testKey1", "testValue1" }, { "testKey2", "testValue2" } }
                            },
                            {
                                "testArea2",
                                new Dictionary<string, string> { { "blah1", "blahValue1" }, { "blah2", "blahValue2" } }
                            },
                        })
                },
            },
            s_loggerFactory.CreateLogger<LocalizedTextService>());

        var result = txtService.GetAllStoredValues(culture);

        Assert.AreEqual(4, result.Count);
        Assert.AreEqual("testArea1/testKey1", result.ElementAt(0).Key);
        Assert.AreEqual("testArea1/testKey2", result.ElementAt(1).Key);
        Assert.AreEqual("testArea2/blah1", result.ElementAt(2).Key);
        Assert.AreEqual("testArea2/blah2", result.ElementAt(3).Key);
        Assert.AreEqual("testValue1", result["testArea1/testKey1"]);
        Assert.AreEqual("testValue2", result["testArea1/testKey2"]);
        Assert.AreEqual("blahValue1", result["testArea2/blah1"]);
        Assert.AreEqual("blahValue2", result["testArea2/blah2"]);
    }

    [Test]
    public void Using_XDocument_Gets_All_Stored_Values()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        var txtService = new LocalizedTextService(
            new Dictionary<CultureInfo, Lazy<XDocument>>
            {
                {
                    culture, new Lazy<XDocument>(() => new XDocument(
                        new XElement(
                            "language",
                            new XElement(
                                "area",
                                new XAttribute("alias", "testArea1"),
                                new XElement("key", new XAttribute("alias", "testKey1"), "testValue1"),
                                new XElement("key", new XAttribute("alias", "testKey2"), "testValue2")),
                            new XElement(
                                "area",
                                new XAttribute("alias", "testArea2"),
                                new XElement("key", new XAttribute("alias", "blah1"), "blahValue1"),
                                new XElement("key", new XAttribute("alias", "blah2"), "blahValue2")))))
                },
            },
            s_loggerFactory.CreateLogger<LocalizedTextService>());

        var result = txtService.GetAllStoredValues(culture);

        Assert.AreEqual(4, result.Count());
        Assert.AreEqual("testArea1/testKey1", result.ElementAt(0).Key);
        Assert.AreEqual("testArea1/testKey2", result.ElementAt(1).Key);
        Assert.AreEqual("testArea2/blah1", result.ElementAt(2).Key);
        Assert.AreEqual("testArea2/blah2", result.ElementAt(3).Key);
        Assert.AreEqual("testValue1", result["testArea1/testKey1"]);
        Assert.AreEqual("testValue2", result["testArea1/testKey2"]);
        Assert.AreEqual("blahValue1", result["testArea2/blah1"]);
        Assert.AreEqual("blahValue2", result["testArea2/blah2"]);
    }

    [Test]
    public void Using_XDocument_Gets_All_Stored_Values_With_Duplicates()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        var txtService = new LocalizedTextService(
            new Dictionary<CultureInfo, Lazy<XDocument>>
            {
                {
                    culture, new Lazy<XDocument>(() => new XDocument(
                        new XElement(
                            "language",
                            new XElement(
                                "area",
                                new XAttribute("alias", "testArea1"),
                                new XElement("key", new XAttribute("alias", "testKey1"), "testValue1"),
                                new XElement("key", new XAttribute("alias", "testKey1"), "testValue1")))))
                },
            },
            s_loggerFactory.CreateLogger<LocalizedTextService>());

        var result = txtService.GetAllStoredValues(culture);

        Assert.AreEqual(1, result.Count());
    }

    [Test]
    public void Using_Dictionary_Returns_Text_With_Area()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        var txtService = new LocalizedTextService(
            new Dictionary<CultureInfo, Lazy<IDictionary<string, IDictionary<string, string>>>>
            {
                {
                    culture,
                    new Lazy<IDictionary<string, IDictionary<string, string>>>(() =>
                        new Dictionary<string, IDictionary<string, string>>
                        {
                            { "testArea", new Dictionary<string, string> { { "testKey", "testValue" } } },
                        })
                },
            },
            s_loggerFactory.CreateLogger<LocalizedTextService>());

        var result = txtService.Localize("testArea/testKey", culture);

        Assert.AreEqual("testValue", result);
    }

    [Test]
    public void Using_Dictionary_Returns_Text_Without_Area()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        var txtService = new LocalizedTextService(
            new Dictionary<CultureInfo, Lazy<IDictionary<string, IDictionary<string, string>>>>
            {
                {
                    culture,
                    new Lazy<IDictionary<string, IDictionary<string, string>>>(() =>
                        new Dictionary<string, IDictionary<string, string>>
                        {
                            { "testArea", new Dictionary<string, string> { { "testKey", "testValue" } } },
                        })
                },
            },
            s_loggerFactory.CreateLogger<LocalizedTextService>());

        var result = txtService.Localize("testKey", culture);

        Assert.AreEqual("testValue", result);
    }

    [Test]
    public void Using_Dictionary_Returns_Default_Text_When_Not_Found_With_Area()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        var txtService = new LocalizedTextService(
            new Dictionary<CultureInfo, Lazy<IDictionary<string, IDictionary<string, string>>>>
            {
                {
                    culture,
                    new Lazy<IDictionary<string, IDictionary<string, string>>>(() =>
                        new Dictionary<string, IDictionary<string, string>>
                        {
                            { "testArea", new Dictionary<string, string> { { "testKey", "testValue" } } },
                        })
                },
            },
            s_loggerFactory.CreateLogger<LocalizedTextService>());

        var result = txtService.Localize("testArea/doNotFind", culture);

        // NOTE: Based on how legacy works, the default text does not contain the area, just the key
        Assert.AreEqual("[doNotFind]", result);
    }

    [Test]
    public void Using_Dictionary_Returns_Default_Text_When_Not_Found_Without_Area()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        var txtService = new LocalizedTextService(
            new Dictionary<CultureInfo, Lazy<IDictionary<string, IDictionary<string, string>>>>
            {
                {
                    culture,
                    new Lazy<IDictionary<string, IDictionary<string, string>>>(() =>
                        new Dictionary<string, IDictionary<string, string>>
                        {
                            { "testArea", new Dictionary<string, string> { { "testKey", "testValue" } } },
                        })
                },
            },
            s_loggerFactory.CreateLogger<LocalizedTextService>());

        var result = txtService.Localize("doNotFind", culture);

        Assert.AreEqual("[doNotFind]", result);
    }

    [Test]
    public void Using_Dictionary_Returns_Tokenized_Text()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        var txtService = new LocalizedTextService(
            new Dictionary<CultureInfo, Lazy<IDictionary<string, IDictionary<string, string>>>>
            {
                {
                    culture,
                    new Lazy<IDictionary<string, IDictionary<string, string>>>(() =>
                        new Dictionary<string, IDictionary<string, string>>
                        {
                            {
                                "testArea",
                                new Dictionary<string, string> { { "testKey", "Hello %0%, you are such a %1% %2%" } }
                            },
                        })
                },
            },
            s_loggerFactory.CreateLogger<LocalizedTextService>());

        var result = txtService.Localize(
            "testKey",
            culture,
            new Dictionary<string, string> { { "0", "world" }, { "1", "great" }, { "2", "planet" } });

        Assert.AreEqual("Hello world, you are such a great planet", result);
    }

    [Test]
    public void Using_XDocument_Returns_Text_With_Area()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        var txtService = new LocalizedTextService(
            new Dictionary<CultureInfo, Lazy<XDocument>>
            {
                {
                    culture, new Lazy<XDocument>(() => new XDocument(
                        new XElement("area", new XAttribute("alias", "testArea"), new XElement("key", new XAttribute("alias", "testKey"), "testValue"))))
                },
            },
            s_loggerFactory.CreateLogger<LocalizedTextService>());

        var result = txtService.Localize("testArea/testKey", culture);

        Assert.AreEqual("testValue", result);
    }

    [Test]
    public void Using_XDocument_Returns_Text_Without_Area()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        var txtService = new LocalizedTextService(
            new Dictionary<CultureInfo, Lazy<XDocument>>
            {
                {
                    culture, new Lazy<XDocument>(() => new XDocument(
                        new XElement("area", new XAttribute("alias", "testArea"), new XElement("key", new XAttribute("alias", "testKey"), "testValue"))))
                },
            },
            s_loggerFactory.CreateLogger<LocalizedTextService>());

        var result = txtService.Localize("testKey", culture);

        Assert.AreEqual("testValue", result);
    }

    [Test]
    public void Using_XDocument_Returns_Default_Text_When_Not_Found_With_Area()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        var txtService = new LocalizedTextService(
            new Dictionary<CultureInfo, Lazy<XDocument>>
            {
                {
                    culture, new Lazy<XDocument>(() => new XDocument(
                        new XElement("area", new XAttribute("alias", "testArea"), new XElement("key", new XAttribute("alias", "testKey"), "testValue"))))
                },
            },
            s_loggerFactory.CreateLogger<LocalizedTextService>());

        var result = txtService.Localize("testArea/doNotFind", culture);

        // NOTE: Based on how legacy works, the default text does not contain the area, just the key
        Assert.AreEqual("[doNotFind]", result);
    }

    [Test]
    public void Using_XDocument_Returns_Default_Text_When_Not_Found_Without_Area()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        var txtService = new LocalizedTextService(
            new Dictionary<CultureInfo, Lazy<XDocument>>
            {
                {
                    culture, new Lazy<XDocument>(() => new XDocument(
                        new XElement("area", new XAttribute("alias", "testArea"), new XElement("key", new XAttribute("alias", "testKey"), "testValue"))))
                },
            },
            s_loggerFactory.CreateLogger<LocalizedTextService>());

        var result = txtService.Localize("doNotFind", culture);

        Assert.AreEqual("[doNotFind]", result);
    }

    [Test]
    public void Using_XDocument_Returns_Tokenized_Text()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        var txtService = new LocalizedTextService(
            new Dictionary<CultureInfo, Lazy<XDocument>>
            {
                {
                    culture, new Lazy<XDocument>(() => new XDocument(
                        new XElement(
                            "area",
                            new XAttribute("alias", "testArea"),
                            new XElement("key", new XAttribute("alias", "testKey"), "Hello %0%, you are such a %1% %2%"))))
                },
            },
            s_loggerFactory.CreateLogger<LocalizedTextService>());

        var result = txtService.Localize("testKey", culture, new Dictionary<string, string> { { "0", "world" }, { "1", "great" }, { "2", "planet" } });

        Assert.AreEqual("Hello world, you are such a great planet", result);
    }

    [Test]
    public void Using_Dictionary_Returns_Default_Text__When_No_Culture_Found()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        var txtService = new LocalizedTextService(
            new Dictionary<CultureInfo, Lazy<IDictionary<string, IDictionary<string, string>>>>
            {
                {
                    culture,
                    new Lazy<IDictionary<string, IDictionary<string, string>>>(() =>
                        new Dictionary<string, IDictionary<string, string>>
                        {
                            { "testArea", new Dictionary<string, string> { { "testKey", "testValue" } } },
                        })
                },
            },
            s_loggerFactory.CreateLogger<LocalizedTextService>());

        Assert.AreEqual("[testKey]", txtService.Localize("testArea/testKey", CultureInfo.GetCultureInfo("en-AU")));
    }

    [Test]
    public void Using_XDocument_Returns_Default_Text_When_No_Culture_Found()
    {
        var culture = CultureInfo.GetCultureInfo("en-US");
        var txtService = new LocalizedTextService(
            new Dictionary<CultureInfo, Lazy<XDocument>>
            {
                {
                    culture, new Lazy<XDocument>(() => new XDocument(
                        new XElement("area", new XAttribute("alias", "testArea"), new XElement("key", new XAttribute("alias", "testKey"), "testValue"))))
                },
            },
            s_loggerFactory.CreateLogger<LocalizedTextService>());

        Assert.AreEqual("[testKey]", txtService.Localize("testArea/testKey", CultureInfo.GetCultureInfo("en-AU")));
    }
}
