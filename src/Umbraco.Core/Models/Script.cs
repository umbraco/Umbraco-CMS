using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a Script file
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class Script : File, IScript
{
    public Script(string path)
        : this(path, null)
    {
    }

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
