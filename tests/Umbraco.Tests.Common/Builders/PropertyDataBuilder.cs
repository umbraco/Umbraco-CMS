using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Infrastructure.PublishedCache.DataSource;

namespace Umbraco.Cms.Tests.Common.Builders;

public class PropertyDataBuilder : BuilderBase<Dictionary<string, PropertyData[]>>
{
    private readonly Dictionary<string, List<PropertyData>> _properties = new();

    public PropertyDataBuilder WithPropertyData(string alias, PropertyData propertyData)
    {
        if (!_properties.TryGetValue(alias, out var propertyDataCollection))
        {
            propertyDataCollection = new List<PropertyData>();
            _properties[alias] = propertyDataCollection;
        }

        propertyDataCollection.Add(propertyData);

        return this;
    }

    public PropertyDataBuilder WithPropertyData(string alias, object value, string? culture = null, string? segment = null)
        => WithPropertyData(alias, new PropertyData { Culture = culture ?? string.Empty, Segment = segment ?? string.Empty, Value = value });

    public override Dictionary<string, PropertyData[]> Build()
        => _properties.ToDictionary(x => x.Key, x => x.Value.ToArray());
}
