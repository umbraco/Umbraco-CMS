using System;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Serialization;

namespace Umbraco.Core.Manifest
{
    /// <summary>
    /// Implements a json read converter for <see cref="ManifestContentAppDefinition"/>.
    /// </summary>
    internal class ContentAppDefinitionConverter : JsonReadConverter<ManifestContentAppDefinition>
    {
        protected override ManifestContentAppDefinition Create(Type objectType, string path, JObject jObject)
            => new ManifestContentAppDefinition();
    }
}
