using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using Umbraco.Core.Models.Packaging;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Web._Legacy.Packager.PackageInstance;

namespace Umbraco.Web.Editors
{
    //TODO: Packager stuff still lives in business logic - YUK

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
            return Services.PackagingService.GetAll();
        }

        public PackageDefinition GetCreatedPackageById(int id)
        {
            var package = Services.PackagingService.GetById(id);
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
            if (!Services.PackagingService.SavePackage(model))
                throw new HttpResponseException(Request.CreateNotificationValidationErrorResponse("The package with id {definition.Id} was not found"));

            Services.PackagingService.ExportPackage(model);

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
            Services.PackagingService.Delete(packageId);

            return Ok();
        }
        
    }
}
