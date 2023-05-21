using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.BackOffice.ModelBinders;

internal class BlueprintItemBinder : ContentItemBinder
{
    private readonly IContentService _contentService;

    public BlueprintItemBinder(IJsonSerializer jsonSerializer, IUmbracoMapper umbracoMapper, IContentService contentService, IContentTypeService contentTypeService, IHostingEnvironment hostingEnvironment)
        : base(jsonSerializer, umbracoMapper, contentService, contentTypeService, hostingEnvironment) =>
        _contentService = contentService;

    protected override IContent? GetExisting(ContentItemSave model) => _contentService.GetBlueprintById(model.Id);
}
