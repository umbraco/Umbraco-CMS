using System;
using Newtonsoft.Json.Linq;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;

namespace Umbraco.Core.Manifest
{
    /// <summary>
    /// Implements a json read converter for <see cref="IValueValidator"/>.
    /// </summary>
    internal class ManifestValidatorConverter : JsonReadConverter<IValueValidator>
    {
        private readonly ManifestValidatorCollection _validators;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManifestValidatorConverter"/> class.
        /// </summary>
        public ManifestValidatorConverter(ManifestValidatorCollection validators)
        {
            _validators = validators;
        }

        protected override IValueValidator Create(Type objectType, JObject jObject)
        {
            // all validators coming from manifests are ManifestPropertyValidator instances
            return new ManifestValueValidator(_validators);
        }
    }
}
