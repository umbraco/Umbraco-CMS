using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// A base MVC controller for use in the back office that ensures that every call to it authorizes the current user.
	/// </summary>
	/// <remarks>
	/// This controller essentially just uses a global UmbracoAuthorizeAttribute, inheritors that require more granular control over the 
	/// authorization of each method can use this attribute instead of inheriting from this controller.
	/// </remarks>
	[UmbracoAuthorize]
	public abstract class UmbracoAuthorizedController : Controller
	{
	}
}
