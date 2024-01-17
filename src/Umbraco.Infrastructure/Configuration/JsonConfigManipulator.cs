using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Configuration.Models;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Umbraco.Cms.Core.Configuration
{
    public class JsonConfigManipulator : IConfigManipulator
    {
        private const string ConnectionStringObjectName = "ConnectionStrings";
        private const string UmbracoConnectionStringPath = $"{ConnectionStringObjectName}:{Constants.System.UmbracoConnectionName}";
        private const string UmbracoConnectionStringProviderNamePath = UmbracoConnectionStringPath + ConnectionStrings.ProviderNamePostfix;

        private readonly IConfiguration _configuration;
        private readonly ILogger<JsonConfigManipulator> _logger;
        private readonly object _locker = new object();
        private readonly SemaphoreSlim _lock = new(1, 1);

        public JsonConfigManipulator(IConfiguration configuration, ILogger<JsonConfigManipulator> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void RemoveConnectionString()
            => RemoveConnectionStringAsync().GetAwaiter().GetResult();

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

        public void SaveConnectionString(string connectionString, string? providerName)
            => SaveConnectionStringAsync(connectionString, providerName).GetAwaiter().GetResult();

        public async Task SaveConnectionStringAsync(string connectionString, string? providerName)
        {
            JsonConfigurationProvider? provider = GetJsonConfigurationProvider();

            JsonNode? jsonNode = await GetJsonNodeAsync(provider);

            if (jsonNode is null)
            {
                _logger.LogWarning("Failed to save connection string in JSON configuration");
                return;
            }

            JsonObject connectionItem = CreateConnectionItem(connectionString, providerName);
            jsonNode[ConnectionStringObjectName] = connectionItem;
            await SaveJsonAsync(provider, jsonNode);
        }

        public void SaveConfigValue(string key, object value)
            => SaveConfigValueAsync(key, value).GetAwaiter().GetResult();

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

        public async Task CreateOrUpdateConfigValueAsync(string itemPath, object value)
        {
            // This is required because System.Text.Json has no merge function, and doesn't support patch
            // this is a problem because we don't know if the key(s) exists yet, so we can't simply update it,
            // we may have to create one ore more json objects.

            JsonConfigurationProvider? provider = GetJsonConfigurationProvider();
            JsonNode? node = await GetJsonNodeAsync(provider);

            if (node is null)
            {
                _logger.LogWarning("Failed to save configuration key \"{Key}\" in JSON configuration", itemPath);
                return;
            }

            // First we find the inner most child that already exists.
            var propertyNames = itemPath.Split(':');
            JsonNode propertyNode = node;
            var currentIndex = 0;
            foreach (var propertyName in propertyNames)
            {
                JsonNode? found = FindChildNode(propertyNode, propertyName);
                if (found is null)
                {
                    break;
                }

                propertyNode = found;
                currentIndex++;
            }


            while(currentIndex < propertyNames.Length)
            {
                var propertyName = propertyNames[currentIndex];
                var newNode = new JsonObject();
                propertyNode.AsObject()[propertyName] = newNode;
                propertyNode = newNode;
                currentIndex++;
            }

            propertyNode.ReplaceWith(value);
            await SaveJsonAsync(provider, node);

        }

        public void SaveDisableRedirectUrlTracking(bool disable)
            => SaveDisableRedirectUrlTrackingAsync(disable).GetAwaiter().GetResult();

        public async Task SaveDisableRedirectUrlTrackingAsync(bool disable)
            => await CreateOrUpdateConfigValueAsync("Umbraco:CMS:WebRouting:DisableRedirectUrlTracking", disable);

        public void SetGlobalId(string id)
        {
            // Save key to JSON
            var provider = GetJsonConfigurationProvider();

            var json = GetJson(provider);
            if (json is null)
            {
                _logger.LogWarning("Failed to save global identifier in JSON configuration.");
                return;
            }

            var item = GetGlobalIdItem(id);
            if (item is not null)
            {
                json.Merge(item, new JsonMergeSettings());
            }

            SaveJson(provider, json);
        }

        private object? GetGlobalIdItem(string id)
        {
            JTokenWriter writer = new JTokenWriter();

            writer.WriteStartObject();
            writer.WritePropertyName("Umbraco");
            writer.WriteStartObject();
            writer.WritePropertyName("CMS");
            writer.WriteStartObject();
            writer.WritePropertyName("Global");
            writer.WriteStartObject();
            writer.WritePropertyName("Id");
            writer.WriteValue(id);
            writer.WriteEndObject();
            writer.WriteEndObject();
            writer.WriteEndObject();
            writer.WriteEndObject();

            return writer.Token;
        }

        private JToken? GetDisableRedirectUrlItem(bool value)
        {
            JTokenWriter writer = new JTokenWriter();

            writer.WriteStartObject();
            writer.WritePropertyName("Umbraco");
            writer.WriteStartObject();
            writer.WritePropertyName("CMS");
            writer.WriteStartObject();
            writer.WritePropertyName("WebRouting");
            writer.WriteStartObject();
            writer.WritePropertyName("DisableRedirectUrlTracking");
            writer.WriteValue(value);
            writer.WriteEndObject();
            writer.WriteEndObject();
            writer.WriteEndObject();
            writer.WriteEndObject();

            return writer.Token;
        }

        private JsonObject CreateConnectionItem(string connectionString, string? providerName)
        {
            var connectionObject = new JsonObject
            {
                [Constants.System.UmbracoConnectionName] = connectionString,
            };

            if (string.IsNullOrEmpty(providerName))
            {
                return connectionObject;
            }

            connectionObject[Constants.System.UmbracoConnectionName + ConnectionStrings.ProviderNamePostfix] = providerName;
            return connectionObject;
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

        private void SaveJson(JsonConfigurationProvider? provider, JObject? json)
        {
            if (provider is null)
            {
                return;
            }

            lock (_locker)
            {
                if (provider.Source.FileProvider is PhysicalFileProvider physicalFileProvider)
                {
                    var jsonFilePath = Path.Combine(physicalFileProvider.Root, provider.Source.Path!);

                    try
                    {
                        using (var sw = new StreamWriter(jsonFilePath, false))
                        using (var jsonTextWriter = new JsonTextWriter(sw)
                        {
                            Formatting = Formatting.Indented,
                        })
                        {
                            json?.WriteTo(jsonTextWriter);
                        }
                    }
                    catch (IOException exception)
                    {
                        _logger.LogWarning(exception, "JSON configuration could not be written: {path}", jsonFilePath);
                    }
                }
            }
        }

        private async Task SaveJsonAsync(JsonConfigurationProvider? provider, JsonNode jsonNode)
        {
            if (provider is null)
            {
                return;
            }

            if (provider.Source.FileProvider is not PhysicalFileProvider physicalFileProvider)
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

        private JObject? GetJson(JsonConfigurationProvider? provider)
        {
            if (provider is null)
            {
                return null;
            }

            lock (_locker)
            {
                if (provider.Source.FileProvider is not PhysicalFileProvider physicalFileProvider)
                {
                    return null;
                }

                var jsonFilePath = Path.Combine(physicalFileProvider.Root, provider.Source.Path!);

                try
                {
                    var serializer = new JsonSerializer();
                    using var sr = new StreamReader(jsonFilePath);
                    using var jsonTextReader = new JsonTextReader(sr);
                    return serializer.Deserialize<JObject>(jsonTextReader);
                }
                catch (IOException exception)
                {
                    _logger.LogWarning(exception, "JSON configuration could not be read: {path}", jsonFilePath);
                    return null;
                }
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
                return await JsonNode.ParseAsync(streamReader.BaseStream);
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
            if (_configuration is IConfigurationRoot configurationRoot)
            {
                foreach (IConfigurationProvider provider in configurationRoot.Providers)
                {
                    if (provider is JsonConfigurationProvider jsonConfigurationProvider &&
                        (requiredKey is null || provider.TryGet(requiredKey, out _)))
                    {
                        return jsonConfigurationProvider;
                    }
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
}
