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

[TestFixture]
public class UdiGetterExtensionsTests
{
    [TestCase(Constants.ObjectTypes.Strings.DataType, "6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://data-type-container/6ad82c70685c4e049b36d81bd779d16f")]
    [TestCase(Constants.ObjectTypes.Strings.DocumentType, "6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://document-type-container/6ad82c70685c4e049b36d81bd779d16f")]
    [TestCase(Constants.ObjectTypes.Strings.MediaType, "6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://media-type-container/6ad82c70685c4e049b36d81bd779d16f")]
    [TestCase(Constants.ObjectTypes.Strings.DocumentBlueprint, "6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://document-blueprint-container/6ad82c70685c4e049b36d81bd779d16f")]
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

    [TestCase("6ad82c70-685c-4e04-9b36-d81bd779d16f", "umb://media/6ad82c70685c4e049b36d81bd779d16f")]
    public void GetUdiForMedia(Guid key, string expected)
    {
        Media entity = new MediaBuilder()
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
