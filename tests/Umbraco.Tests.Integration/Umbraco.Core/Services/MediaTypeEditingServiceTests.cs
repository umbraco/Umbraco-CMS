using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using PropertyTypeValidation = Umbraco.Cms.Core.Models.ContentTypeEditing.PropertyTypeValidation;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
/// Tests for the media type editing service. Please notice that a lot of functional test is covered by the content type
/// editing service tests, since these services share the same base implementation.
/// </summary>
[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true)]
public partial class MediaTypeEditingServiceTests : UmbracoIntegrationTest
{
    private IMediaTypeEditingService MediaTypeEditingService => GetRequiredService<IMediaTypeEditingService>();

    private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    private const string TabContainerType = "Tab";

    private const string GroupContainerType = "Group";

    private MediaTypeCreateModel CreateCreateModel(
        string name = "Test",
        string? alias = null,
        Guid? key = null,
        Guid? parentKey = null,
        IEnumerable<MediaTypePropertyTypeModel>? propertyTypes = null,
        IEnumerable<MediaTypePropertyContainerModel>? containers = null,
        IEnumerable<Composition>? compositions = null) =>
        new()
        {
            Name = name,
            Alias = alias ?? ShortStringHelper.CleanStringForSafeAlias(name),
            Key = key ?? Guid.NewGuid(),
            ParentKey = parentKey,
            Properties = propertyTypes ?? Enumerable.Empty<MediaTypePropertyTypeModel>(),
            Containers = containers ?? Enumerable.Empty<MediaTypePropertyContainerModel>(),
            Compositions = compositions ?? Enumerable.Empty<Composition>(),
        };

    private MediaTypePropertyTypeModel CreatePropertyType(
        string name = "Title",
        string? alias = null,
        Guid? key = null,
        Guid? containerKey = null,
        Guid? dataTypeKey = null) =>
        new()
        {
            Name = name,
            Alias = alias ?? ShortStringHelper.CleanStringForSafeAlias(name),
            Key = key ?? Guid.NewGuid(),
            ContainerKey = containerKey,
            DataTypeKey = dataTypeKey ?? Constants.DataTypes.Guids.TextstringGuid,
            Validation = new PropertyTypeValidation(),
            Appearance = new PropertyTypeAppearance(),
        };

    private MediaTypePropertyContainerModel CreateContainer(
        string name = "Container",
        string type = TabContainerType,
        Guid? key = null) =>
        new()
        {
            Name = name,
            Type = type,
            Key = key ?? Guid.NewGuid(),
        };
}
