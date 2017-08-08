using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Models
{
    public abstract class GridAttributesBase : IGridAttributes
    {
        /// <summary>
        /// Gets or sets the con dfig.
        /// </summary>
        /// <value>
        /// The con dfig.
        /// </value>
        [JsonProperty("config")]
        public JToken Config { get; set; }

        /// <summary>
        /// Gets or sets the styles.
        /// </summary>
        /// <value>
        /// The styles.
        /// </value>
        [JsonProperty("styles")]
        public JToken Styles { get; set; }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetAttributes()
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
