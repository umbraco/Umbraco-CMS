using System;
using System.Collections.Generic;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.PropertyEditors
{
	/// <summary>
	/// Manages the list of IPropertyEditorValueConverter's
	/// </summary>
	internal class PropertyEditorValueConvertersResolver : ManyObjectsResolverBase<PropertyEditorValueConvertersResolver, IPropertyEditorValueConverter>
	{
		public PropertyEditorValueConvertersResolver(IEnumerable<Type> converters)
			: base(converters)
		{
		}
	
		public IEnumerable<IPropertyEditorValueConverter> Converters
		{
			get { return Values; }
		}
	}
}