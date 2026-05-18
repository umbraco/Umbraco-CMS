// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core;

[TestFixture]
public class UdiEntityTypeHelperTests
{
    [TestCase(UmbracoObjectTypes.Document, Constants.UdiEntityType.Document)]
    [TestCase(UmbracoObjectTypes.DocumentBlueprint, Constants.UdiEntityType.DocumentBlueprint)]
    [TestCase(UmbracoObjectTypes.Media, Constants.UdiEntityType.Media)]
    [TestCase(UmbracoObjectTypes.Member, Constants.UdiEntityType.Member)]
    [TestCase(UmbracoObjectTypes.Template, Constants.UdiEntityType.Template)]
    [TestCase(UmbracoObjectTypes.DocumentType, Constants.UdiEntityType.DocumentType)]
    [TestCase(UmbracoObjectTypes.DocumentTypeContainer, Constants.UdiEntityType.DocumentTypeContainer)]
    [TestCase(UmbracoObjectTypes.MediaType, Constants.UdiEntityType.MediaType)]
    [TestCase(UmbracoObjectTypes.MediaTypeContainer, Constants.UdiEntityType.MediaTypeContainer)]
    [TestCase(UmbracoObjectTypes.DataType, Constants.UdiEntityType.DataType)]
    [TestCase(UmbracoObjectTypes.DataTypeContainer, Constants.UdiEntityType.DataTypeContainer)]
    [TestCase(UmbracoObjectTypes.MemberType, Constants.UdiEntityType.MemberType)]
    [TestCase(UmbracoObjectTypes.MemberGroup, Constants.UdiEntityType.MemberGroup)]
    [TestCase(UmbracoObjectTypes.RelationType, Constants.UdiEntityType.RelationType)]
    [TestCase(UmbracoObjectTypes.FormsForm, Constants.UdiEntityType.FormsForm)]
    [TestCase(UmbracoObjectTypes.FormsPreValue, Constants.UdiEntityType.FormsPreValue)]
    [TestCase(UmbracoObjectTypes.FormsDataSource, Constants.UdiEntityType.FormsDataSource)]
    [TestCase(UmbracoObjectTypes.Language, Constants.UdiEntityType.Language)]
    [TestCase(UmbracoObjectTypes.Element, Constants.UdiEntityType.Element)]
    [TestCase(UmbracoObjectTypes.ElementContainer, Constants.UdiEntityType.ElementContainer)]
    public void FromUmbracoObjectType_ReturnsExpectedEntityType(UmbracoObjectTypes umbracoObjectType, string expected)
    {
        string entityType = UdiEntityTypeHelper.FromUmbracoObjectType(umbracoObjectType);
        Assert.AreEqual(expected, entityType);
    }

    [TestCase(Constants.UdiEntityType.Document, UmbracoObjectTypes.Document)]
    [TestCase(Constants.UdiEntityType.DocumentBlueprint, UmbracoObjectTypes.DocumentBlueprint)]
    [TestCase(Constants.UdiEntityType.Media, UmbracoObjectTypes.Media)]
    [TestCase(Constants.UdiEntityType.Member, UmbracoObjectTypes.Member)]
    [TestCase(Constants.UdiEntityType.Template, UmbracoObjectTypes.Template)]
    [TestCase(Constants.UdiEntityType.DocumentType, UmbracoObjectTypes.DocumentType)]
    [TestCase(Constants.UdiEntityType.DocumentTypeContainer, UmbracoObjectTypes.DocumentTypeContainer)]
    [TestCase(Constants.UdiEntityType.MediaType, UmbracoObjectTypes.MediaType)]
    [TestCase(Constants.UdiEntityType.MediaTypeContainer, UmbracoObjectTypes.MediaTypeContainer)]
    [TestCase(Constants.UdiEntityType.DataType, UmbracoObjectTypes.DataType)]
    [TestCase(Constants.UdiEntityType.DataTypeContainer, UmbracoObjectTypes.DataTypeContainer)]
    [TestCase(Constants.UdiEntityType.MemberType, UmbracoObjectTypes.MemberType)]
    [TestCase(Constants.UdiEntityType.MemberGroup, UmbracoObjectTypes.MemberGroup)]
    [TestCase(Constants.UdiEntityType.RelationType, UmbracoObjectTypes.RelationType)]
    [TestCase(Constants.UdiEntityType.FormsForm, UmbracoObjectTypes.FormsForm)]
    [TestCase(Constants.UdiEntityType.FormsPreValue, UmbracoObjectTypes.FormsPreValue)]
    [TestCase(Constants.UdiEntityType.FormsDataSource, UmbracoObjectTypes.FormsDataSource)]
    [TestCase(Constants.UdiEntityType.Language, UmbracoObjectTypes.Language)]
    [TestCase(Constants.UdiEntityType.Element, UmbracoObjectTypes.Element)]
    [TestCase(Constants.UdiEntityType.ElementContainer, UmbracoObjectTypes.ElementContainer)]
    public void ToUmbracoObjectType_ReturnsExpectedObjectType(string entityType, UmbracoObjectTypes expected)
    {
        UmbracoObjectTypes umbracoObjectType = UdiEntityTypeHelper.ToUmbracoObjectType(entityType);
        Assert.AreEqual(expected, umbracoObjectType);
    }
}
