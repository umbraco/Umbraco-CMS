using System;
using System.Collections.Generic;
using Umbraco.Extensions;
using Umbraco.Cms.Infrastructure.PublishedCache.DataSource;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.PropertyEditors;
using Moq;
using Umbraco.Cms.Infrastructure.Serialization;
using System.Linq;

namespace Umbraco.Cms.Tests.Common.Builders
{

    public class ContentDataBuilder : BuilderBase<ContentData>, IWithNameBuilder
    {
        private string _name;
        private DateTime? _now;
        private string _segment;
        private int? _versionId;
        private int? _writerId;
        private int? _templateId;
        private bool? _published;
        private Dictionary<string, PropertyData[]> _properties;
        private Dictionary<string, CultureVariation> _cultureInfos;

        string IWithNameBuilder.Name
        {
            get => _name;
            set => _name = value;
        }

        public ContentDataBuilder WithVersionDate(DateTime now)
        {
            _now = now;
            return this;
        }

        public ContentDataBuilder WithUrlSegment(string segment)
        {
            _segment = segment;
            return this;
        }

        public ContentDataBuilder WithVersionId(int versionId)
        {
            _versionId = versionId;
            return this;
        }

        public ContentDataBuilder WithWriterId(int writerId)
        {
            _writerId = writerId;
            return this;
        }

        public ContentDataBuilder WithTemplateId(int templateId)
        {
            _templateId = templateId;
            return this;
        }

        public ContentDataBuilder WithPublished(bool published)
        {
            _published = published;
            return this;
        }

        public ContentDataBuilder WithProperties(Dictionary<string, PropertyData[]> properties)
        {
            _properties = properties;
            return this;
        }

        public ContentDataBuilder WithCultureInfos(Dictionary<string, CultureVariation> cultureInfos)
        {
            _cultureInfos = cultureInfos;
            return this;
        }

        /// <summary>
        /// Build and dynamically create a matching content type
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public ContentData Build(IShortStringHelper shortStringHelper, string alias, Dictionary<string, IDataType> propertyDataTypes, out ContentType contentType)
        {
            var result = Build();

            contentType = new ContentType(shortStringHelper, -1)
            {
                Alias = alias,
                Name = alias,
                Key = Guid.NewGuid(),
                Id = alias.GetHashCode()
            };

            foreach(var prop in result.Properties)
            {
                //var dataType = new DataType(new VoidEditor("Label", Mock.Of<IDataValueEditorFactory>()), new ConfigurationEditorJsonSerializer())
                //{
                //    Id = 4
                //};

                if (!propertyDataTypes.TryGetValue(prop.Key, out IDataType dataType))
                {
                    dataType = propertyDataTypes.First().Value;
                }

                var propertyType = new PropertyType(shortStringHelper, dataType, prop.Key);
                if (!contentType.PropertyTypeExists(propertyType.Alias))
                {
                    contentType.AddPropertyType(propertyType);
                }
            }

            return result;
        }

        public override ContentData Build()
        {
            var now = _now ?? DateTime.Now;
            var versionId = _versionId ?? 1;
            var writerId = _writerId ?? -1;
            var templateId = _templateId ?? 0;
            var published = _published ?? true;
            var properties = _properties ?? new Dictionary<string, PropertyData[]>();
            var cultureInfos = _cultureInfos ?? new Dictionary<string, CultureVariation>();
            var segment = (_segment ?? _name).ToLower().ReplaceNonAlphanumericChars('-');


            var contentData = new ContentData(
                _name,
                segment,
                versionId,
                now,
                writerId,
                templateId,
                published,
                properties,
                cultureInfos);

            return contentData;
        }

        public static ContentData CreateBasic(string name, DateTime? versionDate = null)
            => new ContentDataBuilder()
            .WithName(name)
            .WithVersionDate(versionDate ?? DateTime.Now)
            .Build();

        public static ContentData CreateVariant(string name, Dictionary<string, CultureVariation> cultureInfos, DateTime? versionDate = null, bool published = true)
            => new ContentDataBuilder()
            .WithName(name)
            .WithVersionDate(versionDate ?? DateTime.Now)
            .WithCultureInfos(cultureInfos)
            .WithPublished(published)
            .Build();
    }
}
