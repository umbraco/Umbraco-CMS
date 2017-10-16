using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Umbraco.Core.Models
{
    public abstract class GridAttributesBase : IGridAttributes
    {
        /// <inheritdoc />
        [JsonProperty("config")]
        public JToken Config { get; set; }

        /// <inheritdoc />
        [JsonProperty("styles")]
        public JToken Styles { get; set; }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, string>> GetAttributes()
        {
            var attributes = new List<KeyValuePair<string, string>>();

            if (Config != null)
            {
                attributes = Config.ToObject<JObject>().Properties()
                    .Select(p => new KeyValuePair<string, string>(p.Name, HttpUtility.HtmlAttributeEncode(p.Value.ToString()))).ToList();
            }

            if (Styles != null)
            {
                var jObject = Styles.ToObject<JObject>();
                if (jObject.Properties().Any() == false) return attributes;

                var cssValues = jObject.Properties()
                    .Where(p => string.IsNullOrWhiteSpace(p.Value.ToString()) == false)
                    .Select(p => string.Format("{0}:{1}", p.Name, p.Value.ToString()));

                attributes.Add(new KeyValuePair<string, string>("style", HttpUtility.HtmlAttributeEncode(string.Join(";", cssValues))));
            }

            return attributes;
        }
    }
}