using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using umbraco.BusinessLogic.Utils;
using umbraco.interfaces;
using umbraco.editorControls;



namespace umbraco.editorControls.macrocontainer
{
    internal class MacroControlFactory
    {
        #region Private Fields
        /// <summary>
        /// All Possible Macro types
        /// </summary>
        private static List<Type> _macroControlTypes = null;
        #endregion

        #region Methods
        /// <summary>
        /// Create an instance of a Macro control and return it.
        /// Because the macro control uses inline client script whichs is not generated after postback
        /// That's why we use the Page Picker instead of the content picker of the macro.
        /// </summary>
        internal static Control GetMacroRenderControlByType(PersistableMacroProperty prop, string uniqueID)
        {
            Control macroControl;
   
            Type m = MacroControlTypes.FindLast(delegate(Type macroGuiCcontrol) { return macroGuiCcontrol.ToString() == string.Format("{0}.{1}", prop.AssemblyName, prop.TypeName); });
            IMacroGuiRendering typeInstance;
            typeInstance = Activator.CreateInstance(m) as IMacroGuiRendering;
            if (!string.IsNullOrEmpty(prop.Value))
            {
                ((IMacroGuiRendering)typeInstance).Value = prop.Value;
            }
            macroControl = (Control)typeInstance;

            macroControl.ID = uniqueID;
            return macroControl;
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
		private static List<Type> MacroControlTypes
        {
            get
            {
                if (_macroControlTypes == null || !_macroControlTypes.Any())
                {
                    //Populate the list with all the types of IMacroGuiRendering
                	var typeFinder = new Umbraco.Core.TypeFinder2();
                    _macroControlTypes = new List<Type>();
                	_macroControlTypes = typeFinder.FindClassesOfType<IMacroGuiRendering>().ToList();
                }

                return _macroControlTypes;
            }
        }
        #endregion

    }
}
