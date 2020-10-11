using System;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Tests.Common.Builders;

namespace Umbraco.Tests.Integration.Testing
{
    public abstract class UmbracoIntegrationTestWithContent : UmbracoIntegrationTest
    {
        protected IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();
        protected IFileService FileService => GetRequiredService<IFileService>();
        protected ContentService ContentService => (ContentService)GetRequiredService<IContentService>();

        protected int NodeIdSeed => 1050;

        public override void Setup()
        {
            base.Setup();
            CreateTestData();
        }

        public virtual void CreateTestData()
        {
            //NOTE Maybe not the best way to create/save test data as we are using the services, which are being tested.

            // Create and Save ContentType "umbTextpage" -> 1051 (template), 1052 (content type)
            var template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            var contentType = ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
            contentType.Key = new Guid("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522");
            ContentTypeService.Save(contentType);

            // Create and Save Content "Homepage" based on "umbTextpage" -> 1053
            var textpage = ContentBuilder.CreateSimpleContent(contentType);
            textpage.Key = new Guid("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0");
            ContentService.Save(textpage, 0);

            // Create and Save Content "Text Page 1" based on "umbTextpage" -> 1054
            var subpage = ContentBuilder.CreateSimpleContent(contentType, "Text Page 1", textpage.Id);
            subpage.ContentSchedule.Add(DateTime.Now.AddMinutes(-5), null);
            ContentService.Save(subpage, 0);

            // Create and Save Content "Text Page 1" based on "umbTextpage" -> 1055
            var subpage2 = ContentBuilder.CreateSimpleContent(contentType, "Text Page 2", textpage.Id);
            ContentService.Save(subpage2, 0);

            // Create and Save Content "Text Page Deleted" based on "umbTextpage" -> 1056
            var trashed = ContentBuilder.CreateSimpleContent(contentType, "Text Page Deleted", -20);
            trashed.Trashed = true;
            ContentService.Save(trashed, 0);
        }
    }
}
