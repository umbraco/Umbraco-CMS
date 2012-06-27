using System;
using System.Data;
using Microsoft.ApplicationBlocks.Data;
using System.Data.SqlClient;

using System.Web.UI;
using ClientDependency.Core;
using umbraco.IO;
using umbraco.interfaces;
using umbraco.uicontrols.TreePicker;

namespace umbraco.macroRenderings
{

    public class media : SimpleMediaPicker, IMacroGuiRendering
	{
        #region IMacroGuiRendering Members

        string IMacroGuiRendering.Value
        {
            get
            {
                return Value;
            }
            set
            {
                Value = value;
            }
        }

        bool IMacroGuiRendering.ShowCaption
        {
            get { return true; }
        }

        #endregion
    }
}
