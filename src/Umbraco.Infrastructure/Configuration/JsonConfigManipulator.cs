using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.Configuration
{
    public class JsonConfigManipulator : IConfigManipulator
    {
        private const string UmbracoConnectionStringPath = $"ConnectionStrings:{Cms.Core.Constants.System.UmbracoConnectionName}";
        private const string UmbracoConnectionStringProviderNamePath = UmbracoConnectionStringPath + ConnectionStrings.ProviderNamePostfix;

        private readonly IConfiguration _configuration;
        private readonly ILogger<JsonConfigManipulator> _logger;
        private readonly object _locker = new object();

        [Obsolete]
        public JsonConfigManipulator(IConfiguration configuration)
            : this(configuration, StaticServiceProvider.Instance.GetRequiredService<ILogger<JsonConfigManipulator>>())
        { }

        public JsonConfigManipulator(IConfiguration configuration, ILogger<JsonConfigManipulator> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [Obsolete]
        public string UmbracoConnectionPath { get; } = UmbracoConnectionStringPath;

        public void RemoveConnectionString()
        {
            // Remove keys from JSON
            var provider = GetJsonConfigurationProvider(UmbracoConnectionStringPath);

            var json = GetJson(provider);
            if (json is null)
            {
                _logger.LogWarning("Failed to remove connection string from JSON configuration.");
                return;
            }

            RemoveJsonKey(json, UmbracoConnectionStringPath);
            RemoveJsonKey(json, UmbracoConnectionStringProviderNamePath);

            SaveJson(provider, json);
        }

        public void SaveConnectionString(string connectionString, string? providerName)
        {
            // Save keys to JSON
            var provider = GetJsonConfigurationProvider();

            var json = GetJson(provider);
            if (json is null)
            {
                _logger.LogWarning("Failed to save connection string in JSON configuration.");
                return;
            }

            var item = GetConnectionItem(connectionString, providerName);
            if (item is not null)
            {
                json.Merge(item, new JsonMergeSettings());
            }

            SaveJson(provider, json);
        }

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

        private JToken? GetConnectionItem(string connectionString, string? providerName)
        {
            JTokenWriter writer = new JTokenWriter();

            writer.WriteStartObject();
            writer.WritePropertyName("ConnectionStrings");
            writer.WriteStartObject();
            writer.WritePropertyName(Constants.System.UmbracoConnectionName);
            writer.WriteValue(connectionString);

            if (!string.IsNullOrEmpty(providerName))
            {
                writer.WritePropertyName(Constants.System.UmbracoConnectionName + ConnectionStrings.ProviderNamePostfix);
                writer.WriteValue(providerName);
            }

            writer.WriteEndObject();
            writer.WriteEndObject();

            return writer.Token;
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
                    var jsonFilePath = Path.Combine(physicalFileProvider.Root, provider.Source.Path);

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

                var jsonFilePath = Path.Combine(physicalFileProvider.Root, provider.Source.Path);

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

        private JsonConfigurationProvider? GetJsonConfigurationProvider(string? requiredKey = null)
        {
            if (_configuration is IConfigurationRoot configurationRoot)
            {
                foreach (var provider in configurationRoot.Providers)
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
