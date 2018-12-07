using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using umbraco.cms.businesslogic.packager;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;

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
        [HttpGet]
        public List<PackageInstance> GetCreatedPackages()
        {
            return CreatedPackage.GetAllCreatedPackages().Select(x => x.Data).ToList();
        }

        [HttpGet]
        public PackageInstance GetCreatedPackageById(int id)
        {
            var package = CreatedPackage.GetById(id);
            if (package == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);
            
            return package.Data;
        }

        [HttpPost]
        public PackageInstance PostCreatePackage(PackageInstance model)
        {
            if (ModelState.IsValid == false)
            {
                //Throw/bubble up errors
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }
            
            var package = new CreatedPackage
            {
                Data = model
            };

            //If ID is 0 & PackageGuid is null - its brand new
            if(model.Id == 0 && model.PackageGuid == null)
            {
                //Brand new
                package = CreatedPackage.MakeNew(model.Name);

                var packageId = package.Data.Id;
                var packageGuid = package.Data.PackageGuid;

                //Need to reset the package ID - as the posted model the package ID is always 0
                //MakeNew will init create the XML & update the file and give us an ID to use
                package.Data = model;
                package.Data.Id = packageId;
                package.Data.PackageGuid = packageGuid;
            }
            
            //Save then publish
            package.Save();
            package.Publish();

            //We should have packagepath populated now
            return package.Data;
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
