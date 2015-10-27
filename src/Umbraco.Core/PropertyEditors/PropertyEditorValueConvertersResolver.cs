using System;
using System.Collections.Generic;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
	/// Manages the list of IPropertyEditorValueConverter's
    /// </summary>
    internal sealed class PropertyEditorValueConvertersResolver : ManyObjectsResolverBase<PropertyEditorValueConvertersResolver, IPropertyEditorValueConverter>
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyEditorValueConvertersResolver"/> class with 
        /// an initial list of converter types.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        /// <param name="converters">The list of converter types</param>
        /// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
        internal PropertyEditorValueConvertersResolver(IServiceProvider serviceProvider, ILogger logger, IEnumerable<Type> converters)
            : base(serviceProvider, logger, converters)
		{ }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyEditorValueConvertersResolver"/> class with 
        /// an initial list of converter types.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        /// <param name="converters">The list of converter types</param>
        /// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
        internal PropertyEditorValueConvertersResolver(IServiceProvider serviceProvider, ILogger logger, params Type[] converters)
            : base(serviceProvider, logger, converters)
        { }

        /// <summary>
        /// Gets the converteres.
        /// </summary>
        public IEnumerable<IPropertyEditorValueConverter> Converters
		{
			get { return Values; }
		}
	}
}