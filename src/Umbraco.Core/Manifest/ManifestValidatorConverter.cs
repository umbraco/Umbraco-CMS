using System;
using Newtonsoft.Json.Linq;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;

namespace Umbraco.Core.Manifest
{
    /// <summary>
    /// Implements a json read converter for <see cref="IPropertyValidator"/>.
    /// </summary>
    internal class ManifestValidatorConverter : JsonReadConverter<IPropertyValidator>
    {
        protected override IPropertyValidator Create(Type objectType, JObject jObject)
        {
            // all validators coming from manifests are ManifestPropertyValidator instances
            return new ManifestPropertyValidator();
        }
    }
}
