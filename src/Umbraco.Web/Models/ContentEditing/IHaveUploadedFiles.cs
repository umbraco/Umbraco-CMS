using System.Collections.Generic;
using Umbraco.Core.Models.Editors;

namespace Umbraco.Web.Models.ContentEditing
{
    public interface IHaveUploadedFiles
    {
        List<ContentPropertyFile> UploadedFiles { get; }
    }
}
