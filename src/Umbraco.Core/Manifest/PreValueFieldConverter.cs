using System;
using Newtonsoft.Json.Linq;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;

namespace Umbraco.Core.Manifest
{
    /// <summary>
    /// Used to convert a pre-value field manifest to a real pre value field
    /// </summary>
    internal class PreValueFieldConverter : JsonCreationConverter<PreValueField>
    {
        protected override PreValueField Create(Type objectType, JObject jObject)
        {
            return new PreValueField();
        }
    }
}