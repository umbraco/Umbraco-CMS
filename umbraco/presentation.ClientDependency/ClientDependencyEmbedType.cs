using System;
using System.Collections.Generic;
using System.Text;

namespace umbraco.presentation.ClientDependency
{
	public enum ClientDependencyEmbedType
	{
		
		/// <summary>
		/// Renders script tags out in the header.
		/// This is the most common type
		/// </summary>
		Header,
		/// <summary>
		/// Dynamically includes the scripts via client side code.
		/// This is usesful for situations when direct access to the ScriptManager or Head may not be possible
		/// (i.e. Canvas)
		/// </summary>
		ClientSideRegistration
	}
}
