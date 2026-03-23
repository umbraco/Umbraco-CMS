using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a Script file
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class Script : File, IScript
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Script" /> class with a file path.
    /// </summary>
    /// <param name="path">The path to the script file.</param>
    public Script(string path)
        : this(path, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Script" /> class with a file path and content provider.
    /// </summary>
    /// <param name="path">The path to the script file.</param>
    /// <param name="getFileContent">A function to retrieve the file content lazily.</param>
    public Script(string path, Func<File, string?>? getFileContent)
        : base(path, getFileContent)
    {
    }

    /// <summary>
    ///     Indicates whether the current entity has an identity, which in this case is a path/name.
    /// </summary>
    /// <remarks>
    ///     Overrides the default Entity identity check.
    /// </remarks>
    public override bool HasIdentity => string.IsNullOrEmpty(Path) == false;
}
