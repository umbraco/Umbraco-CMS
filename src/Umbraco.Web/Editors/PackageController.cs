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
            //TODO: Could be too much data down the wire
            return CreatedPackage.GetAllCreatedPackages().Select(x => x.Data).ToList();
        }

        [HttpGet]
        public PackageInstance GetCreatedPackageById(int id)
        {
            //TODO throw an error if cant find by ID
            return CreatedPackage.GetById(id).Data;
        }

        [HttpPost]
        public PackageInstance PostCreatePackage(PackageInstance model)
        {
            var newPackage = CreatedPackage.MakeNew(model.Name);
            var packageId = newPackage.Data.Id;
            var packageGuid = newPackage.Data.PackageGuid;

            //Need to reset the package ID - as the posted model the package ID is always 0
            //MakeNew will init create the XML & update the file and give us an ID to use
            newPackage.Data = model;
            newPackage.Data.Id = packageId;
            newPackage.Data.PackageGuid = packageGuid;
            
            //TODO Validation on the model?!

            //Save then publish
            newPackage.Save();
            newPackage.Publish();

            //We should have packagepath populated now
            return newPackage.Data;
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
