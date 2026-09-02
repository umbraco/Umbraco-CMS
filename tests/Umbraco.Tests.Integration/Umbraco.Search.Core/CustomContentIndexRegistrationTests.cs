using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Configuration;
using Umbraco.Cms.Search.Core.Services;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Testing.Search;
using CoreConstants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

public class CustomContentIndexRegistrationTests : ContentBaseTestBase
{
    private IContentService ContentService => GetRequiredService<IContentService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    private Guid ContentKey { get; } = Guid.NewGuid();

    private Guid MediaKey { get; } = Guid.NewGuid();

    private Guid MemberKey { get; } = Guid.NewGuid();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.Services.Configure<IndexOptions>(options =>
        {
            options.RegisterContentIndex<IIndexer, ISearcher, IDraftContentChangeStrategy>("My_Index", UmbracoObjectTypes.Document, UmbracoObjectTypes.Media, UmbracoObjectTypes.Member);
        });
    }

    [SetUp]
    public async Task SetupTest()
    {
        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("theContentType")
            .AddPropertyType()
            .WithAlias("title")
            .WithDataTypeId(CoreConstants.DataTypes.Textbox)
            .WithPropertyEditorAlias(CoreConstants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Build();
        await GetRequiredService<IContentTypeService>().CreateAsync(contentType, CoreConstants.Security.SuperUserKey);

        ContentService.Save(
            new ContentBuilder()
                .WithKey(ContentKey)
                .WithContentType(contentType)
                .WithName("The Content")
                .WithPropertyValues(
                    new
                    {
                        title = "The content title"
                    })
                .Build());

        IMediaType mediaType = new MediaTypeBuilder()
            .WithAlias("theMediaType")
            .AddPropertyGroup()
            .AddPropertyType()
            .WithAlias("altText")
            .WithDataTypeId(CoreConstants.DataTypes.Textbox)
            .WithPropertyEditorAlias(CoreConstants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Done()
            .Build();
        await GetRequiredService<IMediaTypeService>().CreateAsync(mediaType, CoreConstants.Security.SuperUserKey);

        MediaService.Save(
            new MediaBuilder()
                .WithKey(MediaKey)
                .WithMediaType(mediaType)
                .WithName("The Media")
                .WithPropertyValues(
                    new
                    {
                        altText = "The media alt text"
                    })
                .Build());

        IMemberType memberType = new MemberTypeBuilder()
            .WithAlias("theMemberType")
            .AddPropertyGroup()
            .AddPropertyType()
            .WithAlias("organization")
            .WithDataTypeId(CoreConstants.DataTypes.Textbox)
            .WithPropertyEditorAlias(CoreConstants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Done()
            .Build();
        await GetRequiredService<IMemberTypeService>().CreateAsync(memberType, CoreConstants.Security.SuperUserKey);

        MemberService.Save(
            new MemberBuilder()
                .WithKey(MemberKey)
                .WithMemberType(memberType)
                .WithName("The Member")
                .WithEmail("member@local")
                .WithLogin("member@local", "Test123456")
                .AddPropertyData()
                .WithKeyValue("organization", "The Organization")
                .Done()
                .Build());

        IndexerAndSearcher.Reset();
    }

    [Test]
    public void CustomIndexRegistration_CanContainAllTypesOfContent()
    {
        ContentService.Save(Content());
        MediaService.Save(Media());
        MemberService.Save(Member());

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump("My_Index");
        Assert.That(documents, Has.Count.EqualTo(3));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(ContentKey));
            Assert.That(documents[0].ObjectType, Is.EqualTo(UmbracoObjectTypes.Document));
            Assert.That(documents[1].Id, Is.EqualTo(MediaKey));
            Assert.That(documents[1].ObjectType, Is.EqualTo(UmbracoObjectTypes.Media));
            Assert.That(documents[2].Id, Is.EqualTo(MemberKey));
            Assert.That(documents[2].ObjectType, Is.EqualTo(UmbracoObjectTypes.Member));
        });

        Assert.Multiple(() =>
        {
            VerifyDocumentSystemValues(documents[0], Content(), []);
            VerifyDocumentSystemValues(documents[1], Media(), []);
            VerifyDocumentSystemValues(documents[2], Member(), []);
        });
    }

    private IContent Content() => ContentService.GetById(ContentKey) ?? throw new InvalidOperationException("Content was not found");

    private IMedia Media() => MediaService.GetById(MediaKey) ?? throw new InvalidOperationException("Media was not found");

    private IMember Member() => MemberService.GetById(MemberKey) ?? throw new InvalidOperationException("Member was not found");
}
