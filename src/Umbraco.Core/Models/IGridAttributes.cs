using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Umbraco.Core.Models
{
    public interface IGridAttributes
    {
        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        [JsonProperty("config")]
        JToken Config { get; set; }

        /// <summary>
        /// Gets or sets the styles.
        /// </summary>
        /// <value>
        /// The styles.
        /// </value>
        [JsonProperty("styles")]
        JToken Styles { get; set; }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <returns></returns>
        Dictionary<string, string> GetAttributes();
    }
}
