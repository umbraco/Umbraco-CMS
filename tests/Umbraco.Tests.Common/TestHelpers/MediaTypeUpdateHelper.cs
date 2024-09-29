using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Common.TestHelpers;

public class MediaTypeUpdateHelper
{
    public MediaTypeUpdateModel CreateMediaTypeUpdateModel(IMediaType mediaType)
    {
        var updateModel = new MediaTypeUpdateModel();
        var model = MapBaseProperties<MediaTypeUpdateModel>(mediaType, updateModel);
        return model;
    }

    private T MapBaseProperties<T>(IMediaType mediaType, T model) where T : MediaTypeModelBase
    {
        model.Alias = mediaType.Alias;
        model.Name = mediaType.Name;
        model.Description = mediaType.Description;
        model.Icon = mediaType.Icon;
        model.AllowedAsRoot = mediaType.AllowedAsRoot;
        model.VariesByCulture = mediaType.VariesByCulture();
        model.VariesBySegment = mediaType.VariesBySegment();
        model.IsElement = mediaType.IsElement;
        model.ListView = mediaType.ListView;
        model.AllowedContentTypes = mediaType.AllowedContentTypes;

        var tempContainerList = model.Containers.ToList();

        foreach (var container in mediaType.PropertyGroups)
        {
            var containerModel = new MediaTypePropertyContainerModel()
            {
                Key = container.Key,
                Name = container.Name,
                SortOrder = container.SortOrder,
                Type = container.Type.ToString()
            };
            tempContainerList.Add(containerModel);
        }

        model.Containers = tempContainerList.AsEnumerable();

        var tempPropertyList = model.Properties.ToList();

        foreach (var propertyType in mediaType.PropertyTypes)
        {
            var propertyModel = new MediaTypePropertyTypeModel
            {
                Key = propertyType.Key,
                ContainerKey = mediaType.PropertyGroups.Single(x => x.PropertyTypes.Contains(propertyType)).Key,
                SortOrder = propertyType.SortOrder,
                Alias = propertyType.Alias,
                Name = propertyType.Name,
                Description = propertyType.Description,
                DataTypeKey = propertyType.DataTypeKey,
                VariesByCulture = propertyType.VariesByCulture(),
                VariesBySegment = propertyType.VariesBySegment(),
                Validation = new PropertyTypeValidation()
                {
                    Mandatory = propertyType.Mandatory,
                    MandatoryMessage = propertyType.ValidationRegExp,
                    RegularExpression = propertyType.ValidationRegExp,
                    RegularExpressionMessage = propertyType.ValidationRegExpMessage,
                },
                Appearance = new PropertyTypeAppearance() { LabelOnTop = propertyType.LabelOnTop, }
            };
            tempPropertyList.Add(propertyModel);
        }

        model.Properties = tempPropertyList.AsEnumerable();
        return model;
    }
}
