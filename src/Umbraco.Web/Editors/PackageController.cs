using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using umbraco.cms.businesslogic.packager;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Editors
{
    //TODO: Packager stuff still lives in business logic - YUK

    /// <summary>
    /// A controller used for installing packages and managing all of the data in the packages section in the back office
    /// </summary>
    [PluginController("UmbracoApi")]
    [UmbracoApplicationAuthorize(Core.Constants.Applications.Packages)]
    public class PackageController : UmbracoAuthorizedJsonController
    {
        [HttpGet]
        public List<CreatedPackage> GetCreatedPackages()
        {
            //TODO: Could be too much data down the wire
            return CreatedPackage.GetAllCreatedPackages();
        }

        [HttpGet]
        public CreatedPackage GetCreatedPackageById(int id)
        {
            return CreatedPackage.GetById(id);
        }

        [HttpPost]
        public CreatedPackage PostCreatePackage(PackageInstance model)
        {
            //TODO Validation on the model?!
            var newPackage = new CreatedPackage
            {
                Data = model
            };

            //Save then publish
            newPackage.Save();
            newPackage.Publish();
            
            //We should have packagepath populated now
            return newPackage;
        }

        [HttpDelete]
        public HttpResponseMessage DeleteCreatedPackageById(int id)
        {
            var package = CreatedPackage.GetById(id);
            package.Delete();

            //204 No Content
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

    }
}
