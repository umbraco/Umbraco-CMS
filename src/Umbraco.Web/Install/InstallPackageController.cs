using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Umbraco.Core;
using umbraco;


namespace Umbraco.Web.Install
{
    /// <summary>
	/// An MVC controller for the installation process regarding packages
	/// </summary>
	/// <remarks>
	/// Currently this is used for web services however we should/could eventually migrate the whole installer to MVC as it
	/// is a bit of a mess currently.
	/// </remarks>
	[UmbracoInstallAuthorize]
	public class InstallPackageController : Controller
	{
		private readonly ApplicationContext _applicationContext;

		public InstallPackageController()
			: this(ApplicationContext.Current)
		{
			
		}

		public InstallPackageController(ApplicationContext applicationContext)
		{
			_applicationContext = applicationContext;
		}

		private const string RepoGuid = "65194810-1f85-11dd-bd0b-0800200c9a66";

		/// <summary>
		/// Empty action, useful for retrieving the base url for this controller
		/// </summary>
		/// <returns></returns>
		public ActionResult Index()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Connects to the repo, downloads the package and creates the manifest
		/// </summary>
		/// <param name="kitGuid"></param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult DownloadPackageFiles(Guid kitGuid)
		{
			var repo = global::umbraco.cms.businesslogic.packager.repositories.Repository.getByGuid(RepoGuid);
			if (!repo.HasConnection())
			{
				return Json(new {success = false, error = "cannot_connect"});
			}
			var installer = new global::umbraco.cms.businesslogic.packager.Installer();

			var tempFile = installer.Import(repo.fetch(kitGuid.ToString()));
			installer.LoadConfig(tempFile);
			var pId = installer.CreateManifest(tempFile, kitGuid.ToString(), RepoGuid);
			return Json(new
				{
					success = true,
					manifestId = pId,
					packageFile = tempFile,
					percentage = 10,
					message = "Downloading starter kit files..."
				});
		}

		/// <summary>
		/// Installs the files in the package
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public JsonResult InstallPackageFiles(Guid kitGuid, int manifestId, string packageFile)
		{
			packageFile = Server.UrlDecode(packageFile);
			var installer = new global::umbraco.cms.businesslogic.packager.Installer();
			installer.LoadConfig(packageFile);
			installer.InstallFiles(manifestId, packageFile);
			return Json(new
				{
					success = true,
					manifestId,
					packageFile,
					percentage = 20,
					message = "Installing starter kit files"
				});
		}

		/// <summary>
		/// Ensures the app pool is restarted
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public JsonResult RestartAppPool()
		{
			_applicationContext.RestartApplicationPool(HttpContext);
			return Json(new
				{
					success = true,
					percentage = 25,
					message = "Installing starter kit files"
				});
		}

		/// <summary>
		/// Checks if the app pool has completed restarted
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public JsonResult CheckAppPoolRestart()
		{
			if (HttpContext.Application.AllKeys.Contains("AppPoolRestarting"))
			{
				return Json(new
					{
						successs = false
					});
			}

			return Json(new
				{
					success = true,
					percentage = 30
				});
		}

		/// <summary>
		/// Installs the business logic portion of the package after app restart
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public JsonResult InstallBusinessLogic(Guid kitGuid, int manifestId, string packageFile)
		{
			packageFile = Server.UrlDecode(packageFile);
			var installer = new global::umbraco.cms.businesslogic.packager.Installer();
			installer.LoadConfig(packageFile);
			installer.InstallBusinessLogic(manifestId, packageFile);
			return Json(new
			{
				success = true,
				manifestId,
				packageFile,
				percentage = 70,
				message = "Installing starter kit files"
			});
		}

		/// <summary>
		/// Cleans up the package installation
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public JsonResult CleanupInstallation(Guid kitGuid, int manifestId, string packageFile)
		{
			packageFile = Server.UrlDecode(packageFile);
			var installer = new global::umbraco.cms.businesslogic.packager.Installer();
			installer.LoadConfig(packageFile);
			installer.InstallCleanUp(manifestId, packageFile);

			library.RefreshContent();

			return Json(new
			{
				success = true,
				manifestId,
				packageFile,
				percentage = 100,
				message = "Starter kit has been installed"
			});
		}
	}

}
