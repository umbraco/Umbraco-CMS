using System;
using System.Collections.Generic;
using Umbraco.Core;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.businesslogic.macro;
using umbraco.cms.businesslogic.media;
using umbraco.interfaces;

namespace umbraco.editorControls
{
	/// <summary>
	/// Extension methods for the PluginTypeResolver
	/// </summary>
	public static class PluginTypeResolverExtensions
	{		

		/// <summary>
		/// Returns all available IMacroGuiRendering in application
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveMacroRenderings(this PluginTypeResolver resolver)
		{
			return resolver.ResolveTypes<IMacroGuiRendering>();
		}

		
	}
}