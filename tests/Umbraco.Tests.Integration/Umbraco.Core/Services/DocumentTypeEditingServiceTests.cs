﻿using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.ContentTypeEditing.Document;
using Umbraco.Cms.Core.Models.ContentTypeEditing.PropertyTypes;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using PropertyTypeValidation = Umbraco.Cms.Core.Models.ContentTypeEditing.PropertyTypes.PropertyTypeValidation;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true)]
public partial class DocumentTypeEditingServiceTests : UmbracoIntegrationTest
{
    private IDocumentTypeEditingService DocumentTypeEditingService => GetRequiredService<IDocumentTypeEditingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IShortStringHelper ShortStringHelper => GetRequiredService<IShortStringHelper>();

    private const string TabContainerType = "Tab";

    private const string GroupContainerType = "Group";

    private DocumentTypeCreateModel CreateCreateModel(
        string name = "Test",
        string? alias = null,
        Guid? key = null,
        bool isElement = false,
        Guid? parentKey = null,
        IEnumerable<DocumentPropertyTypeModel>? propertyTypes = null,
        IEnumerable<DocumentTypePropertyContainerModel>? containers = null,
        IEnumerable<Composition>? compositions = null) =>
        new()
        {
            Name = name,
            Alias = alias ?? ShortStringHelper.CleanStringForSafeAlias(name),
            Key = key ?? Guid.NewGuid(),
            Properties = propertyTypes ?? Enumerable.Empty<DocumentPropertyTypeModel>(),
            Containers = containers ?? Enumerable.Empty<DocumentTypePropertyContainerModel>(),
            Compositions = compositions ?? Enumerable.Empty<Composition>(),
            IsElement = isElement,
            ParentKey = parentKey,
        };

    private DocumentPropertyTypeModel CreatePropertyType(
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

    private DocumentTypePropertyContainerModel CreateContainer(
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
