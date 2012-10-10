using Umbraco.Core.Models;

namespace Umbraco.Tests.TestHelpers.Entities
{
    public class MockedContent
    {
        public static Content CreateTextpageContent(ContentType contentType)
        {
            var content = new Content(-1, contentType) {Name = "Home", Language = "en-US", Level = 1, ParentId = -1, SortOrder = 1, Template = "~/masterpages/umbTextPage.master", UserId = 0};
            object obj =
                new
                    {
                        title = "Welcome to our Home page",
                        bodyText = "This is the welcome message on the first page",
                        keywords = "text,home,page",
                        metaDescription = "The Lorem Ipsum company"
                    };

            content.PropertyValues(obj);

            return content;
        }
    }
}