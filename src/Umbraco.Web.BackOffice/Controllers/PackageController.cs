using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Hosting;
using Umbraco.Core.Models.Packaging;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Exceptions;
using Umbraco.Web.Security;

namespace Umbraco.Web.BackOffice.Controllers
{
    /// <summary>
    /// A controller used for managing packages in the back office
    /// </summary>
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [UmbracoApplicationAuthorizeAttribute(Constants.Applications.Packages)]
    public class PackageController : UmbracoAuthorizedJsonController
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IPackagingService _packagingService;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;

        public PackageController(
            IHostingEnvironment hostingEnvironment,
            IPackagingService packagingService,
            IBackOfficeSecurityAccessor backofficeSecurityAccessor)
        {
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            _packagingService = packagingService ?? throw new ArgumentNullException(nameof(packagingService));
            _backofficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
        }

        public IEnumerable<PackageDefinition> GetCreatedPackages()
        {
            return _packagingService.GetAllCreatedPackages();
        }

        public PackageDefinition GetCreatedPackageById(int id)
        {
            var package = _packagingService.GetCreatedPackageById(id);
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
                throw HttpResponseException.CreateValidationErrorResponse(ModelState);

            //save it
            if (!_packagingService.SaveCreatedPackage(model))
                throw HttpResponseException.CreateNotificationValidationErrorResponse(
                        model.Id == default
                            ? $"A package with the name {model.Name} already exists"
                            : $"The package with id {model.Id} was not found");

            _packagingService.ExportCreatedPackage(model);

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
        public IActionResult DeleteCreatedPackage(int packageId)
        {
            _packagingService.DeleteCreatedPackage(packageId, _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(0));

            return Ok();
        }

        [HttpGet]
        public IActionResult DownloadCreatedPackage(int id)
        {
            var package = _packagingService.GetCreatedPackageById(id);
            if (package == null)
                return NotFound();

            var fullPath = _hostingEnvironment.MapPathContentRoot(package.PackagePath);
            if (!System.IO.File.Exists(fullPath))
                throw HttpResponseException.CreateNotificationValidationErrorResponse("No file found for path " + package.PackagePath);

            var fileName = Path.GetFileName(package.PackagePath);

            var encoding = Encoding.UTF8;

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName =  WebUtility.UrlEncode(fileName),
                Inline = false  // false = prompt the user for downloading;  true = browser to try to show the file inline
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            // Set custom header so umbRequestHelper.downloadFile can save the correct filename
            Response.Headers.Add("x-filename", WebUtility.UrlEncode(fileName));
            return new FileStreamResult(System.IO.File.OpenRead(fullPath), new MediaTypeHeaderValue("application/octet-stream")
            {
                Charset = encoding.WebName,
            });

        }

        public PackageDefinition GetInstalledPackageById(int id)
        {
            var pack = _packagingService.GetInstalledPackageById(id);
            if (pack == null) throw new HttpResponseException(HttpStatusCode.NotFound);
            return pack;
        }

        /// <summary>
        /// Returns all installed packages - only shows their latest versions
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PackageDefinition> GetInstalled()
        {
            return _packagingService.GetAllInstalledPackages()
                .GroupBy(
                    //group by name
                    x => x.Name,
                    //select the package with a parsed version
                    pck => SemVersion.TryParse(pck.Version, out var pckVersion)
                        ? new { package = pck, version = pckVersion }
                        : new { package = pck, version = new SemVersion(0, 0, 0) })
                .Select(grouping =>
                {
                    //get the max version for the package
                    var maxVersion = grouping.Max(x => x.version);
                    //only return the first package with this version
                    return grouping.First(x => x.version == maxVersion).package;
                })
                .ToList();
        }
    }
}
