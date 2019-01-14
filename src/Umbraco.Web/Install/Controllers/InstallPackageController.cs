using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using umbraco;
using Umbraco.Core;
using Umbraco.Web.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Packaging;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Install.Models;
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
    [AngularJsonOnlyConfiguration]
    [Obsolete("This is only used for the legacy way of installing starter kits in the back office")]
    //fixme: Is this used anymore??
    public class InstallPackageController : ApiController
    {
        private readonly IPackagingService _packagingService;
        private readonly UmbracoContext _umbracoContext;

        public InstallPackageController(IPackagingService packagingService, UmbracoContext umbracoContext)
        {
            _packagingService = packagingService;
            _umbracoContext = umbracoContext;
        }

        /// <summary>
        /// Empty action, useful for retrieving the base url for this controller
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage Index()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Connects to the repo, downloads the package and creates the definition
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<HttpResponseMessage> DownloadPackageFiles(InstallPackageModel model)
        {
            var packageFile = await _packagingService.FetchPackageFileAsync(
                model.KitGuid,
                UmbracoVersion.Current,
                UmbracoContext.Current.Security.CurrentUser.Id);

            
            var packageInfo = _packagingService.GetCompiledPackageInfo(packageFile);
            if (packageInfo == null) throw new InvalidOperationException("Could not read package file " + packageFile);

            //save to the installedPackages.config
            var packageDefinition = PackageDefinition.FromCompiledPackage(packageInfo);
            _packagingService.SaveInstalledPackage(packageDefinition);

            return Json(new
                {
                    success = true,
                    packageId = packageDefinition.Id,
                    packageFile = packageInfo.PackageFileName,
                    percentage = 10,
                    message = "Downloading starter kit files..."
                }, HttpStatusCode.OK);
        }

        /// <summary>
        /// Installs the files in the package
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage InstallPackageFiles(InstallPackageModel model)
        {
            model.PackageFile = HttpUtility.UrlDecode(model.PackageFile);

            var definition = _packagingService.GetInstalledPackageById(model.PackageId);
            if (definition == null) throw new InvalidOperationException("Not package definition found with id " + model.PackageId);

            _packagingService.InstallCompiledPackageFiles(definition, model.PackageFile, _umbracoContext.Security.GetUserId().ResultOr(0));

            return Json(new
                {
                    success = true,
                    ManifestId = model.PackageId,
                    model.PackageFile,
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
            Current.RestartAppPool(Request.TryGetHttpContext().Result);
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
                    percentage = 30,
                    success = true,
                }, HttpStatusCode.OK);
        }

        /// <summary>
        /// Installs the business logic portion of the package after app restart
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage InstallBusinessLogic(InstallPackageModel model)
        {
            model.PackageFile = HttpUtility.UrlDecode(model.PackageFile);

            var definition = _packagingService.GetInstalledPackageById(model.PackageId);
            if (definition == null) throw new InvalidOperationException("Not package definition found with id " + model.PackageId);

            _packagingService.InstallCompiledPackageData(definition, model.PackageFile, _umbracoContext.Security.GetUserId().ResultOr(0));

            return Json(new
            {
                success = true,
                ManifestId = model.PackageId,
                model.PackageFile,
                percentage = 70,
                message = "Installing starter kit files"
            }, HttpStatusCode.OK);
        }

        /// <summary>
        /// Cleans up the package installation
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage CleanupInstallation(InstallPackageModel model)
        {
            model.PackageFile = HttpUtility.UrlDecode(model.PackageFile);
            
            return Json(new
            {
                success = true,
                ManifestId = model.PackageId,
                model.PackageFile,
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
