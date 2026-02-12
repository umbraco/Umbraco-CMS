using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a Partial View file
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class PartialView : File, IPartialView
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PartialView" /> class with the specified path.
    /// </summary>
    /// <param name="path">The path to the partial view file.</param>
    public PartialView(string path)
        : this(path, null)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PartialView" /> class with the specified path and content retrieval function.
    /// </summary>
    /// <param name="path">The path to the partial view file.</param>
    /// <param name="getFileContent">A function to retrieve the file content.</param>
    public PartialView(string path, Func<File, string?>? getFileContent)
        : base(path, getFileContent)
    {
    }
}
