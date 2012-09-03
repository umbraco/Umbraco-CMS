using System.Web.UI;

namespace Umbraco.Web
{
	/// <summary>
	/// A formless page for use with the rendering a control in a page via Server.Execute. 
	/// This ignores the check to check for a form control on the page.
	/// </summary>
	/// <remarks>
	/// UmbracoHelper currently uses this for rendering macros but could be used anywhere we want when rendering
	/// a page with Server.Execute. 
	/// SD: I have a custom MVC engine that uses this in my own internal libs if we want to pull it out which is called ViewManager
	/// and works really well for things like this.
	/// </remarks>
	internal class FormlessPage : Page
	{
		public override void VerifyRenderingInServerForm(Control control) { }

	}
}