using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a Partial View file
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class PartialView : File, IPartialView
{
    public PartialView(PartialViewType viewType, string path)
        : this(viewType, path, null)
    {
    }

    public PartialView(PartialViewType viewType, string path, Func<File, string?>? getFileContent)
        : base(path, getFileContent) =>
        ViewType = viewType;

    public PartialViewType ViewType { get; set; }
}
