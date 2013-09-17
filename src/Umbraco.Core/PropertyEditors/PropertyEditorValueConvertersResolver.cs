using System;
using System.Collections.Generic;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.PropertyEditors
{
	/// <summary>
	/// Manages the list of legacy IPropertyEditorValueConverter's
	/// </summary>
	internal sealed class PropertyEditorValueConvertersResolver : ManyObjectsResolverBase<PropertyEditorValueConvertersResolver, IPropertyEditorValueConverter>
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