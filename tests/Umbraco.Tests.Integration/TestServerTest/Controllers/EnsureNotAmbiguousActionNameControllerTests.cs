// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Web.BackOffice.Controllers;

namespace Umbraco.Cms.Tests.Integration.TestServerTest.Controllers;

[TestFixture]
public class EnsureNotAmbiguousActionNameControllerTests : UmbracoTestServerTestBase
{
    [Test]
    public void EnsureNotAmbiguousActionName()
    {
        var intId = 0;
        var guidId = Guid.Empty;
        var udiId = Udi.Create(Constants.UdiEntityType.Script, "test");

        Assert.Multiple(() =>
        {
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<ContentController>(x => x.GetById(intId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<ContentController>(x => x.GetById(guidId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<ContentController>(x => x.GetById(udiId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<ContentController>(x => x.GetNiceUrl(intId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<ContentController>(x => x.GetNiceUrl(guidId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<ContentController>(x => x.GetNiceUrl(udiId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<ContentController>(x => x.GetEmpty("test", 0)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<ContentController>(x =>
                x.GetChildren(intId, string.Empty, 0, 0, "SortOrder", Direction.Ascending, true, string.Empty, string.Empty)));

            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<ContentTypeController>(x => x.GetById(intId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<ContentTypeController>(x => x.GetById(guidId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<ContentTypeController>(x => x.GetById(udiId)));

            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<DataTypeController>(x => x.GetById(intId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<DataTypeController>(x => x.GetById(guidId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<DataTypeController>(x => x.GetById(udiId)));

            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<DictionaryController>(x => x.GetById(intId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<DictionaryController>(x => x.GetById(guidId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<DictionaryController>(x => x.GetById(udiId)));

            EnsureNotAmbiguousActionName(
                PrepareApiControllerUrl<EntityController>(x => x.GetPath(intId, UmbracoEntityTypes.Document)));
            EnsureNotAmbiguousActionName(
                PrepareApiControllerUrl<EntityController>(x => x.GetPath(guidId, UmbracoEntityTypes.Document)));
            EnsureNotAmbiguousActionName(
                PrepareApiControllerUrl<EntityController>(x => x.GetPath(udiId, UmbracoEntityTypes.Document)));
            EnsureNotAmbiguousActionName(
                PrepareApiControllerUrl<EntityController>(x => x.GetUrl(intId, UmbracoEntityTypes.Document, null)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<EntityController>(x => x.GetUrl(udiId, null)));
            EnsureNotAmbiguousActionName(
                PrepareApiControllerUrl<EntityController>(x => x.GetUrlAndAnchors(intId, null)));
            EnsureNotAmbiguousActionName(
                PrepareApiControllerUrl<EntityController>(x => x.GetUrlAndAnchors(udiId, null)));
            EnsureNotAmbiguousActionName(
                PrepareApiControllerUrl<EntityController>(x => x.GetById(intId, UmbracoEntityTypes.Document)));
            EnsureNotAmbiguousActionName(
                PrepareApiControllerUrl<EntityController>(x => x.GetById(guidId, UmbracoEntityTypes.Document)));
            EnsureNotAmbiguousActionName(
                PrepareApiControllerUrl<EntityController>(x => x.GetById(udiId, UmbracoEntityTypes.Document)));
            EnsureNotAmbiguousActionName(
                PrepareApiControllerUrl<EntityController>(x => x.GetByIds(new Guid[0], UmbracoEntityTypes.Document)));
            EnsureNotAmbiguousActionName(
                PrepareApiControllerUrl<EntityController>(x => x.GetByIds(new Udi[0], UmbracoEntityTypes.Document)));
            EnsureNotAmbiguousActionName(
                PrepareApiControllerUrl<EntityController>(x => x.GetByIds(new int[0], UmbracoEntityTypes.Document)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<EntityController>(x =>
                x.GetPagedChildren(intId, UmbracoEntityTypes.Document, 0, 1, "SortOrder", Direction.Ascending, string.Empty, null)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<EntityController>(x =>
                x.GetPagedChildren(guidId.ToString(), UmbracoEntityTypes.Document, 0, 1, "SortOrder", Direction.Ascending, string.Empty, null)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<EntityController>(x =>
                x.GetPagedChildren(udiId.ToString(), UmbracoEntityTypes.Document, 0, 1, "SortOrder", Direction.Ascending, string.Empty, null)));

            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<IconController>(x => x.GetIcon(string.Empty)));

            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MacrosController>(x => x.GetById(intId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MacrosController>(x => x.GetById(guidId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MacrosController>(x => x.GetById(udiId)));

            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MediaController>(x => x.GetById(intId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MediaController>(x => x.GetById(guidId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MediaController>(x => x.GetById(udiId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MediaController>(x =>
                x.GetChildren(intId, 0, 1, "SortOrder", Direction.Ascending, true, string.Empty)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MediaController>(x =>
                x.GetChildren(guidId, 0, 1, "SortOrder", Direction.Ascending, true, string.Empty)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MediaController>(x =>
                x.GetChildren(udiId, 0, 1, "SortOrder", Direction.Ascending, true, string.Empty)));

            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MediaTypeController>(x => x.GetById(intId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MediaTypeController>(x => x.GetById(guidId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MediaTypeController>(x => x.GetById(udiId)));
            EnsureNotAmbiguousActionName(
                PrepareApiControllerUrl<MediaTypeController>(x => x.GetAllowedChildren(intId)));
            EnsureNotAmbiguousActionName(
                PrepareApiControllerUrl<MediaTypeController>(x => x.GetAllowedChildren(guidId)));
            EnsureNotAmbiguousActionName(
                PrepareApiControllerUrl<MediaTypeController>(x => x.GetAllowedChildren(udiId)));

            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MemberGroupController>(x => x.GetById(intId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MemberGroupController>(x => x.GetById(guidId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MemberGroupController>(x => x.GetById(udiId)));

            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MemberTypeController>(x => x.GetById(intId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MemberTypeController>(x => x.GetById(guidId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MemberTypeController>(x => x.GetById(udiId)));

            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<RelationTypeController>(x => x.GetById(intId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<RelationTypeController>(x => x.GetById(guidId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<RelationTypeController>(x => x.GetById(udiId)));

            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<TemplateController>(x => x.GetById(intId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<TemplateController>(x => x.GetById(guidId)));
            EnsureNotAmbiguousActionName(PrepareApiControllerUrl<TemplateController>(x => x.GetById(udiId)));
        });
    }

    private void EnsureNotAmbiguousActionName(string url) =>
        Assert.DoesNotThrowAsync(async () => await Client.GetAsync(url));
}
