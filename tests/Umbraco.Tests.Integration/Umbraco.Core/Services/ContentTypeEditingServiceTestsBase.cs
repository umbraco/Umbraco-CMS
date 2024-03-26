using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true)]
public abstract class ContentTypeEditingServiceTestsBase : UmbracoIntegrationTest
{
    protected IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    protected IMediaTypeEditingService MediaTypeEditingService => GetRequiredService<IMediaTypeEditingService>();

    protected IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    protected IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    protected const string TabContainerType = "Tab";

    protected const string GroupContainerType = "Group";

    protected ContentTypeCreateModel ContentTypeCreateModel(
        string name = "Test",
        string? alias = null,
        Guid? key = null,
        bool isElement = false,
        Guid? containerKey = null,
        IEnumerable<ContentTypePropertyTypeModel>? propertyTypes = null,
        IEnumerable<ContentTypePropertyContainerModel>? containers = null,
        IEnumerable<Composition>? compositions = null)
    {
        var model = CreateContentEditingModel<ContentTypeCreateModel, ContentTypePropertyTypeModel, ContentTypePropertyContainerModel>(
                name,
                alias,
                isElement,
                propertyTypes,
                containers,
                compositions);
        model.Key = key ?? Guid.NewGuid();
        model.Alias = alias ?? ShortStringHelper.CleanStringForSafeAlias(name);
        model.ContainerKey = containerKey;
        return model;
    }

    protected MediaTypeCreateModel MediaTypeCreateModel(
        string name = "Test",
        string? alias = null,
        Guid? key = null,
        bool isElement = false,
        Guid? containerKey = null,
        IEnumerable<MediaTypePropertyTypeModel>? propertyTypes = null,
        IEnumerable<MediaTypePropertyContainerModel>? containers = null,
        IEnumerable<Composition>? compositions = null)
    {
        var model = CreateContentEditingModel<MediaTypeCreateModel, MediaTypePropertyTypeModel, MediaTypePropertyContainerModel>(
                name,
                alias,
                isElement,
                propertyTypes,
                containers,
                compositions);
        model.Key = key ?? Guid.NewGuid();
        model.Alias = alias ?? ShortStringHelper.CleanStringForSafeAlias(name);
        model.ContainerKey = containerKey;
        return model;
    }

    protected ContentTypeUpdateModel ContentTypeUpdateModel(
        string name = "Test",
        string? alias = null,
        bool isElement = false,
        IEnumerable<ContentTypePropertyTypeModel>? propertyTypes = null,
        IEnumerable<ContentTypePropertyContainerModel>? containers = null,
        IEnumerable<Composition>? compositions = null)
        => CreateContentEditingModel<ContentTypeUpdateModel, ContentTypePropertyTypeModel, ContentTypePropertyContainerModel>(
            name,
            alias,
            isElement,
            propertyTypes,
            containers,
            compositions);

    protected MediaTypeUpdateModel MediaTypeUpdateModel(
        string name = "Test",
        string? alias = null,
        bool isElement = false,
        IEnumerable<MediaTypePropertyTypeModel>? propertyTypes = null,
        IEnumerable<MediaTypePropertyContainerModel>? containers = null,
        IEnumerable<Composition>? compositions = null)
        => CreateContentEditingModel<MediaTypeUpdateModel, MediaTypePropertyTypeModel, MediaTypePropertyContainerModel>(
            name,
            alias,
            isElement,
            propertyTypes,
            containers,
            compositions);

    protected ContentTypePropertyTypeModel ContentTypePropertyTypeModel(
        string name = "Title",
        string? alias = null,
        Guid? key = null,
        Guid? containerKey = null,
        Guid? dataTypeKey = null)
        => CreatePropertyType<ContentTypePropertyTypeModel>(name, alias, key, containerKey, dataTypeKey);

    protected MediaTypePropertyTypeModel MediaTypePropertyTypeModel(
        string name = "Title",
        string? alias = null,
        Guid? key = null,
        Guid? containerKey = null,
        Guid? dataTypeKey = null)
        => CreatePropertyType<MediaTypePropertyTypeModel>(name, alias, key, containerKey, dataTypeKey);

    protected ContentTypePropertyContainerModel ContentTypePropertyContainerModel(
        string name = "Container",
        string type = TabContainerType,
        Guid? key = null)
        => CreateContainer<ContentTypePropertyContainerModel>(name, type, key);

    protected MediaTypePropertyContainerModel MediaTypePropertyContainerModel(
        string name = "Container",
        string type = TabContainerType,
        Guid? key = null)
        => CreateContainer<MediaTypePropertyContainerModel>(name, type, key);

    protected TModel CreateContentEditingModel<TModel, TPropertyType, TPropertyTypeContainer>(
        string name,
        string? alias = null,
        bool isElement = false,
        IEnumerable<TPropertyType>? propertyTypes = null,
        IEnumerable<TPropertyTypeContainer>? containers = null,
        IEnumerable<Composition>? compositions = null)
        where TModel : ContentTypeEditingModelBase<TPropertyType, TPropertyTypeContainer>, new()
        where TPropertyType : PropertyTypeModelBase
        where TPropertyTypeContainer : PropertyTypeContainerModelBase
        => new()
        {
            Name = name,
            Alias = alias ?? ShortStringHelper.CleanStringForSafeAlias(name),
            Properties = propertyTypes ?? Enumerable.Empty<TPropertyType>(),
            Containers = containers ?? Enumerable.Empty<TPropertyTypeContainer>(),
            Compositions = compositions ?? Enumerable.Empty<Composition>(),
            IsElement = isElement
        };

    protected TModel CreatePropertyType<TModel>(
        string name,
        string? alias = null,
        Guid? key = null,
        Guid? containerKey = null,
        Guid? dataTypeKey = null)
        where TModel : PropertyTypeModelBase, new()
        => new()
        {
            Name = name,
            Alias = alias ?? ShortStringHelper.CleanStringForSafeAlias(name),
            Key = key ?? Guid.NewGuid(),
            ContainerKey = containerKey,
            DataTypeKey = dataTypeKey ?? Constants.DataTypes.Guids.TextstringGuid,
            Validation = new PropertyTypeValidation(),
            Appearance = new PropertyTypeAppearance(),
        };

    protected TModel CreateContainer<TModel>(
        string name,
        string type = TabContainerType,
        Guid? key = null)
        where TModel : PropertyTypeContainerModelBase, new()
        => new()
        {
            Name = name,
            Type = type,
            Key = key ?? Guid.NewGuid(),
        };
}
