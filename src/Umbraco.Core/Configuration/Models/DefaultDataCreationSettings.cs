// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Cms.Core.Configuration.Models
{
    public enum DefaultDataCreationOption
    {
        None,
        CreateOnly,
        CreateAllExcept,
        All
    }

    /// <summary>
    /// Typed configuration options for NuCache settings.
    /// </summary>
    [UmbracoOptions(Constants.Configuration.ConfigDefaultDataCreation)]
    public class DefaultDataCreationSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to create default language data on installation.
        /// </summary>
        public DefaultDataCreationOption CreateDefaultLanguages { get; set; } = DefaultDataCreationOption.All;

        /// <summary>
        /// Gets or sets a value indicating which default languages should be created when <see cref="CreateDefaultLanguages"/> is
        /// set to <see cref="DefaultDataCreationOption.CreateOnly"/> or <see cref="DefaultDataCreationOption.CreateAllExcept"/>.
        /// </summary>
        /// <remarks>
        /// The values provided should be the ISO codes for the languages to be included or excluded, e.g. "en-US".
        /// </remarks>
        public IEnumerable<string> SelectedDefaultLanguages { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Gets or sets a value indicating whether to create default data type data on installation.
        /// </summary>
        public DefaultDataCreationOption CreateDefaultDataTypes { get; set; } = DefaultDataCreationOption.All;

        /// <summary>
        /// Gets or sets a value indicating which default languages should be created when <see cref="CreateDefaultLanguages"/> is
        /// set to <see cref="DefaultDataCreationOption.CreateOnly"/> or <see cref="DefaultDataCreationOption.CreateAllExcept"/>.
        /// </summary>
        /// <remarks>
        /// The values provided should be the Guid values used by Umbraco for the data type, listed at:
        /// https://github.com/umbraco/Umbraco-CMS/blob/v9/dev/src/Umbraco.Core/Constants-DataTypes.cs.
        /// Some data types - such as the string label - cannot be excluded from install as they are required for core Umbraco
        /// functionality.
        /// Otherwise take care not to remove data types required for default Umbraco media and member types, unless you also
        /// choose to exclude them.
        /// </remarks>
        public IEnumerable<Guid> SelectedDefaultDataTypes { get; set; } = Enumerable.Empty<Guid>();

        /// <summary>
        /// Gets or sets a value indicating whether to create default media type data on installation.
        /// </summary>
        public DefaultDataCreationOption CreateDefaultMediaTypes { get; set; } = DefaultDataCreationOption.All;

        /// <summary>
        /// Gets or sets a value indicating which default media types should be created when <see cref="CreateDefaultMediaTypes"/> is
        /// set to <see cref="DefaultDataCreationOption.CreateOnly"/> or <see cref="DefaultDataCreationOption.CreateAllExcept"/>.
        /// </summary>
        /// <remarks>
        /// The values provided should be the Guid values used by Umbraco for the media type, listed at:
        /// https://github.com/umbraco/Umbraco-CMS/blob/v9/dev/src/Umbraco.Infrastructure/Migrations/Install/DatabaseDataCreator.cs.
        /// </remarks>
        public IEnumerable<Guid> SelectedDefaultMediaTypes { get; set; } = Enumerable.Empty<Guid>();

        /// <summary>
        /// Gets or sets a value indicating whether to create default media type data on installation.
        /// </summary>
        public DefaultDataCreationOption CreateDefaultMemberTypes { get; set; } = DefaultDataCreationOption.All;

        /// <summary>
        /// Gets or sets a value indicating which default media types should be created when <see cref="CreateDefaultMediaTypes"/> is
        /// set to <see cref="DefaultDataCreationOption.CreateOnly"/> or <see cref="DefaultDataCreationOption.CreateAllExcept"/>.
        /// </summary>
        /// <remarks>
        /// The values provided should be the Guid values used by Umbraco for the media type, listed at:
        /// https://github.com/umbraco/Umbraco-CMS/blob/v9/dev/src/Umbraco.Infrastructure/Migrations/Install/DatabaseDataCreator.cs.
        /// </remarks>
        public IEnumerable<Guid> SelectedDefaultMemberTypes { get; set; } = Enumerable.Empty<Guid>();

        /// <summary>
        /// Gets or sets a value indicating whether to create default relation type data on installation.
        /// </summary>
        public DefaultDataCreationOption CreateDefaultRelationTypes { get; set; } = DefaultDataCreationOption.All;

        /// <summary>
        /// Gets or sets a value indicating which default relation types should be created when <see cref="CreateDefaultLanguages"/> is
        /// set to <see cref="DefaultDataCreationOption.CreateOnly"/> or <see cref="DefaultDataCreationOption.CreateAllExcept"/>.
        /// </summary>
        /// <remarks>
        /// The values provided should be the aliases for the relation types, listed at:
        /// https://github.com/umbraco/Umbraco-CMS/blob/v9/dev/src/Umbraco.Core/Constants-Conventions.cs
        /// </remarks>
        public IEnumerable<string> SelectedDefaultRelationTypes { get; set; } = Enumerable.Empty<string>();
    }
}
