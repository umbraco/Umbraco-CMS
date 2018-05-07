using System.Collections.Generic;

namespace Umbraco.Core.Models.Entities
{
    /// <summary>
    /// Represents a lightweight document entity, managed by the entity service.
    /// </summary>
    public interface IDocumentEntitySlim : IContentEntitySlim
    {
        //fixme we need to supply more information than this and change this property name. This will need to include Published/Editor per variation since we need this information for the tree
        IReadOnlyDictionary<string, string> CultureNames { get; }

        /// <summary>
        /// At least one variation is published
        /// </summary>
        /// <remarks>
        /// If the document is invariant, this simply means there is a published version
        /// </remarks>
        bool Published { get; set; }

        /// <summary>
        /// At least one variation has pending changes
        /// </summary>
        /// <remarks>
        /// If the document is invariant, this simply means there is pending changes
        /// </remarks>
        bool Edited { get; set; }

    }
}
