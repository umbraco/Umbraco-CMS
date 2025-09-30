using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Common.TestHelpers;

public static class ContentTypeUpdateHelper
{
    public static ContentTypeUpdateModel CreateContentTypeUpdateModel(IContentType contentType)
    {
        var updateModel = new ContentTypeUpdateModel();
        var model = MapBaseProperties<ContentTypeUpdateModel>(contentType, updateModel);
        return model;
    }

    private static T MapBaseProperties<T>(IContentType contentType, T model) where T : ContentTypeModelBase
    {
        model.Alias = contentType.Alias;
        model.Name = contentType.Name;
        model.Description = contentType.Description;
        model.Icon = contentType.Icon;
        model.AllowedAsRoot = contentType.AllowedAsRoot;
        model.VariesByCulture = contentType.VariesByCulture();
        model.VariesBySegment = contentType.VariesBySegment();
        model.IsElement = contentType.IsElement;
        model.ListView = contentType.ListView;
        model.Cleanup = new ContentTypeCleanup()
        {
            PreventCleanup = contentType.HistoryCleanup.PreventCleanup,
            KeepAllVersionsNewerThanDays = contentType.HistoryCleanup.KeepAllVersionsNewerThanDays,
            KeepLatestVersionPerDayForDays = contentType.HistoryCleanup.KeepLatestVersionPerDayForDays
        };

        model.AllowedTemplateKeys = contentType.AllowedTemplates.Select(x => x.Key);
        model.DefaultTemplateKey = contentType.DefaultTemplate?.Key;

        var tempContainerList = model.Containers.ToList();

        foreach (var container in contentType.PropertyGroups)
        {
            var containerModel = new ContentTypePropertyContainerModel()
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

        foreach (var propertyType in contentType.PropertyTypes)
        {
            var propertyModel = new ContentTypePropertyTypeModel
            {
                Key = propertyType.Key,
                ContainerKey = contentType.PropertyGroups.Single(x => x.PropertyTypes.Contains(propertyType)).Key,
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
