using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IContentImaging
    {
        IEnumerable<string> ImageFileTypes { get; }

        IEnumerable<string> AllowedAttributes { get; }

        IEnumerable<IContentImagingAutoFillUploadField> ImageAutoFillProperties { get; }
    }
}