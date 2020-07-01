using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Serialization;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    public class MappedPropertiesJsonContentNestedDataSerializer : IContentNestedDataSerializer
    {
        private readonly IDictionary<string, PropertyMap> _serializeMap;
        private readonly IDictionary<string, PropertyMap> _deserializeMap;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serializeMap">Map for PropertData properties</param>
        /// <param name="deserializeMap"></param>
        public MappedPropertiesJsonContentNestedDataSerializer(IDictionary<string, PropertyMap> serializeMap, IDictionary<string, PropertyMap> deserializeMap)
        {
            _serializeMap = serializeMap;
            _deserializeMap = deserializeMap;
        }

        public (string mappedName, bool isCompressed) ToSerializedProperty(string name)
        {
            if(_serializeMap.TryGetValue(name,out PropertyMap map))
            {
                return (map.To,map.IsCompressed);
            }
            return (name,false);
        }
        public (string mappedName, bool isCompressed) ToDeserializedProperty(string name)
        {
            if (_deserializeMap.TryGetValue(name, out PropertyMap map))
            {
                return (map.To, map.IsCompressed);
            }
            return (name, false);
        }

        public ContentNestedData Deserialize(string data)
        {
            // by default JsonConvert will deserialize our numeric values as Int64
            // which is bad, because they were Int32 in the database - take care

            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new ForceInt32Converter() },
                
            };

            return JsonConvert.DeserializeObject<ContentNestedData>(data, settings);
        }

        public string Serialize(ContentNestedData nestedData)
        {
            return JsonConvert.SerializeObject(nestedData);
        }
    }

    public class MappedPropertyDataContractResolver : DefaultContractResolver
    {
        private readonly IDictionary<string, PropertyMap> _serializeMap;
        private readonly IDictionary<string, PropertyMap> _deserializeMap;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serializeMap">Map for PropertData properties</param>
        /// <param name="deserializeMap"></param>
        public MappedPropertyDataContractResolver(IDictionary<string, PropertyMap> serializeMap, IDictionary<string, PropertyMap> deserializeMap)
        {
            _serializeMap = serializeMap;
            _deserializeMap = deserializeMap;
        }

        public (string mappedName, bool isCompressed) ToSerializedProperty(string name)
        {
            if (_serializeMap.TryGetValue(name, out PropertyMap map))
            {
                return (map.To, map.IsCompressed);
            }
            return (name, false);
        }
        public (string mappedName, bool isCompressed) ToDeserializedProperty(string name)
        {
            if (_deserializeMap.TryGetValue(name, out PropertyMap map))
            {
                return (map.To, map.IsCompressed);
            }
            return (name, false);
        }

        private readonly Type _type = typeof(PropertyData);
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (property.DeclaringType == _type)
            {
                if (property.PropertyName.Equals("LongPropertyName", StringComparison.OrdinalIgnoreCase))
                {
                    property.PropertyName = "Short";
                }
            }
            return property;
        }
        protected override string ResolvePropertyName(string propertyName)
        {
            return base.ResolvePropertyName(propertyName);
        }
        protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
        {
            JsonDictionaryContract contract = base.CreateDictionaryContract(objectType);

            contract.DictionaryKeyResolver = propertyName => propertyName;

            return contract;
        }
    }

    public class MappedNamingStrategy : NamingStrategy
    {
        public MappedNamingStrategy()
        {
            ProcessDictionaryKeys = true;
        }
        public override string GetDictionaryKey(string key)
        {
            return key;
        }

        protected override string ResolvePropertyName(string name)
        {
            return name;
        }
    }


    public class PropertyMap
    {
        /// <summary>
        /// PropertyName
        /// </summary>
        public string To { get; set; }
        /// <summary>
        /// Whether the property is compressed
        /// </summary>
        public bool IsCompressed { get; set; }
    }
}
