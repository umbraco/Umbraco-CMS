using System;
using System.Collections.Generic;
using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Tests.Common.Builders.Interfaces;
using Umbraco.Tests.Common.Builders.Extensions;
using Umbraco.Tests.Testing;
using Umbraco.Core;

namespace Umbraco.Tests.Common.Builders
{
    public class ContentBuilder
        : BuilderBase<Content>,
            IBuildContentTypes,
            IWithIdBuilder,
            IWithKeyBuilder,
            IWithParentIdBuilder,
            IWithCreatorIdBuilder,
            IWithCreateDateBuilder,
            IWithUpdateDateBuilder,
            IWithNameBuilder,
            IWithTrashedBuilder,
            IWithLevelBuilder,
            IWithPathBuilder,
            IWithSortOrderBuilder,
            IWithCultureInfoBuilder,
            IWithPropertyValues
    {
        private ContentTypeBuilder _contentTypeBuilder;
        private GenericDictionaryBuilder<ContentBuilder, string, object> _propertyDataBuilder;

        private int? _id;
        private int? _versionId;
        private Guid? _key;
        private DateTime? _createDate;
        private DateTime? _updateDate;
        private int? _parentId;
        private IContent _parent;
        private string _name;
        private int? _creatorId;
        private int? _level;
        private string _path;
        private int? _sortOrder;
        private bool? _trashed;
        private CultureInfo _cultureInfo;
        private IContentType _contentType;
        private IDictionary<string, string> _cultureNames = new Dictionary<string, string>();
        private object _propertyValues;
        private string _propertyValuesCulture;
        private string _propertyValuesSegment;

        public ContentBuilder WithVersionId(int versionId)
        {
            _versionId = versionId;
            return this;
        }

        public ContentBuilder WithParent(IContent parent)
        {
            _parentId = null;
            _parent = parent;
            return this;
        }

        public ContentBuilder WithContentType(IContentType contentType)
        {
            _contentTypeBuilder = null;
            _contentType = contentType;
            return this;
        }

        public ContentBuilder WithCultureName(string culture, string name = "")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                if (_cultureNames.TryGetValue(culture, out _))
                {
                    _cultureNames.Remove(culture);
                }
            }
            else
            {
                _cultureNames[culture] = name;
            }

