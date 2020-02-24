﻿namespace Umbraco.Core.Models.Entities
{
    /// <summary>
    /// Implements <see cref="IContentEntitySlim"/>.
    /// </summary>
    public class ContentEntitySlim : EntitySlim, IContentEntitySlim
    {
        /// <inheritdoc />
        public string ContentTypeAlias { get; set; }

        /// <inheritdoc />
        public string ContentTypeIcon { get; set; }

        /// <inheritdoc />
        public string ContentTypeThumbnail { get; set; }
    }
}