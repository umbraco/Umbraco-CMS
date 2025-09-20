using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

public class TagPropertyIndexValueFactory : JsonPropertyIndexValueFactoryBase<string[]>, ITagPropertyIndexValueFactory
{
    private IndexingSettings _indexingSettings;

    public TagPropertyIndexValueFactory(
        IJsonSerializer jsonSerializer,
        IOptionsMonitor<IndexingSettings> indexingSettings)
        : base(jsonSerializer, indexingSettings)
    {
        ForceExplicitlyIndexEachNestedProperty = true;
        _indexingSettings = indexingSettings.CurrentValue;
        indexingSettings.OnChange(newValue => _indexingSettings = newValue);
    }

    [Obsolete("Use non-obsolete constructor. This will be removed in Umbraco 14.")]
    public TagPropertyIndexValueFactory(IJsonSerializer jsonSerializer)
        : this(jsonSerializer, StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<IndexingSettings>>())
    {

    }

    protected override IEnumerable<KeyValuePair<string, IEnumerable<object?>>> Handle(
        string[] deserializedPropertyValue,
        IProperty property,
        string? culture,
        string? segment,
        bool published,
        IEnumerable<string> availableCultures)
    {
        yield return new KeyValuePair<string, IEnumerable<object?>>(property.Alias, deserializedPropertyValue);
    }

    [Obsolete("Use the overload that specifies availableCultures, scheduled for removal in v14")]
    protected override IEnumerable<KeyValuePair<string, IEnumerable<object?>>> Handle(
        string[] deserializedPropertyValue,
        IProperty property,
        string? culture,
        string? segment,
        bool published)
    {
        yield return new KeyValuePair<string, IEnumerable<object?>>(property.Alias, deserializedPropertyValue);
    }

    public override IEnumerable<KeyValuePair<string, IEnumerable<object?>>> GetIndexValues(
        IProperty property,
        string? culture,
        string? segment,
        bool published,
        IEnumerable<string> availableCultures,
        IDictionary<Guid, IContentType> contentTypeDictionary)
    {
        IEnumerable<KeyValuePair<string, IEnumerable<object?>>> jsonValues = base.GetIndexValues(property, culture, segment, published, availableCultures, contentTypeDictionary);
        if (jsonValues?.Any() is true)
        {
            return jsonValues;
        }

        var result = new List<KeyValuePair<string, IEnumerable<object?>>>();

        var propertyValue = property.GetValue(culture, segment, published);

        // If there is a value, it's a string and it's not empty/white space
        if (propertyValue is string rawValue && !string.IsNullOrWhiteSpace(rawValue))
        {
            var values = rawValue.Split(',', StringSplitOptions.RemoveEmptyEntries);

            result.AddRange(Handle(values, property, culture, segment, published, availableCultures, contentTypeDictionary));
        }

        IEnumerable<KeyValuePair<string, IEnumerable<object?>>> summary = HandleResume(result, property, culture, segment, published);
        if (_indexingSettings.ExplicitlyIndexEachNestedProperty || ForceExplicitlyIndexEachNestedProperty)
        {
            result.AddRange(summary);
            return result;
        }

        return summary;
    }
}
