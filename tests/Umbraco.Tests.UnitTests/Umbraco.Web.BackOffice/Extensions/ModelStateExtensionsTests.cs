// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.BackOffice.Extensions;

[TestFixture]
public class ModelStateExtensionsTests
{
    [Test]
    public void Get_Cultures_With_Errors()
    {
        var ms = new ModelStateDictionary();
        var localizationService = new Mock<ILocalizationService>();
        localizationService.Setup(x => x.GetDefaultLanguageIsoCode()).Returns("en-US");

        ms.AddPropertyError(new ValidationResult("no header image"), "headerImage", null); // invariant property
        ms.AddPropertyError(new ValidationResult("title missing"), "title", "en-US"); // variant property

        var result = ms.GetVariantsWithErrors("en-US");

        // even though there are 2 errors, they are both for en-US since that is the default language and one of the errors is for an invariant property
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("en-US", result[0].culture);

        ms = new ModelStateDictionary();
        ms.AddVariantValidationError("en-US", null, "generic culture error");

        result = ms.GetVariantsWithErrors("en-US");

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("en-US", result[0].culture);
    }

    [Test]
    public void Get_Cultures_With_Property_Errors()
    {
        var ms = new ModelStateDictionary();
        var localizationService = new Mock<ILocalizationService>();
        localizationService.Setup(x => x.GetDefaultLanguageIsoCode()).Returns("en-US");

        ms.AddPropertyError(new ValidationResult("no header image"), "headerImage", null); // invariant property
        ms.AddPropertyError(new ValidationResult("title missing"), "title", "en-US"); // variant property

        var result = ms.GetVariantsWithPropertyErrors("en-US");

        // even though there are 2 errors, they are both for en-US since that is the default language and one of the errors is for an invariant property
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("en-US", result[0].culture);
    }

    [Test]
    public void Add_Invariant_Property_Error()
    {
        var ms = new ModelStateDictionary();
        var localizationService = new Mock<ILocalizationService>();
        localizationService.Setup(x => x.GetDefaultLanguageIsoCode()).Returns("en-US");

        ms.AddPropertyError(new ValidationResult("no header image"), "headerImage", null); // invariant property

        Assert.AreEqual("_Properties.headerImage.invariant.null", ms.Keys.First());
    }

    [Test]
    public void Add_Variant_Property_Error()
    {
        var ms = new ModelStateDictionary();
        var localizationService = new Mock<ILocalizationService>();
        localizationService.Setup(x => x.GetDefaultLanguageIsoCode()).Returns("en-US");

        ms.AddPropertyError(new ValidationResult("no header image"), "headerImage", "en-US"); // variant property

        Assert.AreEqual("_Properties.headerImage.en-US.null", ms.Keys.First());
    }

    [Test]
    public void Add_Invariant_Segment_Property_Error()
    {
        var ms = new ModelStateDictionary();
        var localizationService = new Mock<ILocalizationService>();
        localizationService.Setup(x => x.GetDefaultLanguageIsoCode()).Returns("en-US");

        ms.AddPropertyError(new ValidationResult("no header image"), "headerImage", null, "mySegment"); // invariant/segment property

        Assert.AreEqual("_Properties.headerImage.invariant.mySegment", ms.Keys.First());
    }

    [Test]
    public void Add_Variant_Segment_Property_Error()
    {
        var ms = new ModelStateDictionary();
        var localizationService = new Mock<ILocalizationService>();
        localizationService.Setup(x => x.GetDefaultLanguageIsoCode()).Returns("en-US");

        ms.AddPropertyError(new ValidationResult("no header image"), "headerImage", "en-US", "mySegment"); // variant/segment property

        Assert.AreEqual("_Properties.headerImage.en-US.mySegment", ms.Keys.First());
    }

    [Test]
    public void Add_Invariant_Segment_Field_Property_Error()
    {
        var ms = new ModelStateDictionary();
        var localizationService = new Mock<ILocalizationService>();
        localizationService.Setup(x => x.GetDefaultLanguageIsoCode()).Returns("en-US");

        ms.AddPropertyError(new ValidationResult("no header image", new[] { "myField" }), "headerImage", null, "mySegment"); // invariant/segment property

        Assert.AreEqual("_Properties.headerImage.invariant.mySegment.myField", ms.Keys.First());
    }

    [Test]
    public void Add_Variant_Segment_Field_Property_Error()
    {
        var ms = new ModelStateDictionary();
        var localizationService = new Mock<ILocalizationService>();
        localizationService.Setup(x => x.GetDefaultLanguageIsoCode()).Returns("en-US");

        ms.AddPropertyError(new ValidationResult("no header image", new[] { "myField" }), "headerImage", "en-US", "mySegment"); // variant/segment property

        Assert.AreEqual("_Properties.headerImage.en-US.mySegment.myField", ms.Keys.First());
    }
}
