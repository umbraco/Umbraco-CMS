using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using ContentTypeEditingModels = Umbraco.Cms.Core.Models.ContentTypeEditing;
using ContentTypeViewModels = Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.Factories;

internal abstract class ContentTypeEditingPresentationFactory<TContentType>
    where TContentType : IContentTypeComposition
{
    private readonly IContentTypeBaseService<TContentType> _contentTypeService;

    protected ContentTypeEditingPresentationFactory(IContentTypeBaseService<TContentType> contentTypeService)
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
            Containers = MapContainers<TPropertyTypeContainerEditingModel>(viewModel.Containers),
            Properties = MapProperties<TPropertyTypeEditingModel>(viewModel.Properties)
        };

        return editingModel;
    }

    protected T MapCompositionModel<T>(ContentTypeAvailableCompositionsResult compositionResult)
        where T : ContentTypeViewModels.AvailableContentTypeCompositionResponseModelBase, new()
    {
        IContentTypeComposition composition = compositionResult.Composition;
        IEnumerable<string>? folders = null;

        if (composition is TContentType contentType)
        {
            var containers = _contentTypeService.GetContainers(contentType);
            folders = containers.Select(c => c.Name).WhereNotNull();
        }

        T compositionModel = new()
        {
            Id = composition.Key,
            Name = composition.Name ?? string.Empty,
            Icon = composition.Icon ?? string.Empty,
            FolderPath = folders ?? Array.Empty<string>(),
            IsCompatible = compositionResult.Allowed
        };

        return compositionModel;
    }

    protected ContentTypeSort[] MapAllowedContentTypes(IDictionary<Guid, int> allowedContentTypesAndSortOrder)
    {
        // need to fetch the content type aliases to construct the corresponding ContentTypeSort entities
        IDictionary<Guid, string> contentTypeAliasesByKey = _contentTypeService
            .GetAll()
            .Where(c => allowedContentTypesAndSortOrder.Keys.Contains(c.Key))
            .ToDictionary(c => c.Key, c => c.Alias);

        return allowedContentTypesAndSortOrder
            .Select(a =>
                contentTypeAliasesByKey.TryGetValue(a.Key, out var alias)
                    ? new ContentTypeSort(a.Key, a.Value, alias)
                    : null)
            .WhereNotNull()
            .ToArray();
    }

    protected ContentTypeEditingModels.Composition[] MapCompositions(IDictionary<Guid, ContentTypeViewModels.CompositionType> compositions)
        => compositions.Select(composition => new ContentTypeEditingModels.Composition
        {
            Key = composition.Key,
            CompositionType = composition.Value == ContentTypeViewModels.CompositionType.Inheritance
                ? ContentTypeEditingModels.CompositionType.Inheritance
                : ContentTypeEditingModels.CompositionType.Composition
        }).ToArray();

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
            ContainerKey = property.Container?.Id,
            SortOrder = property.SortOrder,
            DataTypeKey = property.DataType.Id,
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
            ParentKey = container.Parent?.Id,
        }).ToArray();
}
