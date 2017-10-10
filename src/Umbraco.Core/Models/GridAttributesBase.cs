using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

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
        public IDictionary<string, string> GetAttributes()
        {
            var attributes = new Dictionary<string, string>();
            if (Config != null)
            {
                attributes = Config.ToObject<JObject>().Properties().ToDictionary(p => p.Name, p => p.Value.ToString());
            }

            if (Styles == null) return attributes;

            var cssValues = Styles.ToObject<JObject>().Properties().Select(p => string.Format("{0}:{1}", p.Name, p.Value.ToString()));
            if (Styles.ToObject<JObject>().Properties().Any())
            {
                attributes.Add("style", string.Join(";", cssValues));
            }
            return attributes;
        }
    }
}