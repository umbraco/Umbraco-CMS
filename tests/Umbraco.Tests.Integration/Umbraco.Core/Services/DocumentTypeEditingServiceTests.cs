using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentTypeEditing.Document;
using Umbraco.Cms.Core.Models.ContentTypeEditing.PropertyTypes;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
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

    private DocumentPropertyType CreatePropertyType(
        string alias = "title",
        string name = "Title",
        Guid? key = null,
        Guid? containerKey = null,
        Guid? dataTypeKey = null) =>
        new()
        {
            Alias = alias,
            Name = name,
            Key = key ?? Guid.NewGuid(),
            ContainerKey = containerKey,
            DataTypeKey = dataTypeKey ?? Constants.DataTypes.Guids.TextstringGuid,
            Validation = new PropertyTypeValidation(),
            Appearance = new PropertyTypeAppearance(),
        };
}
