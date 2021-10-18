using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.TestHelpers.Entities
{
    public class MockedContent
    {
        public static Content CreateBasicContent(IContentType contentType)
        {
            var content = new Content("Home", -1, contentType) { Level = 1, SortOrder = 1, CreatorId = 0, WriterId = 0 };

            content.ResetDirtyProperties(false);

            return content;
        }

        public static Content CreateSimpleContent(IContentType contentType)
        {
            var content = new Content("Home", -1, contentType)
            {
                Level = 1,
                SortOrder = 1,
                CreatorId = 0,
                WriterId = 0,
                Key = Guid.NewGuid()
            };

            object obj =
                new
                    {
                        title = "Welcome to our Home page",
                        bodyText = "This is the welcome message on the first page",
                        author = "John Doe"
                    };

            content.PropertyValues(obj);

            content.ResetDirtyProperties(false);

            return content;
        }

        public static Content CreateSimpleContent(IContentType contentType, string name, int parentId = -1, string culture = null, string segment = null)
        {
            var content = new Content(name, parentId, contentType) { CreatorId = 0, WriterId = 0 };
            object obj =
                new
                {
                    title = name + " Subpage",
                    bodyText = "This is a subpage",
                    author = "John Doe"
                };

            content.PropertyValues(obj, culture, segment);

            content.ResetDirtyProperties(false);

            return content;
        }

        public static Content CreateSimpleContent(IContentType contentType, string name, IContent parent, string culture = null, string segment = null, bool setPropertyValues = true)
        {
            var content = new Content(name, parent, contentType, culture) { CreatorId = 0, WriterId = 0 };

            if (setPropertyValues)
            {
                object obj =
                new
                {
                    title = name + " Subpage",
                    bodyText = "This is a subpage",
                    author = "John Doe"
                };

                content.PropertyValues(obj, culture, segment);
            }

            content.ResetDirtyProperties(false);

            return content;
        }

        public static Content CreateTextpageContent(IContentType contentType, string name, int parentId)
        {
            var content = new Content(name, parentId, contentType) { CreatorId = 0, WriterId = 0};
            object obj =
                new
                {
                    title = name + " textpage",
                    bodyText = string.Format("This is a textpage based on the {0} ContentType", contentType.Alias),
                    keywords = "text,page,meta",
                    description = "This is the meta description for a textpage"
                };

            content.PropertyValues(obj);

            content.ResetDirtyProperties(false);

            return content;
        }

        public static Content CreateSimpleContentWithSpecialDatabaseTypes(IContentType contentType, string name, int parentId, string decimalValue, string intValue, DateTime datetimeValue)
        {
            var content = new Content(name, parentId, contentType) { CreatorId = 0, WriterId = 0 };
            object obj = new
            {
                decimalProperty = decimalValue,
                intProperty = intValue,
                datetimeProperty = datetimeValue
            };

            content.PropertyValues(obj);
            content.ResetDirtyProperties(false);
            return content;
        }

        public static Content CreateAllTypesContent(IContentType contentType, string name, int parentId)
        {
            var content = new Content("Random Content Name", parentId, contentType) { Level = 1, SortOrder = 1, CreatorId = 0, WriterId = 0 };

            content.SetValue("isTrue", true);
            content.SetValue("number", 42);
            content.SetValue("bodyText", "Lorem Ipsum Body Text Test");
            content.SetValue("singleLineText", "Single Line Text Test");
            content.SetValue("multilineText", "Multiple lines \n in one box");
            content.SetValue("upload", "/media/1234/koala.jpg");
            content.SetValue("label", "Non-editable label");
            content.SetValue("dateTime", DateTime.Now.AddDays(-20));
            content.SetValue("colorPicker", "black");
            //that one is gone in 7.4
            //content.SetValue("folderBrowser", "");
            content.SetValue("ddlMultiple", "1234,1235");
            content.SetValue("rbList", "random");
            content.SetValue("date", DateTime.Now.AddDays(-10));
            content.SetValue("ddl", "1234");
            content.SetValue("chklist", "randomc");
            content.SetValue("contentPicker", Udi.Create(Constants.UdiEntityType.Document, new Guid("74ECA1D4-934E-436A-A7C7-36CC16D4095C")).ToString());
            content.SetValue("mediaPicker3", "[{\"key\": \"8f78ce9e-8fe0-4500-a52d-4c4f35566ba9\",\"mediaKey\": \"44CB39C8-01E5-45EB-9CF8-E70AAF2D1691\",\"crops\": [],\"focalPoint\": {\"left\": 0.5,\"top\": 0.5}}]");
            content.SetValue("memberPicker", Udi.Create(Constants.UdiEntityType.Member, new Guid("9A50A448-59C0-4D42-8F93-4F1D55B0F47D")).ToString());
            content.SetValue("multiUrlPicker", "[{\"name\":\"https://test.com\",\"url\":\"https://test.com\"}]");
            content.SetValue("tags", "this,is,tags");

            return content;
        }

        public static IEnumerable<Content> CreateTextpageContent(IContentType contentType, int parentId, int amount)
        {
            var list = new List<Content>();

            for (int i = 0; i < amount; i++)
            {
                var name = "Textpage No-" + i;
                var content = new Content(name, parentId, contentType) { CreatorId = 0, WriterId = 0 };
                object obj =
                    new
                    {
                        title = name + " title",
                        bodyText = string.Format("This is a textpage based on the {0} ContentType", contentType.Alias),
                        keywords = "text,page,meta",
                        description = "This is the meta description for a textpage"
                    };

                content.PropertyValues(obj);

                content.ResetDirtyProperties(false);

                list.Add(content);
            }

            return list;
        }
    }
}
