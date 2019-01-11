using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Umbraco.Core.IO;
using Umbraco.Core.Models.Packaging;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// A controller used for installing packages and managing all of the data in the packages section in the back office
    /// </summary>
    [PluginController("UmbracoApi")]
    [SerializeVersion]
    [UmbracoApplicationAuthorize(Core.Constants.Applications.Packages)]
    public class PackageController : UmbracoAuthorizedJsonController
    {
        public IEnumerable<PackageDefinition> GetCreatedPackages()
        {
            return Services.PackagingService.GetAllCreatedPackages();
        }

        public PackageDefinition GetCreatedPackageById(int id)
        {
            var package = Services.PackagingService.GetCreatedPackageById(id);
            if (package == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            return package;
        }

        public PackageDefinition GetEmpty()
        {
            return new PackageDefinition();
        }

        /// <summary>
        /// Creates or updates a package
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public PackageDefinition PostSavePackage(PackageDefinition model)
        {
            if (ModelState.IsValid == false)
                throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));

            //save it
            if (!Services.PackagingService.SaveCreatedPackage(model))
                throw new HttpResponseException(
                    Request.CreateNotificationValidationErrorResponse(
                        model.Id == default
                            ? $"A package with the name {model.Name} already exists"
                            : $"The package with id {model.Id} was not found"));

            Services.PackagingService.ExportCreatedPackage(model);

            //the packagePath will be on the model 
            return model;
        }

        /// <summary>
        /// Deletes a created package
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        [HttpPost]
        [HttpDelete]
        public IHttpActionResult DeleteCreatedPackage(int packageId)
        {
            Services.PackagingService.DeleteCreatedPackage(packageId, Security.GetUserId().ResultOr(0));

            return Ok();
        }

        [HttpGet]
        public HttpResponseMessage DownloadCreatedPackage(int id)
        {
            var package = Services.PackagingService.GetCreatedPackageById(id);
            if (package == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var fullPath = IOHelper.MapPath(package.PackagePath);
            if (!File.Exists(fullPath))
                return Request.CreateNotificationValidationErrorResponse("No file found for path " + package.PackagePath);

            var fileName = Path.GetFileName(package.PackagePath);

            var response = new HttpResponseMessage
            {
                Content = new StreamContent(File.OpenRead(fullPath))
                {
                    Headers =
                    {
                        ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = fileName
                        },
                        ContentType = new MediaTypeHeaderValue( "application/octet-stream")
                    }
                }
            };

            // Set custom header so umbRequestHelper.downloadFile can save the correct filename
            response.Headers.Add("x-filename", fileName);

            return response;
        }

    }
}
