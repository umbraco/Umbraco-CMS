using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Infrastructure.Serialization;

public class ContextualConfigurationEditorJsonSerializer : IConfigurationEditorJsonSerializer
{
    private readonly IRequestAccessor _requestAccessor;
    private readonly ConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;
    private readonly SystemTextConfigurationEditorJsonSerializer _systemTextConfigurationEditorJsonSerializer;

    public ContextualConfigurationEditorJsonSerializer(IRequestAccessor requestAccessor)
    {
        _requestAccessor = requestAccessor;
        _configurationEditorJsonSerializer = new ConfigurationEditorJsonSerializer();
        _systemTextConfigurationEditorJsonSerializer = new SystemTextConfigurationEditorJsonSerializer();
    }

    public string Serialize(object? input) => ContextualizedSerializer().Serialize(input);

    public T? Deserialize<T>(string input) => ContextualizedSerializer().Deserialize<T>(input);

    public T? DeserializeSubset<T>(string input, string key) => throw new NotSupportedException();

    private IConfigurationEditorJsonSerializer ContextualizedSerializer()
    {
        var requestedPath = _requestAccessor.GetRequestUrl()?.AbsolutePath;
        if (requestedPath == null)
        {
            return _configurationEditorJsonSerializer;
        }

        // always use System.Text.Json config serializer for the new management API
        if (requestedPath.Contains("/umbraco/management/api/"))
        {
            return _systemTextConfigurationEditorJsonSerializer;
        }

        return _configurationEditorJsonSerializer;
    }
}
