﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Bogus;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Extensions;
using Umbraco.Web.Mvc;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.TestData
{
    /// <summary>
    /// Creates test data
    /// </summary>
    public class UmbracoTestDataController : SurfaceController
    {
        private const string RichTextDataTypeName = "UmbracoTestDataContent.RTE";
        private const string MediaPickerDataTypeName = "UmbracoTestDataContent.MediaPicker";
        private const string TextDataTypeName = "UmbracoTestDataContent.Text";
        private const string TestDataContentTypeAlias = "umbTestDataContent";
        private readonly IScopeProvider _scopeProvider;
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly IShortStringHelper _shortStringHelper;

        public UmbracoTestDataController(IScopeProvider scopeProvider, PropertyEditorCollection propertyEditors, IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IShortStringHelper shortStringHelper)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger)
        {
            _scopeProvider = scopeProvider;
            _propertyEditors = propertyEditors;
            _shortStringHelper = shortStringHelper;
        }

        /// <summary>
        /// Creates a content and associated media tree (hierarchy)
        /// </summary>
        /// <param name="count"></param>
        /// <param name="depth"></param>
        /// <param name="locale"></param>
        /// <returns></returns>
        /// <remarks>
        /// Each content item created is associated to a media item via a media picker and therefore a relation is created between the two
        /// </remarks>
        public ActionResult CreateTree(int count, int depth, string locale = "en")
        {
            if (ConfigurationManager.AppSettings["Umbraco.TestData.Enabled"] != "true")
                return HttpNotFound();

            if (!Validate(count, depth, out var message, out var perLevel))
                throw new InvalidOperationException(message);

            var faker = new Faker(locale);
            var company = faker.Company.CompanyName();

            using (var scope = _scopeProvider.CreateScope())
            {
                var imageIds = CreateMediaTree(company, faker, count, depth).ToList();
                var contentIds = CreateContentTree(company, faker, count, depth, imageIds, out var root).ToList();

                Services.ContentService.SaveAndPublishBranch(root, true);

                scope.Complete();
            }


            return Content("Done");
        }

        private bool Validate(int count, int depth, out string message, out int perLevel)
        {
            perLevel = 0;
            message = null;

            if (count <= 0)
            {
                message = "Count must be more than 0";
                return false;
            }

            perLevel = count / depth;
            if (perLevel < 1)
            {
                message = "Count not high enough for specified for number of levels required";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Utility to create a tree hierarchy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <param name="count"></param>
        /// <param name="depth"></param>
        /// <param name="create">
        /// A callback that returns a tuple of Content and another callback to produce a Container.
        /// For media, a container will be another folder, for content the container will be the Content itself.
        /// </param>
        /// <returns></returns>
        private IEnumerable<Udi> CreateHierarchy<T>(
            T parent, int count, int depth,
            Func<T, (T content, Func<T> container)> create)
            where T: class, IContentBase
        {
            yield return parent.GetUdi();

            // This will not calculate a balanced tree but it will ensure that there will be enough nodes deep enough to not fill up the tree.
            var totalDescendants = count - 1;
            var perLevel = Math.Ceiling(totalDescendants / (double)depth);
            var perBranch = Math.Ceiling(perLevel / depth);

            var tracked = new Stack<(T parent, int childCount)>();

            var currChildCount = 0;

            for (int i = 0; i < count; i++)
            {
                var created = create(parent);
                var contentItem = created.content;

                yield return contentItem.GetUdi();

                currChildCount++;

                if (currChildCount == perBranch)
                {
                    // move back up...

                    var prev = tracked.Pop();

                    // restore child count
                    currChildCount = prev.childCount;
                    // restore the parent
                    parent = prev.parent;

                }
                else if (contentItem.Level < depth)
                {
                    // track the current parent and it's current child count
                    tracked.Push((parent, currChildCount));

                    // not at max depth, create below
                    parent = created.container();

                    currChildCount = 0;
                }

            }
        }

        /// <summary>
        /// Creates the media tree hiearachy
        /// </summary>
        /// <param name="company"></param>
        /// <param name="faker"></param>
        /// <param name="count"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        private IEnumerable<Udi> CreateMediaTree(string company, Faker faker, int count, int depth)
        {
            var parent = Services.MediaService.CreateMediaWithIdentity(company, -1, Constants.Conventions.MediaTypes.Folder);

            return CreateHierarchy(parent, count, depth, currParent =>
            {
                var imageUrl = faker.Image.PicsumUrl();

                // we are appending a &ext=.jpg to the end of this for a reason. The result of this URL will be something like:
                // https://picsum.photos/640/480/?image=106
                // and due to the way that we detect images there must be an extension so we'll change it to
                // https://picsum.photos/640/480/?image=106&ext=.jpg
                // which will trick our app into parsing this and thinking it's an image ... which it is so that's good.
                // if we don't do this we don't get thumbnails in the back office.
                imageUrl += "&ext=.jpg";

                var media = Services.MediaService.CreateMedia(faker.Commerce.ProductName(), currParent, Constants.Conventions.MediaTypes.Image);
                media.SetValue(Constants.Conventions.Media.File, imageUrl);
                Services.MediaService.Save(media);
                return (media, () =>
                {
                    // create a folder to contain child media
                    var container = Services.MediaService.CreateMediaWithIdentity(faker.Commerce.Department(), currParent, Constants.Conventions.MediaTypes.Folder);
                    return container;
                });
            });
        }

        /// <summary>
        /// Creates the content tree hiearachy
        /// </summary>
        /// <param name="company"></param>
        /// <param name="faker"></param>
        /// <param name="count"></param>
        /// <param name="depth"></param>
        /// <param name="imageIds"></param>
        /// <returns></returns>
        private IEnumerable<Udi> CreateContentTree(string company, Faker faker, int count, int depth, List<Udi> imageIds, out IContent root)
        {
            var random = new Random(company.GetHashCode());

            var docType = GetOrCreateContentType();

            var parent = Services.ContentService.Create(company, -1, docType.Alias);
            parent.SetValue("review", faker.Rant.Review());
            parent.SetValue("desc", company);
            parent.SetValue("media", imageIds[random.Next(0, imageIds.Count - 1)]);
            Services.ContentService.Save(parent);

            root = parent;

            return CreateHierarchy(parent, count, depth, currParent =>
            {
                var content = Services.ContentService.Create(faker.Commerce.ProductName(), currParent, docType.Alias);
                content.SetValue("review", faker.Rant.Review());
                content.SetValue("desc", string.Join(", ", Enumerable.Range(0, 5).Select(x => faker.Commerce.ProductAdjective())));
                content.SetValue("media", imageIds[random.Next(0, imageIds.Count - 1)]);

                Services.ContentService.Save(content);
                return (content, () => content);
            });

        }

        private IContentType GetOrCreateContentType()
        {
            var docType = Services.ContentTypeService.Get(TestDataContentTypeAlias);
            if (docType != null)
                return docType;

            docType = new ContentType(_shortStringHelper, -1)
            {
                Alias = TestDataContentTypeAlias,
                Name = "Umbraco Test Data Content",
                Icon = "icon-science color-green"
            };
            docType.AddPropertyGroup("Content");
            docType.AddPropertyType(new PropertyType(_shortStringHelper, GetOrCreateRichText(), "review")
            {
                Name = "Review"
            });
            docType.AddPropertyType(new PropertyType(_shortStringHelper, GetOrCreateMediaPicker(), "media")
            {
                Name = "Media"
            });
            docType.AddPropertyType(new PropertyType(_shortStringHelper, GetOrCreateText(), "desc")
            {
                Name = "Description"
            });
            Services.ContentTypeService.Save(docType);
            docType.AllowedContentTypes = new[] { new ContentTypeSort(docType.Id, 0) };
            Services.ContentTypeService.Save(docType);
            return docType;
        }

        private IDataType GetOrCreateRichText() => GetOrCreateDataType(RichTextDataTypeName, Constants.PropertyEditors.Aliases.TinyMce);

        private IDataType GetOrCreateMediaPicker() => GetOrCreateDataType(MediaPickerDataTypeName, Constants.PropertyEditors.Aliases.MediaPicker);

        private IDataType GetOrCreateText() => GetOrCreateDataType(TextDataTypeName, Constants.PropertyEditors.Aliases.TextBox);

        private IDataType GetOrCreateDataType(string name, string editorAlias)
        {
            var dt = Services.DataTypeService.GetDataType(name);
            if (dt != null) return dt;

            var editor = _propertyEditors.FirstOrDefault(x => x.Alias == editorAlias);
            if (editor == null)
                throw new InvalidOperationException($"No {editorAlias} editor found");

            var serializer = new ConfigurationEditorJsonSerializer();

            dt = new DataType(editor, serializer)
            {
                Name = name,
                Configuration = editor.GetConfigurationEditor().DefaultConfigurationObject,
                DatabaseType = ValueStorageType.Ntext
            };

            Services.DataTypeService.Save(dt);
            return dt;
        }
    }
}
