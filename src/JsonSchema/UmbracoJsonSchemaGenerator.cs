using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema.Generation;

namespace JsonSchema
{
    /// <summary>
    /// Generator of the JsonSchema for AppSettings.json including A specific Umbraco version.
    /// </summary>
    public class UmbracoJsonSchemaGenerator
    {
        private readonly JsonSchemaGenerator _innerGenerator;
        private static readonly HttpClient s_client = new HttpClient();

        /// <summary>
        /// Creates a new instance of <see cref="UmbracoJsonSchemaGenerator"/>.
        /// </summary>
        /// <param name="definitionPrefix">The prefix to use for definitions generated.</param>
        public UmbracoJsonSchemaGenerator(string definitionPrefix)
            => _innerGenerator = new JsonSchemaGenerator(new UmbracoJsonSchemaGeneratorSettings(definitionPrefix));


        /// <summary>
        /// Generates a json representing the JsonSchema for AppSettings.json including A specific Umbraco version..
        /// </summary>
        public async Task<string> Generate()
        {
            var umbracoSchema = GenerateUmbracoSchema();
            var officialSchema = await GetOfficialAppSettingsSchema();

            officialSchema.Merge(umbracoSchema);

            return officialSchema.ToString();
        }

        private async Task<JObject> GetOfficialAppSettingsSchema()
        {

            var response = await s_client.GetAsync("https://json.schemastore.org/appsettings.json");


            var result =  await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<JObject>(result);

        }

        private JObject GenerateUmbracoSchema()
        {
            var schema = _innerGenerator.Generate(typeof(AppSettings));

            return JsonConvert.DeserializeObject<JObject>(schema.ToJson());
        }
    }
}
