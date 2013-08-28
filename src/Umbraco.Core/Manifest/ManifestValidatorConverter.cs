using System;
using Newtonsoft.Json.Linq;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;

namespace Umbraco.Core.Manifest
{
    /// <summary>
    /// Used when deserialing the validation collection, any serialized property editors are from a manifest and thus the
    /// validators are manifest validators.
    /// </summary>
    internal class ManifestValidatorConverter : JsonCreationConverter<IPropertyValidator>
    {
        protected override IPropertyValidator Create(Type objectType, JObject jObject)
        {
            return new ManifestPropertyValidator();
        }
    }
}