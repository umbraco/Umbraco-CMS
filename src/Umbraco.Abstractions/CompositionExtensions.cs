using System;
using System.Collections.Generic;
using System.Text;
using Umbraco.Core.Composing;

namespace Umbraco.Core
{
    public static partial class CompositionExtensions
    {

        #region Collection Builders

        /// <summary>
        /// Gets the components collection builder.
        /// </summary>
        public static ComponentCollectionBuilder Components(this Composition composition)
            => composition.WithCollectionBuilder<ComponentCollectionBuilder>();

        #endregion
    }
}
