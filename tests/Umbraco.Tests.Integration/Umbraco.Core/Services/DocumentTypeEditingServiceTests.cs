using NUnit.Framework;
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
public partial class DocumentTypeEditingServiceTests : UmbracoIntegrationTest
{
    private IDocumentTypeEditingService DocumentTypeEditingService => GetRequiredService<IDocumentTypeEditingService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();
}
