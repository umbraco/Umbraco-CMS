using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Extensions;

public static class ModelsBuilderConfigExtensions
{
    private static string? _modelsDirectoryAbsolute;

    public static string ModelsDirectoryAbsolute(
        this ModelsBuilderSettings modelsBuilderConfig,
        IHostingEnvironment hostingEnvironment)
    {
        if (_modelsDirectoryAbsolute is null)
        {
            var modelsDirectory = modelsBuilderConfig.ModelsDirectory;
            var root = hostingEnvironment.MapPathContentRoot("~/");

            _modelsDirectoryAbsolute = GetModelsDirectory(root, modelsDirectory, modelsBuilderConfig.AcceptUnsafeModelsDirectory);
        }

        return _modelsDirectoryAbsolute;
    }

    // internal for tests
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
