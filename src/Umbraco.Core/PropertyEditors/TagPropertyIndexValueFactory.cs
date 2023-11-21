using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors;

public class TagPropertyIndexValueFactory : JsonPropertyIndexValueFactoryBase<string[]>, ITagPropertyIndexValueFactory
{
    public TagPropertyIndexValueFactory(
        IJsonSerializer jsonSerializer,
        IOptionsMonitor<IndexingSettings> indexingSettings)
        : base(jsonSerializer, indexingSettings)
    {
        ForceExplicitlyIndexEachNestedProperty = true;
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
}
