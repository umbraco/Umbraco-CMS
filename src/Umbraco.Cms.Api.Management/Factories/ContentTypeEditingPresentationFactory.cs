using Umbraco.Cms.Core.Models;
using ContentTypeEditingModels = Umbraco.Cms.Core.Models.ContentTypeEditing;
using ContentTypeViewModels = Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.Factories;

internal abstract class ContentTypeEditingPresentationFactory
{
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

    private ContentTypeSort[] MapAllowedContentTypes(
        IEnumerable<ContentTypeViewModels.ContentTypeSort> allowedContentTypes)
    {
        // FIXME: we need to get rid of the integer ID in ContentTypeSort before we can implement this
        //        - see also FIXME in ContentTypeSort constructor
        return Array.Empty<ContentTypeSort>();
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
