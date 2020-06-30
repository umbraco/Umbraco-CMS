using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Blocks;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Abstract class for block editor based editors
    /// </summary>
    public abstract class BlockEditorPropertyEditor : DataEditor
    {
        public const string ContentTypeKeyPropertyKey = "contentTypeKey";
        public const string UdiPropertyKey = "udi";
        private readonly IBlockEditorDataHelper _dataHelper;
        private readonly Lazy<PropertyEditorCollection> _propertyEditors;
        private readonly IDataTypeService _dataTypeService;
        private readonly IContentTypeService _contentTypeService;

        public BlockEditorPropertyEditor(ILogger logger, Lazy<PropertyEditorCollection> propertyEditors, IDataTypeService dataTypeService, IContentTypeService contentTypeService, IBlockEditorDataHelper dataHelper)
            : base(logger)
        {
            _dataHelper = dataHelper;
            _propertyEditors = propertyEditors;
            _dataTypeService = dataTypeService;
            _contentTypeService = contentTypeService;
        }

        // has to be lazy else circular dep in ctor
        private PropertyEditorCollection PropertyEditors => _propertyEditors.Value;

        #region Value Editor

        protected override IDataValueEditor CreateValueEditor() => new BlockEditorPropertyValueEditor(Attribute, _dataHelper, PropertyEditors, _dataTypeService, _contentTypeService);

        internal class BlockEditorPropertyValueEditor : DataValueEditor, IDataValueReference
        {
            private readonly IBlockEditorDataHelper _dataHelper;
            private readonly PropertyEditorCollection _propertyEditors;
            private readonly IDataTypeService _dataTypeService;
            private readonly BlockEditorValues _blockEditorValues;

            public BlockEditorPropertyValueEditor(DataEditorAttribute attribute, IBlockEditorDataHelper dataHelper, PropertyEditorCollection propertyEditors, IDataTypeService dataTypeService, IContentTypeService contentTypeService)
                : base(attribute)
            {
                _dataHelper = dataHelper;
                _propertyEditors = propertyEditors;
                _dataTypeService = dataTypeService;
                _blockEditorValues = new BlockEditorValues(dataHelper, contentTypeService);
                Validators.Add(new BlockEditorValidator(propertyEditors, dataTypeService, _blockEditorValues));
            }

            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                var rawJson = value == null ? string.Empty : value is string str ? str : value.ToString();

                var result = new List<UmbracoEntityReference>();

                foreach (var row in _blockEditorValues.GetPropertyValues(rawJson, out _))
                {
                    if (row.PropType == null) continue;

                    var propEditor = _propertyEditors[row.PropType.PropertyEditorAlias];

                    var valueEditor = propEditor?.GetValueEditor();
                    if (!(valueEditor is IDataValueReference reference)) continue;

                    var val = row.JsonRowValue[row.PropKey]?.ToString();

                    var refs = reference.GetReferences(val);

                    result.AddRange(refs);
                }

                return result;
            }
        }

        internal class BlockEditorValidator : IValueValidator
        {
            private readonly PropertyEditorCollection _propertyEditors;
            private readonly IDataTypeService _dataTypeService;
            private readonly BlockEditorValues _blockEditorValues;

            public BlockEditorValidator(PropertyEditorCollection propertyEditors, IDataTypeService dataTypeService, BlockEditorValues blockEditorValues)
            {
                _propertyEditors = propertyEditors;
                _dataTypeService = dataTypeService;
                _blockEditorValues = blockEditorValues;
            }

            public IEnumerable<ValidationResult> Validate(object rawValue, string valueType, object dataTypeConfiguration)
            {
                var validationResults = new List<ValidationResult>();

                foreach (var row in _blockEditorValues.GetPropertyValues(rawValue, out _))
                {
                    if (row.PropType == null) continue;

                    var config = _dataTypeService.GetDataType(row.PropType.DataTypeId).Configuration;
                    var propertyEditor = _propertyEditors[row.PropType.PropertyEditorAlias];
                    if (propertyEditor == null) continue;

                    foreach (var validator in propertyEditor.GetValueEditor().Validators)
                    {
                        foreach (var result in validator.Validate(row.JsonRowValue[row.PropKey], propertyEditor.GetValueEditor().ValueType, config))
                        {
                            result.ErrorMessage = "Item " + (row.RowIndex + 1) + " '" + row.PropType.Name + "' " + result.ErrorMessage;
                            validationResults.Add(result);
                        }
                    }

                    // Check mandatory
                    if (row.PropType.Mandatory)
                    {
                        if (row.JsonRowValue[row.PropKey] == null)
                        {
                            var message = string.IsNullOrWhiteSpace(row.PropType.MandatoryMessage)
                                                      ? $"'{row.PropType.Name}' cannot be null"
                                                      : row.PropType.MandatoryMessage;
                            validationResults.Add(new ValidationResult($"Item {(row.RowIndex + 1)}: {message}", new[] { row.PropKey }));
                        }
                        else if (row.JsonRowValue[row.PropKey].ToString().IsNullOrWhiteSpace() || (row.JsonRowValue[row.PropKey].Type == JTokenType.Array && !row.JsonRowValue[row.PropKey].HasValues))
                        {
                            var message = string.IsNullOrWhiteSpace(row.PropType.MandatoryMessage)
                                                      ? $"'{row.PropType.Name}' cannot be empty"
                                                      : row.PropType.MandatoryMessage;
                            validationResults.Add(new ValidationResult($"Item {(row.RowIndex + 1)}: {message}", new[] { row.PropKey }));
                        }
                    }

                    // Check regex
                    if (!row.PropType.ValidationRegExp.IsNullOrWhiteSpace()
                        && row.JsonRowValue[row.PropKey] != null && !row.JsonRowValue[row.PropKey].ToString().IsNullOrWhiteSpace())
                    {
                        var regex = new Regex(row.PropType.ValidationRegExp);
                        if (!regex.IsMatch(row.JsonRowValue[row.PropKey].ToString()))
                        {
                            var message = string.IsNullOrWhiteSpace(row.PropType.ValidationRegExpMessage)
                                                      ? $"'{row.PropType.Name}' is invalid, it does not match the correct pattern"
                                                      : row.PropType.ValidationRegExpMessage;
                            validationResults.Add(new ValidationResult($"Item {(row.RowIndex + 1)}: {message}", new[] { row.PropKey }));
                        }
                    }
                }

                return validationResults;
            }
        }

        internal class BlockEditorValues
        {
            private readonly IBlockEditorDataHelper _dataHelper;
            private readonly Lazy<Dictionary<Guid, IContentType>> _contentTypes;

            public BlockEditorValues(IBlockEditorDataHelper dataHelper, IContentTypeService contentTypeService)
            {
                _dataHelper = dataHelper;
                _contentTypes = new Lazy<Dictionary<Guid, IContentType>>(() => contentTypeService.GetAll().ToDictionary(c => c.Key));
            }

            private IContentType GetElementType(JObject item)
            {
                Guid contentTypeKey = item[ContentTypeKeyPropertyKey]?.ToObject<Guid>() ?? Guid.Empty;
                _contentTypes.Value.TryGetValue(contentTypeKey, out var contentType);
                return contentType;
            }

            public IEnumerable<RowValue> GetPropertyValues(object propertyValue, out List<JObject> deserialized)
            {
                var rowValues = new List<RowValue>();

                deserialized = null;

                if (propertyValue == null || string.IsNullOrWhiteSpace(propertyValue.ToString()))
                    return Enumerable.Empty<RowValue>();

                var data = JsonConvert.DeserializeObject<BlockEditorData>(propertyValue.ToString());
                if (data?.Layout == null || data.Data == null || data.Data.Count == 0)
                    return Enumerable.Empty<RowValue>();

                var blockRefs = _dataHelper.GetBlockReferences(data.Layout);
                if (blockRefs == null)
                    return Enumerable.Empty<RowValue>();

                var dataMap = new Dictionary<Udi, JObject>(data.Data.Count);
                data.Data.ForEach(d =>
                {
                    var udiObj = d?[UdiPropertyKey];
                    if (Udi.TryParse(udiObj == null || udiObj.Type != JTokenType.String ? null : udiObj.ToString(), out var udi))
                        dataMap[udi] = d;
                });

                deserialized = blockRefs.Select(r => dataMap.TryGetValue(r.Udi, out var block) ? block : null).Where(r => r != null).ToList();
                if (deserialized == null || deserialized.Count == 0)
                    return Enumerable.Empty<RowValue>();

                var index = 0;

                foreach (var o in deserialized)
                {
                    var propValues = o;

                    var contentType = GetElementType(propValues);
                    if (contentType == null)
                        continue;

                    var propertyTypes = contentType.CompositionPropertyTypes.ToDictionary(x => x.Alias, x => x);
                    var propAliases = propValues.Properties().Select(x => x.Name);
                    foreach (var propAlias in propAliases)
                    {
                        propertyTypes.TryGetValue(propAlias, out var propType);
                        rowValues.Add(new RowValue(propAlias, propType, propValues, index));
                    }
                    index++;
                }

                return rowValues;
            }

            internal class RowValue
            {
                public RowValue(string propKey, PropertyType propType, JObject propValues, int index)
                {
                    PropKey = propKey ?? throw new ArgumentNullException(nameof(propKey));
                    PropType = propType;
                    JsonRowValue = propValues ?? throw new ArgumentNullException(nameof(propValues));
                    RowIndex = index;
                }

                /// <summary>
                /// The current property key being iterated for the row value
                /// </summary>
                public string PropKey { get; }

                /// <summary>
                /// The <see cref="PropertyType"/> of the value (if any), this may be null
                /// </summary>
                public PropertyType PropType { get; }

                /// <summary>
                /// The json values for the current row
                /// </summary>
                public JObject JsonRowValue { get; }

                /// <summary>
                /// The Nested Content row index
                /// </summary>
                public int RowIndex { get; }
            }

            private class BlockEditorData
            {
                [JsonProperty("layout")]
                public JObject Layout { get; set; }

                [JsonProperty("data")]
                public List<JObject> Data { get; set; }
            }
        }
        #endregion

        private static bool IsSystemPropertyKey(string propertyKey) => ContentTypeKeyPropertyKey == propertyKey || UdiPropertyKey == propertyKey;
    }
}
