namespace Umbraco.Core.Models.Entities
{
    /// <summary>
    /// Implements <see cref="IDocumentEntitySlim"/>.
    /// </summary>
    public class DocumentEntitySlim : ContentEntitySlim, IDocumentEntitySlim
    {
        /// <inheritdoc />
        public bool Published { get; set; }

        /// <inheritdoc />
        public bool Edited { get; set; }
    }
}