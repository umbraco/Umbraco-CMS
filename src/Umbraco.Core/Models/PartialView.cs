using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a Partial View file
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class PartialView : File, IPartialView
{
    public PartialView(string path)
        : this(path, null)
    {
    }

    public PartialView(string path, Func<File, string?>? getFileContent)
        : base(path, getFileContent)
    {
    }
}
