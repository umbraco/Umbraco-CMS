using System.Collections.Generic;

namespace Umbraco.Core.Models.Entities
{
    /// <summary>
    /// Implements <see cref="IDocumentEntitySlim"/>.
    /// </summary>
    public class DocumentEntitySlim : ContentEntitySlim, IDocumentEntitySlim
    {
        private static readonly IReadOnlyDictionary<string, string> Empty = new Dictionary<string, string>();
        private IReadOnlyDictionary<string, string> _cultureNames;
        public IReadOnlyDictionary<string, string> CultureNames
        {
            get => _cultureNames ?? Empty;
            set => _cultureNames = value;
        }

        public ContentVariation Variations { get; set; }

        /// <inheritdoc />
        public bool Published { get; set; }

        /// <inheritdoc />
        public bool Edited { get; set; }
    }
}
