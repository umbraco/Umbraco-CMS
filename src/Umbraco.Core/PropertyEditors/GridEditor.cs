using System.Collections.Generic;
using Newtonsoft.Json;
using Umbraco.Core.Configuration.Grid;
using Umbraco.Core.IO;

namespace Umbraco.Core.PropertyEditors
{
    public class GridEditor : IGridEditorConfig
    {
        private string _view;
        private string _render;

        public GridEditor()
        {
            Config = new Dictionary<string, object>();
        }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("alias", Required = Required.Always)]
        public string Alias { get; set; }

        [JsonProperty("view", Required = Required.Always)]
        public string View
        {
            get => _view;
            set => _view = IOHelper.ResolveVirtualUrl(value);
        }

        [JsonProperty("render")]
        public string Render
        {
            get => _render;
            set => _render = IOHelper.ResolveVirtualUrl(value);
        }

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
