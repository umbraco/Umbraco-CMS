using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true,
    Boot = true)]
public class TrackRelationsTests : UmbracoIntegrationTestWithContent
{
    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private IMediaService MediaService => GetRequiredService<IMediaService>();

    private IRelationService RelationService => GetRequiredService<IRelationService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder.AddNuCache();
    }

    [Test]
    public void Automatically_Track_Relations()
    {
        var mt = MediaTypeBuilder.CreateSimpleMediaType("testMediaType", "Test Media Type");
        MediaTypeService.Save(mt);
        var m1 = MediaBuilder.CreateSimpleMedia(mt, "hello 1", -1);
        var m2 = MediaBuilder.CreateSimpleMedia(mt, "hello 1", -1);
        MediaService.Save(m1);
        MediaService.Save(m2);

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var ct = ContentTypeBuilder.CreateTextPageContentType("richTextTest", defaultTemplateId: template.Id);
        ct.AllowedTemplates = Enumerable.Empty<ITemplate>();

        ContentTypeService.Save(ct);

        var c1 = ContentBuilder.CreateTextpageContent(ct, "my content 1", -1);
        ContentService.Save(c1);

        var c2 = ContentBuilder.CreateTextpageContent(ct, "my content 2", -1);

        // 'bodyText' is a property with a RTE property editor which we knows tracks relations
        c2.Properties["bodyText"].SetValue(@"<p>
        <img src='/media/12312.jpg' data-udi='umb://media/" + m1.Key.ToString("N") + @"' />
</p><p><img src='/media/234234.jpg' data-udi=""umb://media/" + m2.Key.ToString("N") + @""" />
</p>
<p>
    <a href=""{locallink:umb://document/" + c1.Key.ToString("N") + @"}"">hello</a>
</p>");

        ContentService.Save(c2);

        var relations = RelationService.GetByParentId(c2.Id).ToList();
        Assert.AreEqual(3, relations.Count);
        Assert.AreEqual(Constants.Conventions.RelationTypes.RelatedMediaAlias, relations[0].RelationType.Alias);
        Assert.AreEqual(m1.Id, relations[0].ChildId);
        Assert.AreEqual(Constants.Conventions.RelationTypes.RelatedMediaAlias, relations[1].RelationType.Alias);
        Assert.AreEqual(m2.Id, relations[1].ChildId);
        Assert.AreEqual(Constants.Conventions.RelationTypes.RelatedDocumentAlias, relations[2].RelationType.Alias);
        Assert.AreEqual(c1.Id, relations[2].ChildId);
    }
}
