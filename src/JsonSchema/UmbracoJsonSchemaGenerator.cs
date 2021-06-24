using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema.Generation;

namespace JsonSchema
{
    public class UmbracoJsonSchemaGenerator
    {
        private readonly JsonSchemaGenerator _innerGenerator;
        private static readonly HttpClient s_client = new HttpClient();

        public UmbracoJsonSchemaGenerator()
        {
            _innerGenerator = new JsonSchemaGenerator(new UmbracoJsonSchemaGeneratorSettings());
        }

        public async Task<string> Generate()
        {
            var umbracoSchema = GenerateUmbracoSchema();
            var officialSchema = await GetOfficialAppSettingsSchema().ConfigureAwait(false);;

            officialSchema.Merge(umbracoSchema);

            return officialSchema.ToString();
        }

        private async Task<JObject> GetOfficialAppSettingsSchema()
        {

            var response = s_client.GetAsync(new Uri("https://json.schemastore.org/appsettings.json")).GetAwaiter().GetResult();


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
