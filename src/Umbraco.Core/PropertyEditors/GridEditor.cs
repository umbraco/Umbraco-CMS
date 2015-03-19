using System.Collections.Generic;
using Newtonsoft.Json;

namespace Umbraco.Core.PropertyEditors
{
    internal class GridEditor
    {
        public GridEditor()
        {
            Config = new Dictionary<string, object>();
        }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("alias", Required = Required.Always)]
        public string Alias { get; set; }

        [JsonProperty("view", Required = Required.Always)]
        public string View { get; set; }

        [JsonProperty("render")]
        public string Render { get; set; }

        [JsonProperty("icon", Required = Required.Always)]
        public string Icon { get; set; }

        [JsonProperty("config")]
        public IDictionary<string, object> Config { get; set; }

        protected bool Equals(GridEditor other)
        {
            return string.Equals(Alias, other.Alias);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GridEditor) obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return Alias.GetHashCode();
        }
    }
}