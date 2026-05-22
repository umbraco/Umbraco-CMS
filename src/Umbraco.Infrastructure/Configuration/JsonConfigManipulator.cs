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

/// <summary>
/// Default <see cref="IConfigManipulator" /> implementation that persists configuration values back to the
/// underlying JSON files registered as <see cref="JsonConfigurationProvider" /> sources on the applications <see cref="IConfigurationRoot" />.
/// </summary>
internal sealed class JsonConfigManipulator : IConfigManipulator
{
    private const string ConnectionStringsObjectName = "ConnectionStrings";
    private const string UmbracoConnectionStringPath = $"{ConnectionStringsObjectName}:{Constants.System.UmbracoConnectionName}";
    private const string UmbracoConnectionStringProviderNamePath = UmbracoConnectionStringPath + ConnectionStrings.ProviderNamePostfix;
    private const string DisableRedirectUrlTrackingPath = Constants.Configuration.ConfigWebRouting + ":DisableRedirectUrlTracking";
    private const string ImagingHmacSecretKeyPath = Constants.Configuration.ConfigImaging + ":HMACSecretKey";

    // Allowlist of filenames created on first write when the source is registered but missing on disk.
    private static readonly string[] _creatableFileNames =
    [
        "appsettings.Local.json",
    ];

    private readonly JsonDocumentOptions _jsonDocumentOptions = new() { CommentHandling = JsonCommentHandling.Skip };
    private readonly IConfiguration _configuration;
    private readonly ILogger<JsonConfigManipulator> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonConfigManipulator"/> class.
    /// </summary>
    /// <param name="configuration">The <see cref="IConfiguration"/> instance containing configuration settings.</param>
    /// <param name="logger">The <see cref="ILogger{JsonConfigManipulator}"/> instance used for logging operations.</param>
    public JsonConfigManipulator(IConfiguration configuration, ILogger<JsonConfigManipulator> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SaveConnectionStringAsync(string connectionString, string? providerName)
    {
        if (await LoadJsonAsync(preferLast: true) is not (var provider, var node))
        {
            _logger.LogWarning("Was unable to load the configuration file to save the connection string");
            return;
        }

        SetJsonValue(node, UmbracoConnectionStringPath, connectionString);
        if (providerName is not null)
        {
            SetJsonValue(node, UmbracoConnectionStringProviderNamePath, providerName);
        }

        await WriteJsonAsync(provider, node);

        // Reload synchronously so IOptionsMonitor.CurrentValue sees the new value before this returns —
        // the file watcher reload races with OnChange callbacks reading stale data.
        if (_configuration is IConfigurationRoot configurationRoot)
        {
            configurationRoot.Reload();
        }
    }

    /// <inheritdoc />
    public async Task RemoveConnectionStringAsync()
    {
        if (await LoadJsonAsync(preferLast: true) is not (var provider, var node))
        {
            _logger.LogWarning("Failed to remove connection string from JSON configuration");
            return;
        }

        RemoveJsonValue(node, UmbracoConnectionStringPath);
        RemoveJsonValue(node, UmbracoConnectionStringProviderNamePath);

        await WriteJsonAsync(provider, node);
    }

    /// <inheritdoc />
    [Obsolete("This method is no longer used by Umbraco. Scheduled for removal in Umbraco 19.")]
    public async Task SaveConfigValueAsync(string itemPath, object value)
    {
        if (await LoadJsonAsync(preferLast: false) is not (var provider, var node))
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
        await WriteJsonAsync(provider, node);
    }

    /// <inheritdoc />
    [Obsolete("This method is no longer used by Umbraco. Set the Umbraco:CMS:WebRouting:DisableRedirectUrlTracking configuration key instead. Scheduled for removal in Umbraco 19.")]
    public Task SaveDisableRedirectUrlTrackingAsync(bool disable)
        => SetConfigValueAsync(DisableRedirectUrlTrackingPath, disable);

    /// <inheritdoc />
    public Task SetGlobalIdAsync(string id)
        => SetConfigValueAsync(Constants.Configuration.ConfigGlobalId, id);

    /// <inheritdoc />
    public Task SetImagingHmacSecretKeyAsync(string base64Key)
        => SetConfigValueAsync(ImagingHmacSecretKeyPath, base64Key);

    /// <summary>
    /// Creates or updates a configuration value at the specified path in the base JSON configuration source.
    /// </summary>
    private async Task SetConfigValueAsync(string itemPath, object value)
    {
        if (await LoadJsonAsync(preferLast: false) is not (var provider, var node))
        {
            _logger.LogWarning("Failed to save configuration key \"{Key}\" in JSON configuration", itemPath);
            return;
        }

        SetJsonValue(node, itemPath, value);
        await WriteJsonAsync(provider, node);
    }

    /// <summary>
    /// Creates or updates a JSON value at the specified path, creating intermediate object nodes as needed.
    /// </summary>
    private static void SetJsonValue(JsonNode node, string itemPath, object value)
    {
        // System.Text.Json has no merge/patch support, so we walk the tree manually.
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

        while (index < propertyNames.Length)
        {
            var newNode = new JsonObject();
            propertyNode.AsObject()[propertyNames[index]] = newNode;
            propertyNode = newNode;
            index++;
        }

        propertyNode.ReplaceWith(value);
    }

    /// <summary>
    /// Removes the JSON value at the specified path, if it exists.
    /// </summary>
    private static void RemoveJsonValue(JsonNode node, string itemPath)
    {
        JsonNode? propertyNode = node;
        foreach (var propertyName in itemPath.Split(':'))
        {
            propertyNode = FindChildNode(propertyNode, propertyName);
        }

        propertyNode?.Parent?.AsObject().Remove(propertyNode.GetPropertyName());
    }

    /// <summary>
    /// Locates the first or last writable JSON configuration provider and loads its content as a <see cref="JsonNode"/> tree.
    /// </summary>
    /// <param name="preferLast">If true, walks providers in reverse order to find the most specific override.</param>
    /// <returns>The provider/node pair, or null when no writable JSON source is available or its file cannot be read.</returns>
    private async Task<(JsonConfigurationProvider Provider, JsonNode Node)?> LoadJsonAsync(bool preferLast)
    {
        if (_configuration is not IConfigurationRoot configurationRoot)
        {
            return null;
        }

        IEnumerable<IConfigurationProvider> providers = preferLast
            ? configurationRoot.Providers.Reverse()
            : configurationRoot.Providers;

        foreach (IConfigurationProvider configurationProvider in providers)
        {
            if (configurationProvider is not JsonConfigurationProvider jsonProvider)
            {
                continue;
            }

            if (TryGetFilePath(jsonProvider, out var jsonFilePath) is false ||
                (File.Exists(jsonFilePath) is false && IsCreatableFile(jsonFilePath) is false))
            {
                continue;
            }

            JsonNode? node = await ReadJsonAsync(jsonFilePath);

            return node is null
                ? null
                : (jsonProvider, node);
        }

        return null;

        async Task<JsonNode?> ReadJsonAsync(string jsonFilePath)
        {
            await _lock.WaitAsync();

            try
            {
                if (File.Exists(jsonFilePath) is false)
                {
                    return new JsonObject
                    {
                        ["$schema"] = "./appsettings-schema.json",
                    };
                }

                await using var jsonConfigStream = File.OpenRead(jsonFilePath);

                return await JsonNode.ParseAsync(jsonConfigStream, documentOptions: _jsonDocumentOptions);
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
    }

    /// <summary>
    /// Writes the JSON node back to the file backing the specified provider.
    /// </summary>
    private async Task WriteJsonAsync(JsonConfigurationProvider provider, JsonNode node)
    {
        if (TryGetFilePath(provider, out var jsonFilePath) is false)
        {
            return;
        }

        await _lock.WaitAsync();

        try
        {
            await using var jsonConfigStream = new FileStream(jsonFilePath, FileMode.Create);
            await using var writer = new Utf8JsonWriter(jsonConfigStream, new JsonWriterOptions { Indented = true });
            node.WriteTo(writer);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Resolves the absolute file path for a JSON configuration provider backed by a <see cref="PhysicalFileProvider"/>.
    /// </summary>
    private static bool TryGetFilePath(JsonConfigurationProvider provider, out string filePath)
    {
        if (provider.Source is { FileProvider: PhysicalFileProvider physicalFileProvider, Path: { } sourcePath })
        {
            filePath = Path.Combine(physicalFileProvider.Root, sourcePath);
            return true;
        }

        filePath = string.Empty;
        return false;
    }

    /// <summary>
    /// Returns true when the file is allowlisted to be created on first write (see <see cref="_creatableFileNames"/>).
    /// </summary>
    private static bool IsCreatableFile(string filePath) =>
        _creatableFileNames.Contains(Path.GetFileName(filePath), StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Finds the immediate child with the specified name, in a case-insensitive manner.
    /// </summary>
    /// <remarks>This is required since keys are case-insensitive in <see cref="IConfiguration"/> but not in <see cref="JsonNode"/>.</remarks>
    private static JsonNode? FindChildNode(JsonNode? node, string key)
    {
        if (node is null)
        {
            return null;
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
