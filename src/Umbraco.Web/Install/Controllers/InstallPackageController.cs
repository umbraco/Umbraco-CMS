using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using umbraco;
using Umbraco.Core;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Install.Controllers
{
    /// <summary>
	/// A  controller for the installation process regarding packages
	/// </summary>
	/// <remarks>
	/// Currently this is used for web services however we should/could eventually migrate the whole installer to MVC as it
	/// is a bit of a mess currently.
	/// </remarks>
	[HttpInstallAuthorize]
    [Obsolete("This is only used for the legacy way of installing starter kits in the back office")]
    public class InstallPackageController : ApiController
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
        public HttpResponseMessage Index()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Connects to the repo, downloads the package and creates the manifest
		/// </summary>
		/// <param name="kitGuid"></param>
		/// <returns></returns>
		[HttpPost]
        public HttpResponseMessage DownloadPackageFiles(Guid kitGuid)
		{
			var repo = global::umbraco.cms.businesslogic.packager.repositories.Repository.getByGuid(RepoGuid);
            if (repo == null)
            {
                return Json(
                    new {success = false, error = "No repository found with id " + RepoGuid},
                    HttpStatusCode.BadGateway);
            }
			if (repo.HasConnection() == false)
			{
                return Json(
                    new { success = false, error = "cannot_connect" },
                    HttpStatusCode.BadGateway);
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
				}, HttpStatusCode.OK);
		}

		/// <summary>
		/// Installs the files in the package
		/// </summary>
		/// <returns></returns>
		[HttpPost]
        public HttpResponseMessage InstallPackageFiles(Guid kitGuid, int manifestId, string packageFile)
		{
			packageFile = HttpUtility.UrlDecode(packageFile);
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
				}, HttpStatusCode.OK);
		}

		/// <summary>
		/// Ensures the app pool is restarted
		/// </summary>
		/// <returns></returns>
		[HttpPost]
        public HttpResponseMessage RestartAppPool()
		{
			_applicationContext.RestartApplicationPool(Request.TryGetHttpContext().Result);
			return Json(new
				{
					success = true,
					percentage = 25,
					message = "Installing starter kit files"
				}, HttpStatusCode.OK);
		}

		/// <summary>
		/// Checks if the app pool has completed restarted
		/// </summary>
		/// <returns></returns>
		[HttpPost]
        public HttpResponseMessage CheckAppPoolRestart()
		{
            if (Request.TryGetHttpContext().Result.Application.AllKeys.Contains("AppPoolRestarting"))
			{
                return Request.CreateResponse(HttpStatusCode.BadRequest);
			}

			return Json(new
				{					
					percentage = 30
				}, HttpStatusCode.OK);
		}

		/// <summary>
		/// Installs the business logic portion of the package after app restart
		/// </summary>
		/// <returns></returns>
		[HttpPost]
        public HttpResponseMessage InstallBusinessLogic(Guid kitGuid, int manifestId, string packageFile)
		{
			packageFile = HttpUtility.UrlDecode(packageFile);
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
			}, HttpStatusCode.OK);
		}

		/// <summary>
		/// Cleans up the package installation
		/// </summary>
		/// <returns></returns>
		[HttpPost]
        public HttpResponseMessage CleanupInstallation(Guid kitGuid, int manifestId, string packageFile)
		{
			packageFile = HttpUtility.UrlDecode(packageFile);
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
			}, HttpStatusCode.OK);
		}

        private HttpResponseMessage Json(object jsonObject, HttpStatusCode status)
        {
            var response = Request.CreateResponse(status);
            var json = JObject.FromObject(jsonObject);
            response.Content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
            return response;
        }
	}

}
