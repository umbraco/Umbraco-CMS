using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.Configuration;

internal class JsonConfigManipulator : IConfigManipulator
{
    private const string ConnectionStringObjectName = "ConnectionStrings";
    private const string UmbracoConnectionStringPath = $"{ConnectionStringObjectName}:{Constants.System.UmbracoConnectionName}";
    private const string UmbracoConnectionStringProviderNamePath = UmbracoConnectionStringPath + ConnectionStrings.ProviderNamePostfix;
    private const string CmsObjectPath = "Umbraco:CMS";
    private const string GlobalIdPath = $"{CmsObjectPath}:Global:Id";
    private const string DisableRedirectUrlTrackingPath = $"{CmsObjectPath}:WebRouting:DisableRedirectUrlTracking";

    private readonly JsonDocumentOptions _jsonDocumentOptions = new() { CommentHandling = JsonCommentHandling.Skip };
    private readonly IConfiguration _configuration;
    private readonly ILogger<JsonConfigManipulator> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public JsonConfigManipulator(IConfiguration configuration, ILogger<JsonConfigManipulator> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task RemoveConnectionStringAsync()
    {
        JsonConfigurationProvider? provider = GetJsonConfigurationProvider(UmbracoConnectionStringPath);

        JsonNode? jsonNode = await GetJsonNodeAsync(provider);

        if (jsonNode is null)
        {
            _logger.LogWarning("Failed to remove connection string from JSON configuration");
            return;
        }

        RemoveJsonNode(jsonNode, UmbracoConnectionStringPath);
        RemoveJsonNode(jsonNode, UmbracoConnectionStringProviderNamePath);

        await SaveJsonAsync(provider, jsonNode);
    }

    /// <inheritdoc />
    public async Task SaveConnectionStringAsync(string connectionString, string? providerName)
    {
        JsonConfigurationProvider? provider = GetJsonConfigurationProvider();
        JsonNode? node = await GetJsonNodeAsync(provider);

        if (node is null)
        {
            _logger.LogWarning("Was unable to load the configuration file to save the connection string");
            return;
        }

        CreateOrUpdateJsonNode(node, UmbracoConnectionStringPath, connectionString);
        if (providerName is not null)
        {
            CreateOrUpdateJsonNode(node, UmbracoConnectionStringProviderNamePath, providerName);
        }

        await SaveJsonAsync(provider, node);
    }

    /// <inheritdoc />
    public async Task SaveConfigValueAsync(string itemPath, object value)
    {
        JsonConfigurationProvider? provider = GetJsonConfigurationProvider();
        JsonNode? node = await GetJsonNodeAsync(provider);

        if (node is null)
        {
            _logger.LogWarning("Failed to save configuration key \"{Key}\" in JSON configuration", itemPath);
            return;
        }

        JsonNode? propertyNode = node;
        foreach (var propertyName in itemPath.Split(':'))
        {
            propertyNode = FindChildNode(propertyNode, propertyName);
        }

        if (propertyNode is null)
        {
            return;
        }

        propertyNode.ReplaceWith(value);
        await SaveJsonAsync(provider, node);
    }

    /// <inheritdoc />
    public async Task SaveDisableRedirectUrlTrackingAsync(bool disable)
        => await CreateOrUpdateConfigValueAsync(DisableRedirectUrlTrackingPath, disable);

    /// <inheritdoc />
    public async Task SetGlobalIdAsync(string id)
        => await CreateOrUpdateConfigValueAsync(GlobalIdPath, id);

    /// <summary>
    /// Creates or updates a config value at the specified path.
    /// <remarks>This causes a rewrite of the configuration file.</remarks>
    /// </summary>
    /// <param name="itemPath">Path to update, uses : as the separator.</param>
    /// <param name="value">The value of the node.</param>
    private async Task CreateOrUpdateConfigValueAsync(string itemPath, object value)
    {
        JsonConfigurationProvider? provider = GetJsonConfigurationProvider();
        JsonNode? node = await GetJsonNodeAsync(provider);

        if (node is null)
        {
            _logger.LogWarning("Failed to save configuration key \"{Key}\" in JSON configuration", itemPath);
            return;
        }

        CreateOrUpdateJsonNode(node, itemPath, value);
        await SaveJsonAsync(provider, node);
    }

    /// <summary>
    /// Updates or creates a json node at the specified path.
    /// <remarks>
    /// Will also create any missing nodes in the path.
    /// </remarks>
    /// </summary>
    /// <param name="node">Node to create or update.</param>
    /// <param name="itemPath">Path to create or update, uses : as the separator.</param>
    /// <param name="value">The value of the node.</param>
    private static void CreateOrUpdateJsonNode(JsonNode node, string itemPath, object value)
    {
        // This is required because System.Text.Json has no merge function, and doesn't support patch
        // this is a problem because we don't know if the key(s) exists yet, so we can't simply update it,
        // we may have to create one ore more json objects.

        // First we find the inner most child that already exists.
        var propertyNames = itemPath.Split(':');
        JsonNode propertyNode = node;
        var index = 0;
        foreach (var propertyName in propertyNames)
        {
            JsonNode? found = FindChildNode(propertyNode, propertyName);
            if (found is null)
            {
                break;
            }

            propertyNode = found;
            index++;
        }


        // We can now use the index to go through the remaining keys, creating them as we go.
        while (index < propertyNames.Length)
        {
            var propertyName = propertyNames[index];
            var newNode = new JsonObject();
            propertyNode.AsObject()[propertyName] = newNode;
            propertyNode = newNode;
            index++;
        }

        // System.Text.Json doesn't like just setting an Object as a value, so instead we first create the node,
        // and then replace the value
        propertyNode.ReplaceWith(value);
    }

    private static void RemoveJsonNode(JsonNode node, string key)
    {
        JsonNode? propertyNode = node;
        foreach (var propertyName in key.Split(':'))
        {
            propertyNode = FindChildNode(propertyNode, propertyName);
        }

        propertyNode?.Parent?.AsObject().Remove(propertyNode.GetPropertyName());
    }

    private async Task SaveJsonAsync(JsonConfigurationProvider? provider, JsonNode jsonNode)
    {
        if (provider?.Source.FileProvider is not PhysicalFileProvider physicalFileProvider)
        {
            return;
        }

        await _lock.WaitAsync();

        try
        {
            var jsonFilePath = Path.Combine(physicalFileProvider.Root, provider.Source.Path!);
            await using var jsonConfigStream = new FileStream(jsonFilePath, FileMode.Create);
            await using var writer = new Utf8JsonWriter(jsonConfigStream, new JsonWriterOptions { Indented = true });
            jsonNode.WriteTo(writer);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<JsonNode?> GetJsonNodeAsync(JsonConfigurationProvider? provider)
    {
        if (provider is null)
        {
            return null;
        }

        await _lock.WaitAsync();
        if (provider.Source.FileProvider is not PhysicalFileProvider physicalFileProvider)
        {
            return null;
        }

        var jsonFilePath = Path.Combine(physicalFileProvider.Root, provider.Source.Path!);

        try
        {
            using var streamReader = new StreamReader(jsonFilePath);
            return await JsonNode.ParseAsync(streamReader.BaseStream, documentOptions: _jsonDocumentOptions);
        }
        catch (IOException exception)
        {
            _logger.LogWarning(exception, "JSON configuration could not be read: {Path}", jsonFilePath);
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    private JsonConfigurationProvider? GetJsonConfigurationProvider(string? requiredKey = null)
    {
        if (_configuration is not IConfigurationRoot configurationRoot)
        {
            return null;
        }

        foreach (IConfigurationProvider provider in configurationRoot.Providers)
        {
            if (provider is JsonConfigurationProvider jsonConfigurationProvider &&
                (requiredKey is null || provider.TryGet(requiredKey, out _)))
            {
                return jsonConfigurationProvider;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the immediate child with the specified name, in a case insensitive manner.
    /// </summary>
    /// <remarks>
    /// This is required since keys are case insensitive in IConfiguration.
    /// But not in JsonNode.
    /// </remarks>
    /// <param name="node">The node to search.</param>
    /// <param name="key">The key to search for.</param>
    /// <returns>The found node, null if no match is found.</returns>
    private static JsonNode? FindChildNode(JsonNode? node, string key)
    {
        if (node is null)
        {
            return node;
        }

        foreach (KeyValuePair<string, JsonNode?> property in node.AsObject())
        {
            if (property.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return property.Value;
            }
        }

        return null;
    }
}
