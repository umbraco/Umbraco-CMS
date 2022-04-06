// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Telemetry.Providers;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

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

        private LanguagesTelemetryProvider LanguagesTelemetryProvider => GetRequiredService<LanguagesTelemetryProvider>();

        private UserTelemetryProvider UserTelemetryProvider => GetRequiredService<UserTelemetryProvider>();

        private ILocalizationService LocalizationService => GetRequiredService<ILocalizationService>();

        private IUserService UserService => GetRequiredService<IUserService>();

        private LanguageBuilder _languageBuilder = new();
        
        private UserBuilder _userBuilder = new();
        
        private UserGroupBuilder _userGroupBuilder = new();

        protected override void CustomTestSetup(IUmbracoBuilder builder)
        {
            builder.Services.AddTransient<DomainTelemetryProvider>();
            builder.Services.AddTransient<ContentTelemetryProvider>();
            builder.Services.AddTransient<LanguagesTelemetryProvider>();
            builder.Services.AddTransient<UserTelemetryProvider>();
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
            Assert.AreEqual(1, result.First().Data);
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
            // TODO : Test multiple roots, with children + grandchildren
            Assert.AreEqual(1, result.First().Data);
        }

        [Test]
        public void Language_Telemetry_Can_Get_Languages()
        {
            // Arrange
            var langTwo = _languageBuilder.WithCultureInfo("da-DK").Build();
            var langThree = _languageBuilder.WithCultureInfo("se-SV").Build();

            LocalizationService.Save(langTwo);
            LocalizationService.Save(langThree);

            IEnumerable<UsageInformation> result = null;

            // Act
            result = LanguagesTelemetryProvider.GetInformation();

            // Assert
            Assert.AreEqual(3, result.First().Data);
        }

        [Test]
        public void MacroTelemetry_Can_Get_Macros()
        {
            // TODO: Test MacroTelemetryProvider by creating macros & asserting telemetry reports the correct number
        }

        [Test]
        public void MediaTelemetry_Can_Get_Media()
        {
            // TODO: Test MediaTelemetryProvider by creating media & asserting telemetry reports the correct number
        }

        [Test]
        public void PropertyEditorTelemetry_Can_Get_PropertyTypes()
        {
            // TODO: Test PropertyEditorTelemetryProvider by creating documentTypes and using propertyTypes, and creating custom PropertyTypes
            // and assert that the telemetry reports the correct numbers/properties
        }

        [Test]
        public void UserTelemetry_Can_GetUser()
        {
            var result = UserTelemetryProvider.GetInformation().FirstOrDefault(x => x.Name == "UserCount");

            Assert.AreEqual(1, result.Data);
        }

        [Test]
        public void UserTelemetry_Can_Get_More_Users()
        {
            var user = BuildUser();

            UserService.Save(user);

            var result = UserTelemetryProvider.GetInformation().FirstOrDefault(x => x.Name == "UserCount");

            Assert.AreEqual(2, result.Data);
        }

        [Test]
        public void UserTelemetry_Can_Get_Default_UserGroups()
        {
            var result = UserTelemetryProvider.GetInformation().FirstOrDefault(x => x.Name == "UserGroupCount");

            Assert.AreEqual(5, result.Data);
        }

        [Test]
        public void UserTelemetry_Can_Get_Saved_UserGroups()
        {
            var userGroup = BuildUserGroup("testGroup");

            UserService.Save(userGroup);
            var result = UserTelemetryProvider.GetInformation().FirstOrDefault(x => x.Name == "UserGroupCount");

            Assert.AreEqual(6, result.Data);
        }
        
        [Test]
        public void UserTelemetry_Can_Get_More_UserGroups()
        {
            var userGroups = BuildUserGroups(100);


            foreach (var userGroup in userGroups)
            {
                UserService.Save(userGroup);
            }

            var result = UserTelemetryProvider.GetInformation().FirstOrDefault(x => x.Name == "UserGroupCount");

            Assert.AreEqual(105, result.Data);
        }

        private User BuildUser() =>
            _userBuilder
                .WithLogin("username", "test pass")
                .WithName("Test")
                .WithEmail("test@test.com")
                .Build();
        private IUserGroup BuildUserGroup(string alias) =>
            _userGroupBuilder
                .WithAlias(alias)
                .WithName(alias)
                .WithAllowedSections(new List<string>(){"A", "B"})
                .Build();

        private IEnumerable<IUserGroup> BuildUserGroups(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return BuildUserGroup(i.ToString());
            }
        }
    }
}
