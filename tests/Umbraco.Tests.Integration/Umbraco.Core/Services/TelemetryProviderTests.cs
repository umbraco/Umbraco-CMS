// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Implement;
using Umbraco.Cms.Core.Telemetry.Providers;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services
{
    /// <summary>
    /// Tests covering the SectionService
    /// </summary>
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class TelemetryProviderTests : UmbracoIntegrationTest
    {
        private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();
        private IFileService FileService => GetRequiredService<IFileService>();
        private IDomainService DomainService => GetRequiredService<IDomainService>();
        private IContentService ContentService => GetRequiredService<IContentService>();
        private DomainTelemetryProvider DetailedTelemetryProviders => GetRequiredService<DomainTelemetryProvider>();
        private ContentTelemetryProvider ContentTelemetryProvider => GetRequiredService<ContentTelemetryProvider>();

        protected override void CustomTestSetup(IUmbracoBuilder builder)
        {
            builder.Services.AddTransient<DomainTelemetryProvider>();
            builder.Services.AddTransient<ContentTelemetryProvider>();
            base.CustomTestSetup(builder);
        }

        [Test]
        public void Domain_Telemetry_Provider_Can_Get_Domains()
        {
            // Arrange
            DomainService.Save(new UmbracoDomain("danish", "da-DK"));

            IEnumerable<UsageInformation> result = null;
            // Act
            result = DetailedTelemetryProviders.GetInformation();


            // Assert
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public void SectionService_Can_Get_Allowed_Sections_For_User()
        {
            // Arrange
            Template template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            ContentType contentType = ContentTypeBuilder.CreateTextPageContentType(defaultTemplateId: template.Id);
            ContentTypeService.Save(contentType);

            Content blueprint = ContentBuilder.CreateTextpageContent(contentType, "hello", Constants.System.Root);
            blueprint.SetValue("title", "blueprint 1");
            blueprint.SetValue("bodyText", "blueprint 2");
            blueprint.SetValue("keywords", "blueprint 3");
            blueprint.SetValue("description", "blueprint 4");

            ContentService.SaveBlueprint(blueprint);

            IContent fromBlueprint = ContentService.CreateContentFromBlueprint(blueprint, "My test content");
            ContentService.Save(fromBlueprint);

            IEnumerable<UsageInformation> result = null;
            // Act
            result = ContentTelemetryProvider.GetInformation();


            // Assert
            Assert.AreEqual(1, result.Count());
        }
    }
}
