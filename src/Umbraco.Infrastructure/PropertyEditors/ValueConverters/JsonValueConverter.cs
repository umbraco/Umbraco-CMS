﻿// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters
{
    /// <summary>
    /// The default converter for all property editors that expose a JSON value type
    /// </summary>
    /// <remarks>
    /// Since this is a default (umbraco) converter it will be ignored if another converter found conflicts with this one.
    /// </remarks>
    [DefaultPropertyValueConverter]
    public class JsonValueConverter : PropertyValueConverterBase
    {
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly ILogger<JsonValueConverter> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonValueConverter"/> class.
        /// </summary>
        public JsonValueConverter(PropertyEditorCollection propertyEditors, ILogger<JsonValueConverter> logger)
        {
            _propertyEditors = propertyEditors;
            _logger = logger;
        }

        /// <summary>
        /// It is a converter for any value type that is "JSON"
        /// </summary>
        /// <param name="propertyType"></param>
        /// <returns></returns>
        public override bool IsConverter(IPublishedPropertyType propertyType)
        {
            return _propertyEditors.TryGet(propertyType.EditorAlias, out var editor)
                   && editor.GetValueEditor().ValueType.InvariantEquals(ValueTypes.Json);
        }

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof (JToken);

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;
            var sourceString = source.ToString();

            if (sourceString.DetectIsJson())
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject(sourceString);
                    return obj;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not parse the string '{JsonString}' to a json object", sourceString);
                }
            }

            //it's not json, just return the string
            return sourceString;
        }

        // TODO: Now to convert that to XPath!
    }
}
