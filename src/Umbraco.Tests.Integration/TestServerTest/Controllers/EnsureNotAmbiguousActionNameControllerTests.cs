// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Tests.Integration.TestServerTest.Controllers
{
    [TestFixture]
    public class EnsureNotAmbiguousActionNameControllerTests : UmbracoTestServerTestBase
    {
        [Test]
        public void EnsureNotAmbiguousActionName()
        {
            var intId = 0;
            Guid guidId = Guid.Empty;
            var udiId = Udi.Create(Constants.UdiEntityType.Script, "test");

            Assert.Multiple(() =>
            {
                EnsureNotAmbiguousActionName(PrepareUrl<ContentController>(x => x.GetById(intId)));
                EnsureNotAmbiguousActionName(PrepareUrl<ContentController>(x => x.GetById(guidId)));
                EnsureNotAmbiguousActionName(PrepareUrl<ContentController>(x => x.GetById(udiId)));
                EnsureNotAmbiguousActionName(PrepareUrl<ContentController>(x => x.GetNiceUrl(intId)));
                EnsureNotAmbiguousActionName(PrepareUrl<ContentController>(x => x.GetNiceUrl(guidId)));
                EnsureNotAmbiguousActionName(PrepareUrl<ContentController>(x => x.GetNiceUrl(udiId)));
                EnsureNotAmbiguousActionName(PrepareUrl<ContentController>(x => x.GetEmpty("test", 0)));
                EnsureNotAmbiguousActionName(PrepareUrl<ContentController>(x => x.GetChildren(intId, string.Empty, 0, 0, "SortOrder", Direction.Ascending, true, string.Empty, string.Empty)));

                EnsureNotAmbiguousActionName(PrepareUrl<ContentTypeController>(x => x.GetById(intId)));
                EnsureNotAmbiguousActionName(PrepareUrl<ContentTypeController>(x => x.GetById(guidId)));
                EnsureNotAmbiguousActionName(PrepareUrl<ContentTypeController>(x => x.GetById(udiId)));

                EnsureNotAmbiguousActionName(PrepareUrl<DataTypeController>(x => x.GetById(intId)));
                EnsureNotAmbiguousActionName(PrepareUrl<DataTypeController>(x => x.GetById(guidId)));
                EnsureNotAmbiguousActionName(PrepareUrl<DataTypeController>(x => x.GetById(udiId)));

                EnsureNotAmbiguousActionName(PrepareUrl<DictionaryController>(x => x.GetById(intId)));
                EnsureNotAmbiguousActionName(PrepareUrl<DictionaryController>(x => x.GetById(guidId)));
                EnsureNotAmbiguousActionName(PrepareUrl<DictionaryController>(x => x.GetById(udiId)));

                EnsureNotAmbiguousActionName(PrepareUrl<EntityController>(x => x.GetPath(intId, UmbracoEntityTypes.Document)));
                EnsureNotAmbiguousActionName(PrepareUrl<EntityController>(x => x.GetPath(guidId, UmbracoEntityTypes.Document)));
                EnsureNotAmbiguousActionName(PrepareUrl<EntityController>(x => x.GetPath(udiId, UmbracoEntityTypes.Document)));
                EnsureNotAmbiguousActionName(PrepareUrl<EntityController>(x => x.GetUrl(intId, UmbracoEntityTypes.Document, null)));
                EnsureNotAmbiguousActionName(PrepareUrl<EntityController>(x => x.GetUrl(udiId, null)));
                EnsureNotAmbiguousActionName(PrepareUrl<EntityController>(x => x.GetUrlAndAnchors(intId, null)));
                EnsureNotAmbiguousActionName(PrepareUrl<EntityController>(x => x.GetUrlAndAnchors(udiId, null)));
                EnsureNotAmbiguousActionName(PrepareUrl<EntityController>(x => x.GetById(intId, UmbracoEntityTypes.Document)));
                EnsureNotAmbiguousActionName(PrepareUrl<EntityController>(x => x.GetById(guidId, UmbracoEntityTypes.Document)));
                EnsureNotAmbiguousActionName(PrepareUrl<EntityController>(x => x.GetById(udiId, UmbracoEntityTypes.Document)));
                EnsureNotAmbiguousActionName(PrepareUrl<EntityController>(x => x.GetByIds(new Guid[0], UmbracoEntityTypes.Document)));
                EnsureNotAmbiguousActionName(PrepareUrl<EntityController>(x => x.GetByIds(new Udi[0], UmbracoEntityTypes.Document)));
                EnsureNotAmbiguousActionName(PrepareUrl<EntityController>(x => x.GetByIds(new int[0], UmbracoEntityTypes.Document)));
                EnsureNotAmbiguousActionName(PrepareUrl<EntityController>(x => x.GetPagedChildren(intId, UmbracoEntityTypes.Document, 0, 1, "SortOrder", Direction.Ascending, string.Empty, null)));
                EnsureNotAmbiguousActionName(PrepareUrl<EntityController>(x => x.GetPagedChildren(guidId.ToString(), UmbracoEntityTypes.Document, 0, 1, "SortOrder", Direction.Ascending, string.Empty, null)));
                EnsureNotAmbiguousActionName(PrepareUrl<EntityController>(x => x.GetPagedChildren(udiId.ToString(), UmbracoEntityTypes.Document, 0, 1, "SortOrder", Direction.Ascending, string.Empty, null)));

                EnsureNotAmbiguousActionName(PrepareUrl<IconController>(x => x.GetIcon(string.Empty)));

                EnsureNotAmbiguousActionName(PrepareUrl<MacrosController>(x => x.GetById(intId)));
                EnsureNotAmbiguousActionName(PrepareUrl<MacrosController>(x => x.GetById(guidId)));
                EnsureNotAmbiguousActionName(PrepareUrl<MacrosController>(x => x.GetById(udiId)));

                EnsureNotAmbiguousActionName(PrepareUrl<MediaController>(x => x.GetById(intId)));
                EnsureNotAmbiguousActionName(PrepareUrl<MediaController>(x => x.GetById(guidId)));
                EnsureNotAmbiguousActionName(PrepareUrl<MediaController>(x => x.GetById(udiId)));
                EnsureNotAmbiguousActionName(PrepareUrl<MediaController>(x => x.GetChildren(intId, 0, 1, "SortOrder", Direction.Ascending, true, string.Empty)));
                EnsureNotAmbiguousActionName(PrepareUrl<MediaController>(x => x.GetChildren(guidId, 0, 1, "SortOrder", Direction.Ascending, true, string.Empty)));
                EnsureNotAmbiguousActionName(PrepareUrl<MediaController>(x => x.GetChildren(udiId, 0, 1, "SortOrder", Direction.Ascending, true, string.Empty)));

                EnsureNotAmbiguousActionName(PrepareUrl<MediaTypeController>(x => x.GetById(intId)));
                EnsureNotAmbiguousActionName(PrepareUrl<MediaTypeController>(x => x.GetById(guidId)));
                EnsureNotAmbiguousActionName(PrepareUrl<MediaTypeController>(x => x.GetById(udiId)));
                EnsureNotAmbiguousActionName(PrepareUrl<MediaTypeController>(x => x.GetAllowedChildren(intId)));
                EnsureNotAmbiguousActionName(PrepareUrl<MediaTypeController>(x => x.GetAllowedChildren(guidId)));
                EnsureNotAmbiguousActionName(PrepareUrl<MediaTypeController>(x => x.GetAllowedChildren(udiId)));

                EnsureNotAmbiguousActionName(PrepareUrl<MemberGroupController>(x => x.GetById(intId)));
                EnsureNotAmbiguousActionName(PrepareUrl<MemberGroupController>(x => x.GetById(guidId)));
                EnsureNotAmbiguousActionName(PrepareUrl<MemberGroupController>(x => x.GetById(udiId)));

                EnsureNotAmbiguousActionName(PrepareUrl<MemberTypeController>(x => x.GetById(intId)));
                EnsureNotAmbiguousActionName(PrepareUrl<MemberTypeController>(x => x.GetById(guidId)));
                EnsureNotAmbiguousActionName(PrepareUrl<MemberTypeController>(x => x.GetById(udiId)));

                EnsureNotAmbiguousActionName(PrepareUrl<RelationTypeController>(x => x.GetById(intId)));
                EnsureNotAmbiguousActionName(PrepareUrl<RelationTypeController>(x => x.GetById(guidId)));
                EnsureNotAmbiguousActionName(PrepareUrl<RelationTypeController>(x => x.GetById(udiId)));

                EnsureNotAmbiguousActionName(PrepareUrl<TemplateController>(x => x.GetById(intId)));
                EnsureNotAmbiguousActionName(PrepareUrl<TemplateController>(x => x.GetById(guidId)));
                EnsureNotAmbiguousActionName(PrepareUrl<TemplateController>(x => x.GetById(udiId)));
            });
        }

        private void EnsureNotAmbiguousActionName(string url) => Assert.DoesNotThrowAsync(async () => await Client.GetAsync(url));
    }
}
