using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;

namespace Umbraco.Core.Manifest
{
    /// <summary>
    /// Ensures that virtual paths are taken care of
    /// </summary>
    internal class GridEditorConverter : JsonCreationConverter<GridEditor>
    {
        protected override GridEditor Create(Type objectType, JObject jObject)
        {
            return new GridEditor();
        }

        protected override void Deserialize(JObject jObject, GridEditor target, JsonSerializer serializer)
        {
            base.Deserialize(jObject, target, serializer);

            if (target.View.IsNullOrWhiteSpace() == false && target.View.StartsWith("~/"))
            {
                target.View = IOHelper.ResolveUrl(target.View);
            }

            if (target.Render.IsNullOrWhiteSpace() == false && target.Render.StartsWith("~/"))
            {
                target.Render = IOHelper.ResolveUrl(target.Render);
            }             
        }
    }
}