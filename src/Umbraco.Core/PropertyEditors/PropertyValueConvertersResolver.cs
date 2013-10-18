using System;
using System.Collections.Generic;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Resolves the IPropertyValueConverter objects.
    /// </summary>
    public sealed class PropertyValueConvertersResolver : ManyObjectsResolverBase<PropertyValueConvertersResolver, IPropertyValueConverter>
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValueConvertersResolver"/> class with 
        /// an initial list of converter types.
        /// </summary>
        /// <param name="converters">The list of converter types</param>
        /// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
        internal PropertyValueConvertersResolver(IEnumerable<Type> converters)
			: base(converters)
		{ }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValueConvertersResolver"/> class with 
        /// an initial list of converter types.
        /// </summary>
        /// <param name="converters">The list of converter types</param>
        /// <remarks>The resolver is created by the <c>WebBootManager</c> and thus the constructor remains internal.</remarks>
        internal PropertyValueConvertersResolver(params Type[] converters)
            : base(converters)
        { }

        /// <summary>
        /// Gets the converters.
        /// </summary>
        public IEnumerable<IPropertyValueConverter> Converters
		{
			get { return Values; }
		}
	}
}