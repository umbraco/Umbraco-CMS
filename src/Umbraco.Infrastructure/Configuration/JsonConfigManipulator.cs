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
        private const string UmbracoConnectionStringPath = $"ConnectionStrings:{Constants.System.UmbracoConnectionName}";
        private const string UmbracoConnectionStringProviderNamePath = UmbracoConnectionStringPath + ConnectionStrings.ProviderNamePostfix;
        private const string ConnectionStringObjectName = "ConnectionStrings";

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
        {
            // Remove keys from JSON
            JsonConfigurationProvider? provider = GetJsonConfigurationProvider(UmbracoConnectionStringPath);

            JObject? json = GetJson(provider);
            if (json is null)
            {
                _logger.LogWarning("Failed to remove connection string from JSON configuration.");
                return;
            }

            RemoveJsonKey(json, UmbracoConnectionStringPath);
            RemoveJsonKey(json, UmbracoConnectionStringProviderNamePath);

            SaveJson(provider, json);
        }

        public async Task RemoveConnectionStringAsync()
        {
            JsonConfigurationProvider? provider = GetJsonConfigurationProvider(UmbracoConnectionStringPath);

            JsonNode? jsonNode = await GetJsonNodeAsync(provider);

            if (jsonNode is null)
            {
                _logger.LogWarning("Failed to remove connection string from JSON configuration");
                return;
            }
            // TODO: Finish me
        }

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

        public void SaveConnectionString(string connectionString, string? providerName)
            => SaveConnectionStringAsync(connectionString, providerName).GetAwaiter().GetResult();

        public void SaveConfigValue(string key, object value)
        {
            // Save key to JSON
            var provider = GetJsonConfigurationProvider();

            var json = GetJson(provider);
            if (json is null)
            {
                _logger.LogWarning("Failed to save configuration key \"{Key}\" in JSON configuration.", key);
                return;
            }

            JToken? token = json;
            foreach (var propertyName in key.Split(new[] { ':' }))
            {
                if (token is null)
                    break;
                token = CaseSelectPropertyValues(token, propertyName);
            }

            if (token is null)
                return;

            var writer = new JTokenWriter();
            writer.WriteValue(value);

            if (writer.Token is not null)
            {
                token.Replace(writer.Token);
            }

            SaveJson(provider, json);
        }

        public void SaveDisableRedirectUrlTracking(bool disable)
        {
            // Save key to JSON
            var provider = GetJsonConfigurationProvider();

            var json = GetJson(provider);
            if (json is null)
            {
                _logger.LogWarning("Failed to save enabled/disabled state for redirect URL tracking in JSON configuration.");
                return;
            }

            var item = GetDisableRedirectUrlItem(disable);
            if (item is not null)
            {
                json.Merge(item, new JsonMergeSettings());
            }

            SaveJson(provider, json);
        }

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

        private static void RemoveJsonKey(JObject? json, string key)
        {
            JToken? token = json;
            foreach (var propertyName in key.Split(new[] { ':' }))
            {
                token = CaseSelectPropertyValues(token, propertyName);
            }

            token?.Parent?.Remove();
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
        /// Returns the property value when case insensative
        /// </summary>
        /// <remarks>
        /// This method is required because keys are case insensative in IConfiguration.
        /// JObject[..] do not support case insensative and JObject.Property(...) do not return a new JObject.
        /// </remarks>
        private static JToken? CaseSelectPropertyValues(JToken? token, string name)
        {
            if (token is JObject obj)
            {
                foreach (var property in obj.Properties())
                {
                    if (name is null)
                    {
                        return property.Value;
                    }

                    if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        return property.Value;
                    }
                }
            }

            return null;
        }
    }
}
