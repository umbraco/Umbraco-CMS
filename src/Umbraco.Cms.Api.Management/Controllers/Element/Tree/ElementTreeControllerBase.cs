using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Element.Tree;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.Element}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Element))]
// TODO ELEMENTS: backoffice authorization policies
public class ElementTreeControllerBase : FolderTreeControllerBase<ElementTreeItemResponseModel>
{
    private readonly IUmbracoMapper _umbracoMapper;

    public ElementTreeControllerBase(IEntityService entityService, IUmbracoMapper umbracoMapper)
        : base(entityService)
        => _umbracoMapper = umbracoMapper;

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.Element;

    protected override UmbracoObjectTypes FolderObjectType => UmbracoObjectTypes.ElementContainer;

    protected override Ordering ItemOrdering
    {
        get
        {
            var ordering = Ordering.By(nameof(Infrastructure.Persistence.Dtos.NodeDto.NodeObjectType), Direction.Descending); // We need to override to change direction
            ordering.Next = Ordering.By(nameof(Infrastructure.Persistence.Dtos.NodeDto.Text));

            return ordering;
        }
    }

    protected override ElementTreeItemResponseModel[] MapTreeItemViewModels(Guid? parentKey, IEntitySlim[] entities)
        => entities.Select(entity =>
        {
            ElementTreeItemResponseModel responseModel = MapTreeItemViewModel(parentKey, entity);
            if (entity is IContentEntitySlim contentEntitySlim)
            {
                responseModel.HasChildren = entity.HasChildren;
                responseModel.ElementType = _umbracoMapper.Map<DocumentTypeReferenceResponseModel>(contentEntitySlim)!;
            }

            return responseModel;
        }).ToArray();
}
