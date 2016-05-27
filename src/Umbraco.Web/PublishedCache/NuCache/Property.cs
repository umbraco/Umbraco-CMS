using System;
using System.Xml.Serialization;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PublishedCache.NuCache
{
    [Serializable]
    [XmlType(Namespace = "http://umbraco.org/webservices/")]
    class Property : PublishedPropertyBase
    {
        private readonly object _dataValue;
        private readonly Guid _contentUid;
        private readonly bool _isPreviewing;
        private readonly bool _isMember;

        readonly object _locko = new object();

        private ValueSet _valueSet;
        private string _valueSetCacheKey;
        private string _recurseCacheKey;

        // initializes a published content property with no value
        public Property(PublishedPropertyType propertyType, PublishedContent content)
            : this(propertyType, content, null)
        { }

        // initializes a published content property with a value
        public Property(PublishedPropertyType propertyType, PublishedContent content, object valueSource)
            : base(propertyType)
        {
            _dataValue = valueSource;
            _contentUid = content.Key;
            var inner = PublishedContent.UnwrapIPublishedContent(content);
            _isPreviewing = inner.IsPreviewing;
            _isMember = content.ContentType.ItemType == PublishedItemType.Member;
        }

        // clone for previewing as draft a published content that is published and has no draft
        public Property(Property origin)
            : base(origin.PropertyType)
        {
            _dataValue = origin._dataValue;
            _contentUid = origin._contentUid;
            _isPreviewing = true;
            _isMember = origin._isMember;
        }

        // detached
        //internal Property(PublishedPropertyType propertyType, Guid contentUid, object valueSource, bool isPreviewing, bool isMember)
        //    : base(propertyType)
        //{
        //    _dataValue = valueSource;
        //    _contentUid = contentUid;
        //    _isPreviewing = isPreviewing;
        //    _isMember = isMember;
        //}

        public override bool HasValue
        {
            get { return _dataValue != null && ((_dataValue is string) == false || string.IsNullOrWhiteSpace((string)_dataValue) == false); }
        }

        private class ValueSet
        {
            public bool SourceInitialized;
            public object Source;
            public bool ValueInitialized;
            public object Value;
            public bool XPathInitialized;
            public object XPath;
        }

        internal string RecurseCacheKey
        {
            get { return _recurseCacheKey ?? (_recurseCacheKey = CacheKeys.PropertyRecurse(_contentUid, PropertyTypeAlias, _isPreviewing)); }
        }

        internal string ValueSetCacheKey
        {
            get { return _valueSetCacheKey ?? (_valueSetCacheKey = CacheKeys.PropertyValueSet(_contentUid, PropertyTypeAlias, _isPreviewing)); }
        }

        private ValueSet GetValueSet(PropertyCacheLevel cacheLevel)
        {
            ValueSet valueSet;
            Facade facade;
            ICacheProvider cache;
            switch (cacheLevel)
            {
                case PropertyCacheLevel.None:
                    // never cache anything
                    valueSet = new ValueSet();
                    break;
                case PropertyCacheLevel.Content:
                    // cache within the property object itself, ie within the content object
                    valueSet = _valueSet ?? (_valueSet = new ValueSet());
                    break;
                case PropertyCacheLevel.ContentCache:
                    // cache within the snapshot cache, unless previewing, then use the facade or
                    // snapshot cache (if we don't want to pollute the snapshot cache with short-lived
                    // data) depending on settings
                    // for members, always cache in the facade cache - never pollute snapshot cache
                    facade = Facade.Current;
                    cache = facade == null
                        ? null 
                        : ((_isPreviewing == false || FacadeService.FullCacheWhenPreviewing) && (_isMember == false)
                            ? facade.SnapshotCache 
                            : facade.FacadeCache);
                    valueSet = GetValueSet(cache);
                    break;
                case PropertyCacheLevel.Request:
                    // cache within the facade cache
                    facade = Facade.Current;
                    cache = facade == null ? null : facade.FacadeCache;
                    valueSet = GetValueSet(cache);
                    break;
                default:
                    throw new InvalidOperationException("Invalid cache level.");
            }
            return valueSet;
        }

        private ValueSet GetValueSet(ICacheProvider cache)
        {
            if (cache == null) // no cache, don't cache
                return new ValueSet();
            return (ValueSet) cache.GetCacheItem(ValueSetCacheKey, () => new ValueSet());
        }

        private object GetSourceValue()
        {
            var valueSet = GetValueSet(PropertyType.SourceCacheLevel);
            if (valueSet.SourceInitialized == false)
            {
                valueSet.Source = PropertyType.ConvertDataToSource(_dataValue, _isPreviewing);
                valueSet.SourceInitialized = true;
            }
            return valueSet.Source;
        }

        public override object DataValue
        {
            get { return _dataValue; }
        }

        public override object Value
        {
            get
            {
                lock (_locko)
                {
                    var valueSet = GetValueSet(PropertyType.ObjectCacheLevel);
                    if (valueSet.ValueInitialized == false)
                    {
                        valueSet.Value = PropertyType.ConvertSourceToObject(GetSourceValue(), _isPreviewing);
                        valueSet.ValueInitialized = true;
                    }
                    return valueSet.Value;
                }
            }
        }

        public override object XPathValue
        {
            get
            {
                lock (_locko)
                {
                    var valueSet = GetValueSet(PropertyType.XPathCacheLevel);
                    if (valueSet.XPathInitialized == false)
                    {
                        valueSet.XPath = PropertyType.ConvertSourceToXPath(GetSourceValue(), _isPreviewing);
                        valueSet.XPathInitialized = true;
                    }
                    return valueSet.XPath;
                }
            }
        }
    }
}
