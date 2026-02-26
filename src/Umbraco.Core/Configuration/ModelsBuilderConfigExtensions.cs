using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Extensions;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for <see cref="ModelsBuilderSettings" />.
/// </summary>
public static class ModelsBuilderConfigExtensions
{
    private static string? _modelsDirectoryAbsolute;

    /// <summary>
    ///     Gets the absolute path to the models directory.
    /// </summary>
    /// <param name="modelsBuilderConfig">The models builder configuration.</param>
    /// <param name="hostEnvironment">The host environment.</param>
    /// <returns>The absolute path to the models directory.</returns>
    public static string ModelsDirectoryAbsolute(
        this ModelsBuilderSettings modelsBuilderConfig,
        IHostEnvironment hostEnvironment)
    {
        if (_modelsDirectoryAbsolute is null)
        {
            var modelsDirectory = modelsBuilderConfig.ModelsDirectory;
            var root = hostEnvironment.MapPathContentRoot("~/");

            _modelsDirectoryAbsolute = GetModelsDirectory(root, modelsDirectory, modelsBuilderConfig.AcceptUnsafeModelsDirectory);
        }

        return _modelsDirectoryAbsolute;
    }

    /// <summary>
    ///     Gets the models directory from the configuration.
    /// </summary>
    /// <param name="root">The root path.</param>
    /// <param name="config">The configured models directory.</param>
    /// <param name="acceptUnsafe">Whether to accept unsafe directory paths.</param>
    /// <returns>The absolute path to the models directory.</returns>
    /// <remarks>Internal for tests.</remarks>
    internal static string GetModelsDirectory(string root, string config, bool acceptUnsafe)
    {
        // making sure it is safe, ie under the website root,
        // unless AcceptUnsafeModelsDirectory and then everything is OK.
        if (!Path.IsPathRooted(root))
        {
            throw new ConfigurationException($"Root is not rooted \"{root}\".");
        }

        if (config.StartsWith("~/"))
        {
            var dir = Path.Combine(root, config.TrimStart("~/"));

            // sanitize - GetFullPath will take care of any relative
            // segments in path, eg '../../foo.tmp' - it may throw a SecurityException
            // if the combined path reaches illegal parts of the filesystem
            dir = Path.GetFullPath(dir);
            root = Path.GetFullPath(root);

            if (!dir.StartsWith(root) && !acceptUnsafe)
            {
                throw new ConfigurationException($"Invalid models directory \"{config}\".");
            }

            return dir;
        }

        if (acceptUnsafe)
        {
            return Path.GetFullPath(config);
        }

        throw new ConfigurationException($"Invalid models directory \"{config}\".");
    }
}
