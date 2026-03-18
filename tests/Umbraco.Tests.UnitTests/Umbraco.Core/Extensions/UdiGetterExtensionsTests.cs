// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

/// <summary>
/// Contains unit tests for the <see cref="UdiGetterExtensions"/> extension methods, verifying UDI (Unique Document Identifier) handling functionality.
/// </summary>
[TestFixture]
public class UdiGetterExtensionsTests
{
    /// <summary>
    /// Verifies that the UDI generated for an <see cref="EntityContainer"/> with a given object type and key matches the expected UDI string.
    /// This test also checks that both the concrete and interface implementations of <c>GetUdi()</c> return the correct value.
    /// </summary>
    /// <param name="containedObjectType">The GUID representing the type of object contained in the entity container (e.g., data type, document type).</param>
    /// <param name="key">The unique GUID key identifying the entity container instance.</param>
    /// <param name="expected">The expected string representation of the generated UDI.</param>
    [TestCase(Constants.ObjectTypes.Strings.DataType, "6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://data-type-container/6ad82c70685c4e049b36d81bd779d16f")]
    [TestCase(Constants.ObjectTypes.Strings.DocumentBlueprint, "6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://document-blueprint-container/6ad82c70685c4e049b36d81bd779d16f")]
    [TestCase(Constants.ObjectTypes.Strings.DocumentType, "6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://document-type-container/6ad82c70685c4e049b36d81bd779d16f")]
    [TestCase(Constants.ObjectTypes.Strings.MediaType, "6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://media-type-container/6ad82c70685c4e049b36d81bd779d16f")]
    [TestCase(Constants.ObjectTypes.Strings.MemberType, "6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://member-type-container/6ad82c70685c4e049b36d81bd779d16f")]
    public void GetUdiForEntityContainer(Guid containedObjectType, Guid key, string expected)
    {
        EntityContainer entity = new EntityContainer(containedObjectType)
        {
            Key = key
        };

        Udi udi = entity.GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IEntity)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());
    }

    [TestCase("6ad82c70-685c-4e04-9b36-d81bd779d16f", false, "umb://document/6ad82c70685c4e049b36d81bd779d16f")]
    [TestCase("6ad82c70-685c-4e04-9b36-d81bd779d16f", true, "umb://document-blueprint/6ad82c70685c4e049b36d81bd779d16f")]
    public void GetUdiForContent(Guid key, bool blueprint, string expected)
    {
        Content entity = new ContentBuilder()
            .WithKey(key)
            .WithBlueprint(blueprint)
            .WithContentType(ContentTypeBuilder.CreateBasicContentType())
            .Build();

        Udi udi = entity.GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IContentBase)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IEntity)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());
    }

    /// <summary>
    /// Tests that the Udi for a media entity is correctly generated based on the provided key.
    /// </summary>
    /// <param name="key">The unique identifier for the media entity.</param>
    /// <param name="expected">The expected Udi string representation.</param>
    [TestCase("6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://media/6ad82c70685c4e049b36d81bd779d16f")]
    public void GetUdiForMedia(Guid key, string expected)
    {
        global::Umbraco.Cms.Core.Models.Media entity = new MediaBuilder()
            .WithKey(key)
            .WithMediaType(MediaTypeBuilder.CreateImageMediaType())
            .Build();

        Udi udi = entity.GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IContentBase)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IEntity)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());
    }

    /// <summary>
    /// Verifies that the <see cref="Udi"/> for a member entity is correctly generated from the specified key,
    /// and that the result is consistent when accessed via <see cref="Member"/>, <see cref="IContentBase"/>, and <see cref="IEntity"/> interfaces.
    /// </summary>
    /// <param name="key">The unique identifier (GUID) for the member.</param>
    /// <param name="expected">The expected UDI string representation for the member.</param>
    [TestCase("6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://member/6ad82c70685c4e049b36d81bd779d16f")]
    public void GetUdiForMember(Guid key, string expected)
    {
        Member entity = new MemberBuilder()
            .WithKey(key)
            .WithMemberType(MemberTypeBuilder.CreateSimpleMemberType())
            .Build();

        Udi udi = entity.GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IContentBase)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IEntity)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());
    }

    /// <summary>
    /// Tests that the Udi for a content type is correctly generated from the provided key.
    /// </summary>
    /// <param name="key">The unique identifier for the content type.</param>
    /// <param name="expected">The expected Udi string representation.</param>
    [TestCase("6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://document-type/6ad82c70685c4e049b36d81bd779d16f")]
    public void GetUdiForContentType(Guid key, string expected)
    {
        IContentType entity = new ContentTypeBuilder()
            .WithKey(key)
            .Build();

        Udi udi = entity.GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IContentTypeComposition)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IEntity)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());
    }

    /// <summary>
    /// Verifies that the UDI (Umbraco Document Identifier) generated for a media type entity with the specified key
    /// matches the expected UDI string, regardless of whether the entity is accessed via IMediaType, IContentTypeComposition, or IEntity interfaces.
    /// </summary>
    /// <param name="key">The unique <see cref="Guid"/> identifier for the media type.</param>
    /// <param name="expected">The expected UDI string representation for the media type.</param>
    [TestCase("6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://media-type/6ad82c70685c4e049b36d81bd779d16f")]
    public void GetUdiForMediaType(Guid key, string expected)
    {
        IMediaType entity = new MediaTypeBuilder()
            .WithKey(key)
            .Build();

        Udi udi = entity.GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IContentTypeComposition)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IEntity)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());
    }

    /// <summary>
    /// Tests that the Udi for a member type is generated correctly based on the provided key.
    /// </summary>
    /// <param name="key">The unique identifier for the member type.</param>
    /// <param name="expected">The expected Udi string representation.</param>
    [TestCase("6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://member-type/6ad82c70685c4e049b36d81bd779d16f")]
    public void GetUdiForMemberType(Guid key, string expected)
    {
        IMemberType entity = new MemberTypeBuilder()
            .WithKey(key)
            .Build();

        Udi udi = entity.GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IContentTypeComposition)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IEntity)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());
    }

    /// <summary>
    /// Verifies that the UDI (Umbraco Distributed Identifier) generated for a data type entity with the specified key matches the expected UDI string.
    /// This test asserts the result for both the strongly-typed entity and its IEntity interface implementation.
    /// </summary>
    /// <param name="key">The unique identifier (GUID) of the data type to test.</param>
    /// <param name="expected">The expected UDI string representation for the data type.</param>
    [TestCase("6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://data-type/6ad82c70685c4e049b36d81bd779d16f")]
    public void GetUdiForDataType(Guid key, string expected)
    {
        DataType entity = new DataTypeBuilder()
            .WithKey(key)
            .Build();

        Udi udi = entity.GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IEntity)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());
    }

    /// <summary>
    /// Tests that the Udi for a dictionary item is correctly generated from the given key.
    /// </summary>
    /// <param name="key">The GUID key of the dictionary item.</param>
    /// <param name="expected">The expected Udi string representation.</param>
    [TestCase("6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://dictionary-item/6ad82c70685c4e049b36d81bd779d16f")]
    public void GetUdiForDictionaryItem(Guid key, string expected)
    {
        DictionaryItem entity = new DictionaryItemBuilder()
            .WithKey(key)
            .Build();

        Udi udi = entity.GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IEntity)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());
    }

    /// <summary>
    /// Tests that the Udi for a given language ISO code is correctly generated.
    /// </summary>
    /// <param name="isoCode">The ISO code of the language.</param>
    /// <param name="expected">The expected Udi string representation.</param>
    [TestCase("en-US", "umb://language/en-US")]
    [TestCase("en", "umb://language/en")]
    public void GetUdiForLanguage(string isoCode, string expected)
    {
        ILanguage entity = new LanguageBuilder()
            .WithCultureInfo(isoCode)
            .Build();

        Udi udi = entity.GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IEntity)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());
    }

    /// <summary>
    /// Verifies that the Udi for a <see cref="MemberGroup"/> is generated correctly from the specified key.
    /// The test asserts that both the concrete <c>GetUdi()</c> method and the <see cref="IEntity.GetUdi"/> implementation return the expected Udi string.
    /// </summary>
    /// <param name="key">The unique identifier (GUID) for the <see cref="MemberGroup"/>.</param>
    /// <param name="expected">The expected Udi string representation for the given key.</param>
    [TestCase("6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://member-group/6ad82c70685c4e049b36d81bd779d16f")]
    public void GetUdiForMemberGroup(Guid key, string expected)
    {
        MemberGroup entity = new MemberGroupBuilder()
            .WithKey(key)
            .Build();

        Udi udi = entity.GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IEntity)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());
    }

    /// <summary>
    /// Tests that the Udi returned for a partial view matches the expected Udi string.
    /// </summary>
    /// <param name="path">The path of the partial view.</param>
    /// <param name="expected">The expected Udi string representation.</param>
    [TestCase("test.cshtml", "umb://partial-view/test.cshtml")]
    [TestCase("editor\\test.cshtml", "umb://partial-view/editor/test.cshtml")]
    [TestCase("editor/test.cshtml", "umb://partial-view/editor/test.cshtml")]
    public void GetUdiForPartialView(string path, string expected)
    {
        IPartialView entity = new PartialViewBuilder()
            .WithPath(path)
            .Build();

        Udi udi = entity.GetUdi();
        Assert.AreEqual(expected, udi.ToString());
    }

    /// <summary>
    /// Verifies that the Udi generated for a relation entity with the specified key matches the expected Udi string.
    /// This test checks both the IRelation and IEntity interface implementations.
    /// </summary>
    /// <param name="key">The unique identifier (GUID) for the relation entity.</param>
    /// <param name="expected">The expected string representation of the Udi for the relation.</param>
    [TestCase("6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://relation/6ad82c70685c4e049b36d81bd779d16f")]
    public void GetUdiForRelation(Guid key, string expected)
    {
        IRelation entity = new RelationBuilder()
            .WithKey(key)
            .AddRelationType().Done()
            .Build();

        Udi udi = entity.GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IEntity)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());
    }

    /// <summary>
    /// Tests that the Udi returned for a given relation type key matches the expected Udi string.
    /// </summary>
    /// <param name="key">The GUID key of the relation type.</param>
    /// <param name="expected">The expected Udi string representation.</param>
    [TestCase("6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://relation-type/6ad82c70685c4e049b36d81bd779d16f")]
    public void GetUdiForRelationType(Guid key, string expected)
    {
        IRelationTypeWithIsDependency entity = new RelationTypeBuilder()
            .WithKey(key)
            .Build();

        Udi udi = entity.GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IEntity)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());
    }

    /// <summary>
    /// Tests that the Udi for a script is correctly generated from the given path.
    /// </summary>
    /// <param name="path">The path of the script.</param>
    /// <param name="expected">The expected Udi string representation.</param>
    [TestCase("script.js", "umb://script/script.js")]
    [TestCase("editor\\script.js", "umb://script/editor/script.js")]
    [TestCase("editor/script.js", "umb://script/editor/script.js")]
    public void GetUdiForScript(string path, string expected)
    {
        IScript entity = new ScriptBuilder()
            .WithPath(path)
            .Build();

        Udi udi = entity.GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IEntity)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());
    }

    /// <summary>
    /// Tests that the Udi for a stylesheet is correctly generated from the given path.
    /// </summary>
    /// <param name="path">The path of the stylesheet.</param>
    /// <param name="expected">The expected Udi string representation.</param>
    [TestCase("style.css", "umb://stylesheet/style.css")]
    [TestCase("editor\\style.css", "umb://stylesheet/editor/style.css")]
    [TestCase("editor/style.css", "umb://stylesheet/editor/style.css")]
    public void GetUdiForStylesheet(string path, string expected)
    {
        IStylesheet entity = new StylesheetBuilder()
            .WithPath(path)
            .Build();

        Udi udi = entity.GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IEntity)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());
    }

    /// <summary>
    /// Tests that the UDI generated for a template matches the expected UDI string.
    /// </summary>
    /// <param name="key">The unique identifier key of the template.</param>
    /// <param name="expected">The expected UDI string representation.</param>
    [TestCase("6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://template/6ad82c70685c4e049b36d81bd779d16f")]
    public void GetUdiForTemplate(Guid key, string expected)
    {
        ITemplate entity = new TemplateBuilder()
            .WithKey(key)
            .Build();

        Udi udi = entity.GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IEntity)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());
    }

    /// <summary>
    /// Verifies that the UDI (Unique Document Identifier) for a user entity is correctly generated from the provided key, both via the IUser interface and the IEntity interface.
    /// </summary>
    /// <param name="key">The unique identifier (GUID) of the user.</param>
    /// <param name="expected">The expected UDI string representation in the format 'umb://user/{key-without-dashes}'.</param>
    [TestCase("6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://user/6ad82c70685c4e049b36d81bd779d16f")]
    public void GetUdiForUser(Guid key, string expected)
    {
        IUser entity = new UserBuilder()
            .WithKey(key)
            .Build();

        Udi udi = entity.GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IEntity)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());
    }

    /// <summary>
    /// Verifies that the UDI (Unique Document Identifier) for a user group is correctly generated from the provided key.
    /// The test asserts that both the strongly-typed and IEntity interface implementations return the expected UDI string.
    /// </summary>
    /// <param name="key">The unique identifier (GUID) for the user group.</param>
    /// <param name="expected">The expected UDI string representation for the user group.</param>
    [TestCase("6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://user-group/6ad82c70685c4e049b36d81bd779d16f")]
    public void GetUdiForUserGroup(Guid key, string expected)
    {
        IUserGroup entity = new UserGroupBuilder()
            .WithKey(key)
            .Build();

        Udi udi = entity.GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IEntity)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());
    }

    /// <summary>
    /// Verifies that the Udi generated for a webhook entity with the specified key matches the expected string representation.
    /// The test checks both the direct entity and its IEntity interface implementation.
    /// </summary>
    /// <param name="key">The unique identifier (GUID) for the webhook entity.</param>
    /// <param name="expected">The expected Udi string representation for the webhook.</param>
    [TestCase("6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://webhook/6ad82c70685c4e049b36d81bd779d16f")]
    public void GetUdiForWebhook(Guid key, string expected)
    {
        Webhook entity = new WebhookBuilder()
            .WithKey(key)
            .Build();

        Udi udi = entity.GetUdi();
        Assert.AreEqual(expected, udi.ToString());

        udi = ((IEntity)entity).GetUdi();
        Assert.AreEqual(expected, udi.ToString());
    }
}
