﻿using Microsoft.AspNetCore.Mvc;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.Filters;

namespace Umbraco.Web.BackOffice.Controllers
{
    /// <summary>
    /// An abstract API controller that only supports JSON and all requests must contain the correct csrf header
    /// </summary>
    /// <remarks>
    /// Inheriting from this controller means that ALL of your methods are JSON methods that are called by Angular,
    /// methods that are not called by Angular or don't contain a valid csrf header will NOT work.
    /// </remarks>
    [TypeFilter(typeof(ValidateAngularAntiForgeryTokenAttribute))]
    [TypeFilter(typeof(AngularJsonOnlyConfigurationAttribute))] // TODO: This could be applied with our Application Model conventions
    public abstract class UmbracoAuthorizedJsonController : UmbracoAuthorizedApiController
    {

    }
}
