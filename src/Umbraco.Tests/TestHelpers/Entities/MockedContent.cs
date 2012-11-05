using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Tests.TestHelpers.Entities
{
    public class MockedContent
    {
        public static Content CreateSimpleContent(IContentType contentType)
        {
            var content = new Content(-1, contentType) { Name = "Home", Language = "en-US", Level = 1, ParentId = -1, SortOrder = 1, Template = "~/masterpages/umbTextPage.master", User = new Profile(0, "Administrator"), Writer = new Profile(0, "Administrator") };
            object obj =
                new
                    {
                        title = "Welcome to our Home page",
                        bodyText = "This is the welcome message on the first page",
                        author = "John Doe"
                    };

            content.PropertyValues(obj);

            return content;
        }

        public static Content CreateSimpleContent(IContentType contentType, string name, int parentId)
        {
            var content = new Content(parentId, contentType) { Name = name, Language = "en-US", ParentId = parentId, Template = "~/masterpages/umbTextPage.master", User = new Profile(0, "Administrator"), Writer = new Profile(0, "Administrator") };
            object obj =
                new
                {
                    title = name + " Subpage",
                    bodyText = "This is a subpage",
                    author = "John Doe"
                };

            content.PropertyValues(obj);

            return content;
        }

        public static Content CreateTextpageContent(IContentType contentType, string name, int parentId)
        {
            var content = new Content(parentId, contentType) { Name = name, Language = "en-US", ParentId = parentId, Template = "~/masterpages/umbTextPage.master", User = new Profile(0, "Administrator"), Writer = new Profile(0, "Administrator")};
            object obj =
                new
                {
                    title = name + " textpage",
                    bodyText = string.Format("This is a textpage based on the {0} ContentType", contentType.Alias),
                    keywords = "text,page,meta",
                    metaDescription = "This is the meta description for a textpage"
                };

            content.PropertyValues(obj);

            return content;
        }
    }
}