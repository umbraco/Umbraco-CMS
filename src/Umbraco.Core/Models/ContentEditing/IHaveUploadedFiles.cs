using Umbraco.Cms.Core.Models.Editors;

namespace Umbraco.Cms.Core.Models.ContentEditing;

public interface IHaveUploadedFiles
{
    List<ContentPropertyFile> UploadedFiles { get; }
}
