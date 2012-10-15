using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Umbraco.Web.UI.Install.Steps.Skinning
{
	public partial class LoadStarterKits : global::umbraco.presentation.install.steps.Skinning.loadStarterKits
	{
		/// <summary>
		/// Returns the string for the package installer web service base url
		/// </summary>
		protected string PackageInstallServiceBaseUrl { get; private set; }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			//Get the URL for the package install service base url
			var umbracoPath = global::Umbraco.Core.Configuration.GlobalSettings.UmbracoMvcArea;
			var urlHelper = new UrlHelper(new RequestContext());
			PackageInstallServiceBaseUrl = urlHelper.Action("Index", "InstallPackage", new { area = umbracoPath });
		}

	}
}