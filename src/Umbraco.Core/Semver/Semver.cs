namespace Umbraco.Cms.Core.Semver;

public sealed class SemVersion
{
    public SemVersion(int major = 1, int minor = 0, int patch = 0, string prerelease = "", string metadata = "")
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        Prerelease = prerelease;
        Metadata = metadata;
    }

    /// <summary>
    ///     Gets the major version.
    /// </summary>
    /// <value>
    ///     The major version.
    /// </value>
    public int Major { get; }

    /// <summary>
    ///     Gets the minor version.
    /// </summary>
    /// <value>
    ///     The minor version.
    /// </value>
    public int Minor { get; }

    /// <summary>
    ///     Gets the patch version.
    /// </summary>
    /// <value>
    ///     The patch version.
    /// </value>
    public int Patch { get; }

    /// <summary>
    ///     Gets the pre-release version.
    /// </summary>
    /// <value>
    ///     The pre-release version.
    /// </value>
    public string Prerelease { get;  }

    /// <summary>
    ///     Gets the version metadata.
    /// </summary>
    /// <value>
    ///     The version metadata.
    /// </value>
    public string Metadata { get; }

    /// <summary>
    ///     Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>
    ///     A <see cref="System.String" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        var version = string.Empty + Major + "." + Minor + "." + Patch;
        if (!string.IsNullOrEmpty(Prerelease))
        {
            version += "-" + Prerelease;
        }

        if (!string.IsNullOrEmpty(Metadata))
        {
            version += "+" + Metadata;
        }

        return version;
    }
}

