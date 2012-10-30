using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core
{
    /// <summary>
    /// A resolver to return all <see cref="IMacroPropertyType"/> objects
    /// </summary>
    internal sealed class MacroPropertyTypeResolver : ManyObjectsResolverBase<MacroPropertyTypeResolver, IMacroPropertyType>
    {
        /// <summary>
		/// Constructor
		/// </summary>
		/// <param name="macroPropertyTypes"></param>		
        internal MacroPropertyTypeResolver(IEnumerable<Type> macroPropertyTypes)
			: base(macroPropertyTypes)
		{

		}

		/// <summary>
		/// Gets the <see cref="IMacroPropertyType"/> implementations.
		/// </summary>
		public IEnumerable<IMacroPropertyType> MacroPropertyTypes
		{
			get
			{
				return Values;
			}
		}
    }
}