            return this;
        }

        public ContentTypeBuilder AddContentType()
        {
            _contentType = null;
            var builder = new ContentTypeBuilder(this);
            _contentTypeBuilder = builder;
            return builder;
        }

        public GenericDictionaryBuilder<ContentBuilder, string, object> AddPropertyData()
        {
            var builder = new GenericDictionaryBuilder<ContentBuilder, string, object>(this);
            _propertyDataBuilder = builder;
            return builder;
        }

        public override Content Build()
        {
            var id = _id ?? 0;
            var versionId = _versionId ?? 0;
            var key = _key ?? Guid.NewGuid();
            var parentId = _parentId ?? -1;
            var parent = _parent ?? null;
            var createDate = _createDate ?? DateTime.Now;
            var updateDate = _updateDate ?? DateTime.Now;
            var name = _name ?? Guid.NewGuid().ToString();
            var creatorId = _creatorId ?? 0;
            var level = _level ?? 1;
            var path = _path ?? $"-1,{id}";
            var sortOrder = _sortOrder ?? 0;
            var trashed = _trashed ?? false;
            var culture = _cultureInfo?.Name ?? null;
            var propertyValues = _propertyValues ?? null;
            var propertyValuesCulture = _propertyValuesCulture ?? null;
            var propertyValuesSegment = _propertyValuesSegment ?? null;

            if (_contentTypeBuilder is null && _contentType is null)
            {
                throw new InvalidOperationException("A content item cannot be constructed without providing a content type. Use AddContentType() or WithContentType().");
            }

            var contentType = _contentType ?? _contentTypeBuilder.Build();

            Content content;
            if (parent != null)
            {
                content = new Content(name, parent, contentType, culture);
            }
            else
            {
                content = new Content(name, parentId, contentType, culture);
            }

            content.Id = id;
            content.VersionId = versionId;
            content.Key = key;
            content.CreateDate = createDate;
            content.UpdateDate = updateDate;
            content.CreatorId = creatorId;
            content.Level = level;
            content.Path = path;
            content.SortOrder = sortOrder;
            content.Trashed = trashed;

            foreach (var cultureName in _cultureNames)
            {
                content.SetCultureName(cultureName.Value, cultureName.Key);
            }

            if (_propertyDataBuilder != null || propertyValues != null)
            {
                if (_propertyDataBuilder != null)
                {
                    var propertyData = _propertyDataBuilder.Build();
                    foreach (var keyValuePair in propertyData)
                    {
                        content.SetValue(keyValuePair.Key, keyValuePair.Value);
                    }
                }
                else
                {
                    content.PropertyValues(propertyValues, propertyValuesCulture, propertyValuesSegment);
                }

                content.ResetDirtyProperties(false);
            }

            return content;
        }

        public static Content CreateBasicContent(IContentType contentType)
        {
            return new ContentBuilder()
                .WithContentType(contentType)
                .WithName("Home")
                .Build();
        }

        public static Content CreateSimpleContent(IContentType contentType)
        {
            return new ContentBuilder()
                .WithContentType(contentType)
                .WithName("Home")
                .WithPropertyValues(new
                    {
                        title = "Welcome to our Home page",
                        bodyText = "This is the welcome message on the first page",
                        author = "John Doe"
                    })
                .Build();
        }

        public static Content CreateSimpleContent(IContentType contentType, string name, int parentId = -1, string culture = null, string segment = null)
        {
            return new ContentBuilder()
                .WithContentType(contentType)
                .WithName(name)
                .WithParentId(parentId)
                .WithPropertyValues(new
                    {
                        title = "Welcome to our Home page",
                        bodyText = "This is the welcome message on the first page",
                        author = "John Doe"
                    }, culture, segment)
                .Build();
        }

        public static Content CreateSimpleContent(IContentType contentType, string name, IContent parent, string culture = null, string segment = null, bool setPropertyValues = true)
        {
            var builder = new ContentBuilder()
                .WithContentType(contentType)
                .WithName(name)
                .WithParent(parent);


            if (!(culture is null))
            {
                builder = builder.WithCultureName(culture, name);
            }

            if (setPropertyValues)
            {
                builder = builder.WithPropertyValues(new
                {
                    title = name + " Subpage",
                    bodyText = "This is a subpage",
                    author = "John Doe"
                }, culture, segment);
            }

            var content = builder.Build();

            content.ResetDirtyProperties(false);

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
        public static Content CreateTextpageContent(IContentType contentType, string name, int parentId)
        {
            return new ContentBuilder()
                .WithId(0)
                .WithContentType(contentType)
                .WithName(name)
                .WithParentId(parentId)
                .WithPropertyValues(new
                    {
                        title = name + " textpage",
                        bodyText = string.Format("This is a textpage based on the {0} ContentType", contentType.Alias),
                        keywords = "text,page,meta",
                        description = "This is the meta description for a textpage"
                    })
                .Build();
        }

        public static IEnumerable<Content> CreateMultipleTextpageContent(IContentType contentType, int parentId, int amount)
        {
            var list = new List<Content>();

            for (var i = 0; i < amount; i++)
            {
                var name = "Textpage No-" + i;
                var content = new ContentBuilder()
                    .WithName(name)
                    .WithParentId(parentId)
                    .WithContentType(contentType)
                    .WithPropertyValues(new
                        {
                            title = name + " title",
                            bodyText = $"This is a textpage based on the {contentType.Alias} ContentType",
                            keywords = "text,page,meta",
                            description = "This is the meta description for a textpage"
                        })
                    .Build();

                list.Add(content);
            }

            return list;
        }

        public static Content CreateAllTypesContent(IContentType contentType, string name, int parentId)
        {
            var content = new ContentBuilder()
                .WithName(name)
                .WithParentId(parentId)
                .WithContentType(contentType)
                .Build();

            content.SetValue("isTrue", true);
            content.SetValue("number", 42);
            content.SetValue("bodyText", "Lorem Ipsum Body Text Test");
            content.SetValue("singleLineText", "Single Line Text Test");
            content.SetValue("multilineText", "Multiple lines \n in one box");
            content.SetValue("upload", "/media/1234/koala.jpg");
            content.SetValue("label", "Non-editable label");
            content.SetValue("dateTime", DateTime.Now.AddDays(-20));
            content.SetValue("colorPicker", "black");
            content.SetValue("ddlMultiple", "1234,1235");
            content.SetValue("rbList", "random");
            content.SetValue("date", DateTime.Now.AddDays(-10));
            content.SetValue("ddl", "1234");
            content.SetValue("chklist", "randomc");
            content.SetValue("contentPicker", Udi.Create(Constants.UdiEntityType.Document, new Guid("74ECA1D4-934E-436A-A7C7-36CC16D4095C")).ToString());
            content.SetValue("mediaPicker", Udi.Create(Constants.UdiEntityType.Media, new Guid("44CB39C8-01E5-45EB-9CF8-E70AAF2D1691")).ToString());
            content.SetValue("memberPicker", Udi.Create(Constants.UdiEntityType.Member, new Guid("9A50A448-59C0-4D42-8F93-4F1D55B0F47D")).ToString());
            content.SetValue("multiUrlPicker", "[{\"name\":\"https://test.com\",\"url\":\"https://test.com\"}]");
            content.SetValue("tags", "this,is,tags");

            return content;
        }

        int? IWithIdBuilder.Id
        {
            get => _id;
            set => _id = value;
        }

        Guid? IWithKeyBuilder.Key
        {
            get => _key;
            set => _key = value;
        }

        int? IWithCreatorIdBuilder.CreatorId
        {
            get => _creatorId;
            set => _creatorId = value;
        }

        DateTime? IWithCreateDateBuilder.CreateDate
        {
            get => _createDate;
            set => _createDate = value;
        }

        DateTime? IWithUpdateDateBuilder.UpdateDate
        {
            get => _updateDate;
            set => _updateDate = value;
        }

        string IWithNameBuilder.Name
        {
            get => _name;
            set => _name = value;
        }

        bool? IWithTrashedBuilder.Trashed
        {
            get => _trashed;
            set => _trashed = value;
        }

        int? IWithLevelBuilder.Level
        {
            get => _level;
            set => _level = value;
        }

        string IWithPathBuilder.Path
        {
            get => _path;
            set => _path = value;
        }

        int? IWithSortOrderBuilder.SortOrder
        {
            get => _sortOrder;
            set => _sortOrder = value;
        }

        int? IWithParentIdBuilder.ParentId
        {
            get => _parentId;
            set => _parentId = value;
        }

        CultureInfo IWithCultureInfoBuilder.CultureInfo
        {
            get => _cultureInfo;
            set => _cultureInfo = value;
        }

        object IWithPropertyValues.PropertyValues
        {
            get => _propertyValues;
            set => _propertyValues = value;
        }

        string IWithPropertyValues.PropertyValuesCulture
        {
            get => _propertyValuesCulture;
            set => _propertyValuesCulture = value;
        }

        string IWithPropertyValues.PropertyValuesSegment
        {
            get => _propertyValuesSegment;
            set => _propertyValuesSegment = value;
        }
    }
}
