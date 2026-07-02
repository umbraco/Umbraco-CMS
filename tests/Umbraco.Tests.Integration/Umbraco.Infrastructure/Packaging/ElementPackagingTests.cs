// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Packaging;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Packaging;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ElementPackagingTests : UmbracoIntegrationTest
{
    private IEntityXmlSerializer Serializer => GetRequiredService<IEntityXmlSerializer>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IElementService ElementService => GetRequiredService<IElementService>();

    private IElementContainerService ElementContainerService => GetRequiredService<IElementContainerService>();

    private CompiledPackageXmlParser CompiledPackageXmlParser => GetRequiredService<CompiledPackageXmlParser>();

    private IPackageDataInstallation PackageDataInstallation => GetRequiredService<IPackageDataInstallation>();

    [Test]
    public async Task Can_Serialize_Element_Type_With_AllowedInLibrary()
    {
        var elementType = ContentTypeBuilder.CreateSimpleElementType("element1", "Element 1");
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        var element = Serializer.Serialize(elementType);

        var info = element.Element("Info");
        Assert.That(info, Is.Not.Null);
        Assert.That(info!.Element("IsElement")!.Value, Is.EqualTo("True"));
        Assert.That(info.Element("AllowedInLibrary")!.Value, Is.EqualTo("True"));
    }

    [Test]
    public async Task Can_Serialize_Element_To_Expected_Xml()
    {
        var elementType = ContentTypeBuilder.CreateSimpleElementType("element2", "Element 2");
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        Element element = new Element("My Element", elementType);
        element.SetValue("title", "The Element Title");
        ElementService.Save(element);

        var xml = Serializer.Serialize(element);

        Assert.Multiple(() =>
        {
            Assert.That(xml.Name.LocalName, Is.EqualTo(elementType.Alias));
            Assert.That((string)xml.Attribute("key"), Is.EqualTo(element.Key.ToString()));
            Assert.That((string)xml.Attribute("nodeTypeAlias"), Is.EqualTo(elementType.Alias));
            Assert.That((string)xml.Attribute("nodeType"), Is.EqualTo(elementType.Id.ToString()));
            Assert.That((bool)xml.Attribute("isPublished"), Is.EqualTo(element.Published));
            Assert.That((string)xml.Attribute("isDoc"), Is.EqualTo(string.Empty));
            Assert.That(xml.Elements("title").Single().Value, Is.EqualTo("The Element Title"));
        });
    }

    [Test]
    public async Task Can_RoundTrip_Element_Through_Package()
    {
        // Arrange: create an element type and an element, then capture them as package XML.
        var elementType = ContentTypeBuilder.CreateSimpleElementType("element3", "Element 3");
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        Element element = new Element("My Element", elementType);
        element.SetValue("title", "The Element Title");
        ElementService.Save(element);

        var elementKey = element.Key;

        var packageXml = new XDocument(
            new XElement(
                "umbPackage",
                new XElement("info", new XElement("package", new XElement("name", "ElementPackage"))),
                new XElement("DocumentTypes", Serializer.Serialize(elementType)),
                new XElement(
                    "Elements",
                    new XElement(
                        "ElementSet",
                        new XAttribute("importMode", "root"),
                        Serializer.Serialize(element)))));

        // Remove the originals so the install genuinely re-creates them (import skips items whose key already exists).
        ElementService.Delete(element);
        ContentTypeService.Delete(elementType);

        Assert.That(ElementService.GetById(elementKey), Is.Null);

        // Act
        CompiledPackage compiledPackage = CompiledPackageXmlParser.ToCompiledPackage(packageXml);
#pragma warning disable CS0618
        InstallationSummary summary = PackageDataInstallation.InstallPackageData(compiledPackage, Constants.Security.SuperUserId);
#pragma warning restore CS0618

        // Assert
        Assert.Multiple(() =>
        {
            // Element types are installed as document types (they are content types with IsElement = true).
            var installedType = summary.DocumentTypesInstalled.SingleOrDefault();
            Assert.That(installedType, Is.Not.Null);
            Assert.That(installedType!.IsElement, Is.True);
            Assert.That(installedType.AllowedInLibrary, Is.True);

            var installedElement = summary.ElementsInstalled.SingleOrDefault();
            Assert.That(installedElement, Is.Not.Null);
            Assert.That(installedElement!.Key, Is.EqualTo(elementKey));
        });

        IElement reloaded = ElementService.GetById(elementKey);
        Assert.That(reloaded, Is.Not.Null);
        Assert.That(reloaded.GetValue<string>("title"), Is.EqualTo("The Element Title"));
    }

    [Test]
    public async Task Can_RoundTrip_Element_In_Nested_Container()
    {
        // Arrange: element type + two nested containers, with an element in the leaf container.
        var elementType = ContentTypeBuilder.CreateSimpleElementType("element4", "Element 4");
        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        EntityContainer containerA = (await ElementContainerService.CreateAsync(null, "FolderA", null, Constants.Security.SuperUserKey)).Result!;
        EntityContainer containerB = (await ElementContainerService.CreateAsync(null, "FolderB", containerA.Key, Constants.Security.SuperUserKey)).Result!;

        IElement element = new Element("My Element", containerB.Id, elementType);
        element.SetValue("title", "Nested Title");
        ElementService.Save(element);
        var elementKey = element.Key;

        // Reload so Level/Path (used to resolve the ancestor folders) are populated.
        element = ElementService.GetById(elementKey)!;
        XElement serialized = Serializer.Serialize(element);

        Assert.Multiple(() =>
        {
            Assert.That((string)serialized.Attribute("Folders"), Is.EqualTo("FolderA/FolderB"));
            Assert.That((string)serialized.Attribute("FolderKeys"), Is.EqualTo($"{containerA.Key}/{containerB.Key}"));
        });

        var packageXml = new XDocument(
            new XElement(
                "umbPackage",
                new XElement("info", new XElement("package", new XElement("name", "ElementPackage"))),
                new XElement("DocumentTypes", Serializer.Serialize(elementType)),
                new XElement(
                    "Elements",
                    new XElement(
                        "ElementSet",
                        new XAttribute("importMode", "root"),
                        serialized))));

        // Remove the originals (leaf-first) so the install genuinely re-creates them.
        ElementService.Delete(element);
        await ElementContainerService.DeleteAsync(containerB.Key, Constants.Security.SuperUserKey);
        await ElementContainerService.DeleteAsync(containerA.Key, Constants.Security.SuperUserKey);
        ContentTypeService.Delete(elementType);

        Assert.That(ElementService.GetById(elementKey), Is.Null);

        // Act
        CompiledPackage compiledPackage = CompiledPackageXmlParser.ToCompiledPackage(packageXml);
#pragma warning disable CS0618
        InstallationSummary summary = PackageDataInstallation.InstallPackageData(compiledPackage, Constants.Security.SuperUserId);
#pragma warning restore CS0618

        // Assert: both containers recreated and the element is placed back under the leaf container.
        IElement reloaded = ElementService.GetById(elementKey)!;
        Assert.That(reloaded, Is.Not.Null);
        Assert.That(reloaded.GetValue<string>("title"), Is.EqualTo("Nested Title"));

        EntityContainer? parent = await ElementContainerService.GetParentAsync(reloaded);
        Assert.Multiple(() =>
        {
            Assert.That(parent, Is.Not.Null);
            Assert.That(parent!.Name, Is.EqualTo("FolderB"));
            Assert.That(parent.Key, Is.EqualTo(containerB.Key));
            Assert.That(summary.EntityContainersInstalled.Count(), Is.EqualTo(2));
        });
    }
}
