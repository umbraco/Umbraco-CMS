using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Umbraco.Web.UI.Umbraco.Dialogs
{
	public partial class EditMacro : global::umbraco.dialogs.editMacro
	{

		/// <summary>
		/// Returns the number of macro properties defined on the macro found
		/// </summary>
		protected int CountOfMacroProperties { get; private set; }

		/// <summary>
		/// Sets the macro propery count if the macro object has been loaded
		/// </summary>
		/// <param name="e"></param>
		/// <remarks>
		/// The macro property will only have been loaded on a post back from the first screen after selecting a macro, otherwise it will be zero.
		/// </remarks>
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			CountOfMacroProperties = MacroObject != null 
				? MacroObject.Properties.Count() 
				: 0;
		}
	}
}