using System.Text;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

internal abstract class NestedPropertyIndexValueFactoryBase<TSerialized, TItem> : JsonPropertyIndexValueFactoryBase<TSerialized>
    {
        private readonly PropertyEditorCollection _propertyEditorCollection;

        protected NestedPropertyIndexValueFactoryBase(PropertyEditorCollection propertyEditorCollection,
            IJsonSerializer jsonSerializer): base(jsonSerializer)
        {
            _propertyEditorCollection = propertyEditorCollection;
        }

        protected override IEnumerable<KeyValuePair<string, IEnumerable<object?>>> Handle(
            TSerialized deserializedObject,
            IProperty property,
            string? culture,
            string? segment,
            bool published)
        {
            var result = new List<KeyValuePair<string, IEnumerable<object?>>>();

            foreach (var nestedContentRowValue in GetDataItems(deserializedObject))
            {
                IContentType? contentType = GetContentTypeOfNestedItem(nestedContentRowValue);

                if (contentType is null)
                {
                    continue;
                }

                Dictionary<string, IPropertyType> propertyTypeDictionary =
                    contentType
                        .PropertyGroups
                        .SelectMany(x => x.PropertyTypes!)
                        .ToDictionary(x => x.Alias);

                result.AddRange(GetNestedResults(property.Alias, culture, segment, published,
                    propertyTypeDictionary, nestedContentRowValue));
            }

            return MoveRawKeySegmentsToStart(result);
        }

        private IEnumerable<KeyValuePair<string, IEnumerable<object?>>> MoveRawKeySegmentsToStart(List<KeyValuePair<string, IEnumerable<object?>>> indexContent)
        {
            foreach (KeyValuePair<string, IEnumerable<object?>> indexedKeyValuePair in indexContent)
            {
                //Tests if key includes the RawFieldPrefix, and it is not in the start
                if (indexedKeyValuePair.Key.Substring(1).Contains(UmbracoExamineFieldNames.RawFieldPrefix))
                {
                    var newKey = UmbracoExamineFieldNames.RawFieldPrefix + indexedKeyValuePair.Key.Replace(UmbracoExamineFieldNames.RawFieldPrefix, string.Empty);
                    yield return new KeyValuePair<string, IEnumerable<object?>>(newKey, indexedKeyValuePair.Value);
                }
                else
                {
                    yield return indexedKeyValuePair;
                }
            }

        }

        protected abstract IContentType? GetContentTypeOfNestedItem(TItem input);
        protected abstract IDictionary<string, object?> GetRawProperty(TItem nestedContentRowValue);

        protected abstract IEnumerable<TItem> GetDataItems(TSerialized input);

        protected override IEnumerable<KeyValuePair<string, IEnumerable<object?>>> HandleResume(List<KeyValuePair<string, IEnumerable<object?>>> indexedContent, IProperty property, string? culture, string? segment, bool published)
        {
            yield return new KeyValuePair<string, IEnumerable<object?>>(property.Alias, GetResumeFromAllContent(indexedContent).Yield());
        }

        /// <summary>
        /// Gets a resume as string of all the content in this nested type.
        /// </summary>
        /// <param name="indexedContent">All the indexed content for this property.</param>
        /// <returns>the string with all relevant content from </returns>
        private static string GetResumeFromAllContent(List<KeyValuePair<string, IEnumerable<object?>>> indexedContent)
        {
            var stringBuilder = new StringBuilder();
            foreach ((var indexKey, IEnumerable<object?>? indexedValue) in indexedContent)
            {
                //Ignore Raw fields
                if (indexKey.Contains(UmbracoExamineFieldNames.RawFieldPrefix))
                {
                    continue;
                }

                foreach (var value in indexedValue)
                {
                    if (value is not null)
                    {
                        stringBuilder.AppendLine(value.ToString());
                    }
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        ///  Gets the content to index for a
        /// </summary>
        private IList<KeyValuePair<string, IEnumerable<object?>>> GetNestedResults(
            string keyPrefix,
            string? culture,
            string? segment,
            bool published,
            IDictionary<string, IPropertyType> propertyTypeDictionary,
            TItem nestedContentRowValue)
        {
            var result = new List<KeyValuePair<string, IEnumerable<object?>>>();

            var blockIndex = 0;

            foreach ((var propertyAlias, var propertyValue) in GetRawProperty(nestedContentRowValue))
            {
                if (propertyTypeDictionary.TryGetValue(propertyAlias, out IPropertyType? propertyType))
                {
                    IProperty subProperty = new Property(propertyType);
                    subProperty.SetValue(propertyValue);
                    IDataEditor? editor = _propertyEditorCollection[propertyType.PropertyEditorAlias];
                    if (editor is null)
                    {
                        continue;
                    }

                    IEnumerable<KeyValuePair<string, IEnumerable<object?>>> indexValues =
                        editor.PropertyIndexValueFactory.GetIndexValues(subProperty, culture, segment, published);

                    foreach ((var nestedAlias, IEnumerable<object?> nestedValue) in indexValues)
                    {
                        result.Add(new KeyValuePair<string, IEnumerable<object?>>(
                            $"{keyPrefix}.items[{blockIndex}].{nestedAlias}", nestedValue!));
                    }
                }

                blockIndex++;
            }

            return result;
        }
    }
