// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelatedLinkData.cs" company="Umbraco">
//   Umbraco
// </copyright>
// <summary>
//   Defines the RelatedLink json data type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace Umbraco.Web.Models
{
    public class RelatedLinkData : RelatedLinkBase
    {
        [JsonProperty("internal")]
        public int? Internal { get; set; }
        [JsonProperty("edit")]
        public bool Edit { get; set; }
        [JsonProperty("internalName")]
        public string InternalName { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
