using System;
using System.Collections.Generic;
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
        /// <param name="converters">The list of converter types</param>
        /// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
        internal PropertyEditorValueConvertersResolver(IEnumerable<Type> converters)
			: base(converters)
		{ }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyEditorValueConvertersResolver"/> class with 
        /// an initial list of converter types.
        /// </summary>
        /// <param name="converters">The list of converter types</param>
        /// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
        internal PropertyEditorValueConvertersResolver(params Type[] converters)
            : base(converters)
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