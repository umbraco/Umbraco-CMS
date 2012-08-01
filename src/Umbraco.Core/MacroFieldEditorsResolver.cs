using System;
using System.Collections.Generic;
using System.Web.UI;
using Umbraco.Core.Macros;
using Umbraco.Core.Resolving;
using umbraco.interfaces;

namespace Umbraco.Core
{
	/// <summary>
	/// A resolver to return all IMacroGuiRendering objects
	/// </summary>
	/// <remarks>
	/// Much of this classes methods are based on legacy code from umbraco.editorControls.macrocontainer.MacroControlFactory
	/// this code should probably be reviewed and cleaned up if necessary.
	/// </remarks>
	internal sealed class MacroFieldEditorsResolver : ManyObjectsResolverBase<MacroFieldEditorsResolver, IMacroGuiRendering>
	{

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="macroEditors"></param>		
		internal MacroFieldEditorsResolver(IEnumerable<Type> macroEditors)
			: base(macroEditors, ObjectLifetimeScope.Transient)
		{

		}

		/// <summary>
		/// Gets the <see cref="IMacroGuiRendering"/> implementations.
		/// </summary>
		public IEnumerable<IMacroGuiRendering> MacroFieldEditors
		{
			get
			{
				return Values;
			}
		}

		/// <summary>
		/// Gets the value based on the type of control
		/// </summary>
		/// <param name="macroControl"></param>
		/// <returns></returns>
		/// <remarks>
		/// This is legacy code migrated from umbraco.editorControls.macrocontainer.MacroControlFactory
		/// </remarks>
		internal string GetValueFromMacroControl(Control macroControl)
		{
			return ((IMacroGuiRendering)macroControl).Value;
		}

		/// <remarks>
		/// This is legacy code migrated from umbraco.editorControls.macrocontainer.MacroControlFactory
		/// </remarks>
		internal List<Type> MacroControlTypes
		{
			get { return InstanceTypes; }
		}

		/// <summary>
		/// Create an instance of a Macro control and return it.
		/// Because the macro control uses inline client script whichs is not generated after postback
		/// That's why we use the Page Picker instead of the content picker of the macro.
		/// </summary>
		/// <remarks>
		/// This is legacy code migrated from umbraco.editorControls.macrocontainer.MacroControlFactory
		/// </remarks>
		internal Control GetMacroRenderControlByType(PersistableMacroProperty prop, string uniqueId)
		{
			var m = MacroControlTypes.FindLast(macroGuiCcontrol => macroGuiCcontrol.ToString() == string.Format("{0}.{1}", prop.AssemblyName, prop.TypeName));
			var instance = PluginManager.Current.CreateInstance<IMacroGuiRendering>(m);
			if (instance != null)
			{
				if (!string.IsNullOrEmpty(prop.Value))
				{
					instance.Value = prop.Value;
				}
				var macroControl = instance as Control;
				if (macroControl != null)
				{
					macroControl.ID = uniqueId;
					return macroControl;
				}
			}
			return null;
		}

	}
}