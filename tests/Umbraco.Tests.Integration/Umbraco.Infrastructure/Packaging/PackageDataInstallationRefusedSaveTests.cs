// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Xml.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Packaging;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services.Importing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Packaging;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, WithApplication = true)]
internal sealed class PackageDataInstallationRefusedSaveTests : UmbracoIntegrationTest
{
    private IPackageDataInstallation PackageDataInstallation => GetRequiredService<IPackageDataInstallation>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.AddNotificationHandler<ContentSavingNotification, CancelContentSavingHandler>();

    [Test]
    public async Task Content_Whose_Save_Is_Refused_Is_Not_Reported_As_Installed()
    {
        var xml = XElement.Parse(ImportResources.StandardMvc_Package);
        var dataTypesElement = xml.Descendants("DataTypes").First();
        var docTypesElement = xml.Descendants("DocumentTypes").First();
        var packageDocument = CompiledPackageContentBase.Create(xml.Descendants("DocumentSet").First());

        PackageDataInstallation.ImportDataTypes(dataTypesElement.Elements("DataType").ToList(), -1);
        var importedContentTypes = PackageDataInstallation
            .ImportDocumentTypes(docTypesElement.Elements("DocumentType"), -1)
            .ToDictionary(x => x.Alias, x => x);

        IReadOnlyList<IContent> installed = PackageDataInstallation.ImportContentBase(
            packageDocument.Yield(),
            importedContentTypes,
            -1,
            ContentTypeService,
            ContentService);

        Assert.That(installed, Is.Empty);
        Assert.That(await ContentService.GetRootContentAsync(CancellationToken.None), Is.Empty);
    }

    private sealed class CancelContentSavingHandler : INotificationHandler<ContentSavingNotification>
    {
        public void Handle(ContentSavingNotification notification) => notification.Cancel = true;
    }
}
