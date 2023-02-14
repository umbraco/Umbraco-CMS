using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Infrastructure.Serialization;

// FIXME: move away from Json.NET; this is a temporary fix that attempts to use System.Text.Json for management API operations, Json.NET for other operations
public class ContextualJsonSerializer : IJsonSerializer
{
    private readonly IRequestAccessor _requestAccessor;
    private readonly IJsonSerializer _jsonNetSerializer;
    private readonly IJsonSerializer _systemTextSerializer;

    public ContextualJsonSerializer(IRequestAccessor requestAccessor)
    {
        _requestAccessor = requestAccessor;
        _jsonNetSerializer = new JsonNetSerializer();
        _systemTextSerializer = new SystemTextJsonSerializer();
    }

    public string Serialize(object? input) => ContextualizedSerializer().Serialize(input);

    public T? Deserialize<T>(string input) => ContextualizedSerializer().Deserialize<T>(input);

    public T? DeserializeSubset<T>(string input, string key) => throw new NotSupportedException();

    private IJsonSerializer ContextualizedSerializer()
    {
        try
        {
            var requestedPath = _requestAccessor.GetRequestUrl()?.AbsolutePath;
            if (requestedPath != null)
            {
                // add white listed paths for the System.Text.Json config serializer here
                // - always use it for the new management API
                if (requestedPath.Contains("/umbraco/management/api/"))
                {
                    return _systemTextSerializer;
                }
            }
        }
        catch (Exception ex)
        {
            // ignore - this whole thing is a temporary workaround, let's not make a fuss
        }

        return _jsonNetSerializer;
    }
}


