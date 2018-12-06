using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using umbraco.cms.businesslogic.packager;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Editors
{
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
            //TODO: Packager stuff still lives in business logic - YUK
            //TODO: Could be too much data down the wire
            return CreatedPackage.GetAllCreatedPackages();

            /*
             * "author": "Test",
                        "files": [],
                        "iconUrl": "",
                        "id": 1,
                        "license": "MIT License",
                        "licenseUrl": "http://opensource.org/licenses/MIT",
                        "name": "Test v8",
                        "url": "https://test.com",
                        "version": "0.0.0"
                        */


        }

        [HttpGet]
        public CreatedPackage GetCreatedPackageById(int id)
        {
            return CreatedPackage.GetById(id);
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
