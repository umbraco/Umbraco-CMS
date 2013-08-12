using System.Collections.Generic;

namespace Umbraco.Web.Models.ContentEditing
{
    public interface IHaveUploadedFiles
    {
        List<ContentItemFile> UploadedFiles { get; }
    }
}