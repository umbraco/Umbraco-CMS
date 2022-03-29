// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.TestServerTest.Controllers
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
            string pathBase = string.Empty;

            Assert.Multiple(() =>
            {
                
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<ContentController>(x => x.GetById(intId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<ContentController>(x => x.GetById(guidId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<ContentController>(x => x.GetById(udiId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<ContentController>(x => x.GetNiceUrl(intId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<ContentController>(x => x.GetNiceUrl(guidId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<ContentController>(x => x.GetNiceUrl(udiId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<ContentController>(x => x.GetEmpty("test", 0), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<ContentController>(x => x.GetChildren(intId, string.Empty, 0, 0, "SortOrder", Direction.Ascending, true, string.Empty, string.Empty), pathBase));

                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<ContentTypeController>(x => x.GetById(intId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<ContentTypeController>(x => x.GetById(guidId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<ContentTypeController>(x => x.GetById(udiId), pathBase));

                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<DataTypeController>(x => x.GetById(intId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<DataTypeController>(x => x.GetById(guidId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<DataTypeController>(x => x.GetById(udiId), pathBase));

                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<DictionaryController>(x => x.GetById(intId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<DictionaryController>(x => x.GetById(guidId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<DictionaryController>(x => x.GetById(udiId), pathBase));

                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<EntityController>(x => x.GetPath(intId, UmbracoEntityTypes.Document), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<EntityController>(x => x.GetPath(guidId, UmbracoEntityTypes.Document), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<EntityController>(x => x.GetPath(udiId, UmbracoEntityTypes.Document), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<EntityController>(x => x.GetUrl(intId, UmbracoEntityTypes.Document, null), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<EntityController>(x => x.GetUrl(udiId, null), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<EntityController>(x => x.GetUrlAndAnchors(intId, null), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<EntityController>(x => x.GetUrlAndAnchors(udiId, null), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<EntityController>(x => x.GetById(intId, UmbracoEntityTypes.Document), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<EntityController>(x => x.GetById(guidId, UmbracoEntityTypes.Document), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<EntityController>(x => x.GetById(udiId, UmbracoEntityTypes.Document), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<EntityController>(x => x.GetByIds(new Guid[0], UmbracoEntityTypes.Document), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<EntityController>(x => x.GetByIds(new Udi[0], UmbracoEntityTypes.Document), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<EntityController>(x => x.GetByIds(new int[0], UmbracoEntityTypes.Document), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<EntityController>(x => x.GetPagedChildren(intId, UmbracoEntityTypes.Document, 0, 1, "SortOrder", Direction.Ascending, string.Empty, null), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<EntityController>(x => x.GetPagedChildren(guidId.ToString(), UmbracoEntityTypes.Document, 0, 1, "SortOrder", Direction.Ascending, string.Empty, null), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<EntityController>(x => x.GetPagedChildren(udiId.ToString(), UmbracoEntityTypes.Document, 0, 1, "SortOrder", Direction.Ascending, string.Empty, null), pathBase));

                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<IconController>(x => x.GetIcon(string.Empty), pathBase));

                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MacrosController>(x => x.GetById(intId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MacrosController>(x => x.GetById(guidId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MacrosController>(x => x.GetById(udiId), pathBase));

                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MediaController>(x => x.GetById(intId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MediaController>(x => x.GetById(guidId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MediaController>(x => x.GetById(udiId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MediaController>(x => x.GetChildren(intId, 0, 1, "SortOrder", Direction.Ascending, true, string.Empty), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MediaController>(x => x.GetChildren(guidId, 0, 1, "SortOrder", Direction.Ascending, true, string.Empty), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MediaController>(x => x.GetChildren(udiId, 0, 1, "SortOrder", Direction.Ascending, true, string.Empty), pathBase));

                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MediaTypeController>(x => x.GetById(intId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MediaTypeController>(x => x.GetById(guidId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MediaTypeController>(x => x.GetById(udiId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MediaTypeController>(x => x.GetAllowedChildren(intId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MediaTypeController>(x => x.GetAllowedChildren(guidId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MediaTypeController>(x => x.GetAllowedChildren(udiId), pathBase));

                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MemberGroupController>(x => x.GetById(intId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MemberGroupController>(x => x.GetById(guidId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MemberGroupController>(x => x.GetById(udiId), pathBase));

                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MemberTypeController>(x => x.GetById(intId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MemberTypeController>(x => x.GetById(guidId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<MemberTypeController>(x => x.GetById(udiId), pathBase));

                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<RelationTypeController>(x => x.GetById(intId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<RelationTypeController>(x => x.GetById(guidId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<RelationTypeController>(x => x.GetById(udiId), pathBase));

                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<TemplateController>(x => x.GetById(intId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<TemplateController>(x => x.GetById(guidId), pathBase));
                EnsureNotAmbiguousActionName(PrepareApiControllerUrl<TemplateController>(x => x.GetById(udiId), pathBase));
            });
        }

        private void EnsureNotAmbiguousActionName(string url) => Assert.DoesNotThrowAsync(async () => await Client.GetAsync(url));
    }
}
