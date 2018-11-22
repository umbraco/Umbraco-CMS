using System;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Validation;

namespace Umbraco.Web.WebApi
{
    /// <summary>
    /// By default WebApi always appends a prefix to any ModelState error but we don't want this,
    /// so this is a custom validator that ensures there is no prefix set.
    /// </summary>
    /// <remarks>
    /// We were already doing this with the content/media/members validation since we had to manually validate because we
    /// were posting multi-part values. We were always passing in an empty prefix so it worked. However for other editors we
    /// are validating with normal data annotations (for the most part) and we don't want the prefix there either.
    /// </remarks>
    internal class PrefixlessBodyModelValidator : IBodyModelValidator
    {
        private readonly IBodyModelValidator _innerValidator;

        public PrefixlessBodyModelValidator(IBodyModelValidator innerValidator)
        {
            if (innerValidator == null)
            {
                throw new ArgumentNullException("innerValidator");
            }

            _innerValidator = innerValidator;
        }

        public bool Validate(object model, Type type, ModelMetadataProvider metadataProvider, HttpActionContext actionContext, string keyPrefix)
        {
            // Remove the keyPrefix but otherwise let innerValidator do what it normally does.
            return _innerValidator.Validate(model, type, metadataProvider, actionContext, string.Empty);
        }
    }
}