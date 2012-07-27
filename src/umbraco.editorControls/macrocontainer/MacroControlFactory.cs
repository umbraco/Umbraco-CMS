using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.UI;
using Umbraco.Core;
using umbraco.BusinessLogic.Utils;
using umbraco.interfaces;
using umbraco.editorControls;



namespace umbraco.editorControls.macrocontainer
{
    internal class MacroControlFactory
    {
        
        /// <summary>
        /// All Possible Macro types
        /// </summary>
        private static List<Type> _macroControlTypes = null;

    	private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();


        #region Methods

        /// <summary>
        /// Create an instance of a Macro control and return it.
        /// Because the macro control uses inline client script whichs is not generated after postback
        /// That's why we use the Page Picker instead of the content picker of the macro.
        /// </summary>
        internal static Control GetMacroRenderControlByType(PersistableMacroProperty prop, string uniqueID)
        {
        	var m = MacroControlTypes.FindLast(macroGuiCcontrol => macroGuiCcontrol.ToString() == string.Format("{0}.{1}", prop.AssemblyName, prop.TypeName));
        	var instance = PluginTypeResolver.Current.CreateInstance<IMacroGuiRendering>(m);
			if (instance != null)
			{
				if (!string.IsNullOrEmpty(prop.Value))
				{
					instance.Value = prop.Value;
				}
				var macroControl = instance as Control;
				if (macroControl != null)
				{
					macroControl.ID = uniqueID;
					return macroControl;	
				}
			}
        	return null;
        }

        /// <summary>
        /// Gets the value based on the type of control
        /// </summary>
        /// <param name="macroControl"></param>
        /// <returns></returns>
        internal static string GetValueFromMacroControl(Control macroControl)
        {
            return ((IMacroGuiRendering)macroControl).Value;
        }

        #endregion

        #region Properties
        /// <summary>
        /// All Possible Macro types
        /// </summary>
		internal static List<Type> MacroControlTypes
        {
            get
            {
				using (var readLock = new UpgradeableReadLock(Lock))
				{
					if (_macroControlTypes == null || !_macroControlTypes.Any())
					{

						readLock.UpgradeToWriteLock();

						_macroControlTypes = new List<Type>(PluginTypeResolver.Current.ResolveMacroRenderings());
					}

					return _macroControlTypes;
				}
            }
        }
        #endregion

    }
}
