using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Security;

namespace Umbraco.Web.Editors.Filters
{
    /// <summary>
    /// Validator for <see cref="MediaItemSave"/>
    /// </summary>
    internal class MediaSaveModelValidator : ContentModelValidator<IMedia, MediaItemSave, IContentProperties<ContentPropertyBasic>>
    {
        public MediaSaveModelValidator(ILogger logger, IWebSecurity webSecurity, ILocalizedTextService textService) : base(logger, webSecurity, textService)
        {
        }
    }
}
