using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using ContentTypeEditingModels = Umbraco.Cms.Core.Models.ContentTypeEditing;
using ContentTypeViewModels = Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.Factories;

internal abstract class ContentTypeEditingPresentationFactory
{
    private readonly IContentTypeService _contentTypeService;

    protected ContentTypeEditingPresentationFactory(IContentTypeService contentTypeService)
        => _contentTypeService = contentTypeService;

    protected TContentTypeEditingModel MapContentTypeEditingModel<
        TContentTypeEditingModel,
        TPropertyTypeEditingModel,
        TPropertyTypeContainerEditingModel,
        TPropertyTypeViewModel,
        TPropertyTypeContainerViewModel
    >(ContentTypeViewModels.ContentTypeModelBase<TPropertyTypeViewModel, TPropertyTypeContainerViewModel> viewModel)
            where TContentTypeEditingModel : ContentTypeEditingModels.ContentTypeEditingModelBase<TPropertyTypeEditingModel, TPropertyTypeContainerEditingModel>, new()
            where TPropertyTypeEditingModel : ContentTypeEditingModels.PropertyTypeModelBase, new()
            where TPropertyTypeContainerEditingModel : ContentTypeEditingModels.PropertyTypeContainerModelBase, new()
            where TPropertyTypeViewModel : ContentTypeViewModels.PropertyTypeModelBase
            where TPropertyTypeContainerViewModel : ContentTypeViewModels.PropertyTypeContainerModelBase
    {
        TContentTypeEditingModel editingModel = new()
        {
            Alias = viewModel.Alias,
            Description = viewModel.Description,
            Icon = viewModel.Icon,
            Name = viewModel.Name,
            IsElement = viewModel.IsElement,
            AllowedAsRoot = viewModel.AllowedAsRoot,
            VariesByCulture = viewModel.VariesByCulture,
            VariesBySegment = viewModel.VariesBySegment,
            Compositions = MapCompositions(viewModel.Compositions),
            Containers = MapContainers<TPropertyTypeContainerEditingModel>(viewModel.Containers),
            Properties = MapProperties<TPropertyTypeEditingModel>(viewModel.Properties),
            AllowedContentTypes = MapAllowedContentTypes(viewModel.AllowedContentTypes),
        };

        return editingModel;
    }

    private ContentTypeSort[] MapAllowedContentTypes(IEnumerable<ContentTypeViewModels.ContentTypeSort> allowedContentTypes)
    {
        // need to fetch the content type aliases to construct the corresponding ContentTypeSort entities
        ContentTypeViewModels.ContentTypeSort[] allowedContentTypesArray = allowedContentTypes as ContentTypeViewModels.ContentTypeSort[]
                                                                           ?? allowedContentTypes.ToArray();
        Guid[] contentTypeKeys = allowedContentTypesArray.Select(a => a.Id).ToArray();
        IDictionary<Guid, string> contentTypeAliasesByKey = _contentTypeService
            .GetAll()
            .Where(c => contentTypeKeys.Contains(c.Key))
            .ToDictionary(c => c.Key, c => c.Alias);

        return allowedContentTypesArray
            .Select(a =>
                contentTypeAliasesByKey.TryGetValue(a.Id, out var alias)
                    ? new ContentTypeSort(a.Id, a.SortOrder, alias)
                    : null)
            .WhereNotNull()
            .ToArray();
    }

    private TPropertyTypeEditingModel[] MapProperties<TPropertyTypeEditingModel>(
        IEnumerable<ContentTypeViewModels.PropertyTypeModelBase> properties)
        where TPropertyTypeEditingModel : ContentTypeEditingModels.PropertyTypeModelBase, new()
        => properties.Select(property => new TPropertyTypeEditingModel
        {
            Alias = property.Alias,
            Appearance =
                new ContentTypeEditingModels.PropertyTypeAppearance { LabelOnTop = property.Appearance.LabelOnTop },
            Name = property.Name,
            Validation = new ContentTypeEditingModels.PropertyTypeValidation
            {
                Mandatory = property.Validation.Mandatory,
                MandatoryMessage = property.Validation.MandatoryMessage,
                RegularExpression = property.Validation.RegEx,
                RegularExpressionMessage = property.Validation.RegExMessage
            },
            Description = property.Description,
            VariesBySegment = property.VariesBySegment,
            VariesByCulture = property.VariesByCulture,
            Key = property.Id,
            ContainerKey = property.ContainerId,
            SortOrder = property.SortOrder,
            DataTypeKey = property.DataTypeId,
        }).ToArray();

    private TPropertyTypeContainerEditingModel[] MapContainers<TPropertyTypeContainerEditingModel>(
        IEnumerable<ContentTypeViewModels.PropertyTypeContainerModelBase> containers)
        where TPropertyTypeContainerEditingModel : ContentTypeEditingModels.PropertyTypeContainerModelBase, new()
        => containers.Select(container => new TPropertyTypeContainerEditingModel
        {
            Type = container.Type,
            Key = container.Id,
            SortOrder = container.SortOrder,
            Name = container.Name,
            ParentKey = container.ParentId,
        }).ToArray();

    private ContentTypeEditingModels.Composition[] MapCompositions(IEnumerable<ContentTypeViewModels.ContentTypeComposition> compositions)
        => compositions.Select(composition => new ContentTypeEditingModels.Composition
        {
            Key = composition.Id,
            CompositionType = composition.CompositionType == ContentTypeViewModels.ContentTypeCompositionType.Inheritance
                ? ContentTypeEditingModels.CompositionType.Inheritance
                : ContentTypeEditingModels.CompositionType.Composition
        }).ToArray();
}
