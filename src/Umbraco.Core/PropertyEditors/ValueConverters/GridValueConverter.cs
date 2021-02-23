using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.Grid;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    /// <summary>
    /// This ensures that the grid config is merged in with the front-end value
    /// </summary>
    [DefaultPropertyValueConverter(typeof(JsonValueConverter))] //this shadows the JsonValueConverter
    public class GridValueConverter : JsonValueConverter
    {
        private readonly IGridConfig _config;

        public GridValueConverter(PropertyEditorCollection propertyEditors, IGridConfig config)
            : base(propertyEditors)
        {
            _config = config;
        }

        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.Grid);

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof (JToken);

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;
            var sourceString = source.ToString();

            if (sourceString.DetectIsJson())
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<JObject>(sourceString);

                    //so we have the grid json... we need to merge in the grid's configuration values with the values
                    // we've saved in the database so that when the front end gets this value, it is up-to-date.

                    var sections = GetArray(obj, "sections");
                    foreach (var section in sections.Cast<JObject>())
                    {
                        var rows = GetArray(section, "rows");
                        foreach (var row in rows.Cast<JObject>())
                        {
                            var areas = GetArray(row, "areas");
                            foreach (var area in areas.Cast<JObject>())
                            {
                                var controls = GetArray(area, "controls");
                                foreach (var control in controls.Cast<JObject>())
                                {
                                    var editor = control.Value<JObject>("editor");
                                    if (editor != null)
                                    {
                                        var alias = editor.Value<string>("alias");
                                        if (alias.IsNullOrWhiteSpace() == false)
                                        {
                                            //find the alias in config
                                            var found = _config.EditorsConfig.Editors.FirstOrDefault(x => x.Alias == alias);
                                            if (found != null)
                                            {
                                                //add/replace the editor value with the one from config

                                                var serialized = new JObject();
                                                serialized["name"] = found.Name;
                                                serialized["alias"] = found.Alias;
                                                serialized["view"] = found.View;
                                                serialized["render"] = found.Render;
                                                serialized["icon"] = found.Icon;
                                                serialized["config"] = JObject.FromObject(found.Config);

                                                control["editor"] = serialized;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return obj;
                }
                catch (Exception ex)
                {
                    Current.Logger.Error<GridValueConverter, string>(ex, "Could not parse the string '{JsonString}' to a json object", sourceString);
                }
            }

            //it's not json, just return the string
            return sourceString;
        }

        private JArray GetArray(JObject obj, string propertyName)
        {
            JToken token;
            if (obj.TryGetValue(propertyName, out token))
            {
                var asArray = token as JArray;
                return asArray ?? new JArray();
            }
            return new JArray();
        }

    }
}
