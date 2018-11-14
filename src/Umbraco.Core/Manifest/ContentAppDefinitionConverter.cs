using System;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;

namespace Umbraco.Core.Manifest
{
    /// <summary>
    /// Implements a json read converter for <see cref="IContentAppDefinition"/>.
    /// </summary>
    internal class ContentAppDefinitionConverter : JsonReadConverter<IContentAppDefinition>
    {
        protected override IContentAppDefinition Create(Type objectType, string path, JObject jObject)
            => new ManifestContentAppDefinition();
    }
}
