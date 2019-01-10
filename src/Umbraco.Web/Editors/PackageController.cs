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
        public List<PackageDefinition> GetCreatedPackages()
        {
            return CreatedPackage.GetAllCreatedPackages().Select(x => x.Data).ToList();
        }

        public PackageDefinition GetCreatedPackageById(int id)
        {
            var package = CreatedPackage.GetById(id);
            if (package == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);
            
            return package.Data;
        }

        public PackageDefinition PostUpdatePackage(PackageDefinition model)
        {
            var package = CreatedPackage.GetById(model.Id);
            if (package == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            if (ModelState.IsValid == false)
            {
                //Throw/bubble up errors
                throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));
            }

            package.Data = model;

            //We should have packagepath populated now
            return package.Data;
        }

        public PackageDefinition PostCreatePackage(PackageDefinition model)
        {
            //creating requires an empty model/package id
            if (model.Id != 0 || model.PackageGuid != null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            if (ModelState.IsValid == false)
            {
                //Throw/bubble up errors
                throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));
            }

            //save it
            Services.PackagingService.SavePackageDefinition(model);

            //then publish to get the file
            //package.Publish();
            //TODO: We need a link to the downloadable zip file, in packagepath ?
            
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
            var package = CreatedPackage.GetById(packageId);
            if (package == null)
                return NotFound();

            package.Delete();

            return Ok();
        }
        
    }
}
