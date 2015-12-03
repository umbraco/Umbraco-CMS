using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Tests.TestHelpers.Entities
{
    public class MockedContent
    {
        public static Content CreateSimpleContent(IContentType contentType)
        {
            var content = new Content("Home", -1, contentType) { Language = "en-US", Level = 1, SortOrder = 1, CreatorId = 0, WriterId = 0 };
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

        public static Content CreateSimpleContent(IContentType contentType, string name, int parentId)
        {
            var content = new Content(name, parentId, contentType) { Language = "en-US", CreatorId = 0, WriterId = 0 };
            object obj =
                new
                {
                    title = name + " Subpage",
                    bodyText = "This is a subpage",
                    author = "John Doe"
                };

            content.PropertyValues(obj);

            content.ResetDirtyProperties(false);

            return content;
        }

		public static Content CreateSimpleContent(IContentType contentType, string name, IContent parent)
		{
			var content = new Content(name, parent, contentType) { Language = "en-US", CreatorId = 0, WriterId = 0 };
			object obj =
				new
				{
					title = name + " Subpage",
					bodyText = "This is a subpage",
					author = "John Doe"
				};

			content.PropertyValues(obj);

            content.ResetDirtyProperties(false);

			return content;
		}

        public static Content CreateTextpageContent(IContentType contentType, string name, int parentId)
        {
            var content = new Content(name, parentId, contentType) { Language = "en-US", CreatorId = 0, WriterId = 0};
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

        public static Content CreateAllTypesContent(IContentType contentType, string name, int parentId)
        {
            var content = new Content("Random Content Name", parentId, contentType) { Language = "en-US", Level = 1, SortOrder = 1, CreatorId = 0, WriterId = 0 };

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
            content.SetValue("contentPicker", 1090);
            content.SetValue("mediaPicker", 1091);
            content.SetValue("memberPicker", 1092);
            content.SetValue("relatedLinks", "<links><link title=\"google\" link=\"http://google.com\" type=\"external\" newwindow=\"0\" /></links>");
            content.SetValue("tags", "this,is,tags");

            return content;
        }

        public static IEnumerable<Content> CreateTextpageContent(IContentType contentType, int parentId, int amount)
        {
            var list = new List<Content>();

            for (int i = 0; i < amount; i++)
            {
                var name = "Textpage No-" + i;
                var content = new Content(name, parentId, contentType) { Language = "en-US", CreatorId = 0, WriterId = 0 };
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