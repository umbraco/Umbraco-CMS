using System;
using System.IO;

namespace Umbraco.Core.IO.MediaPathSchemes
{
    /// <summary>
    /// Implements a unique directory media path scheme.
    /// </summary>
    /// <remarks>
    /// <para>This scheme provides short paths, yet handle potential collisions.</para>
    /// </remarks>
    public class UniqueMediaPathScheme : IMediaPathScheme
    {
        private const int DirectoryLength = 8;

        /// <inheritdoc />
        public string GetFilePath(IMediaFileSystem fileSystem, Guid itemGuid, Guid propertyGuid, string filename, string previous = null)
        {
            string directory;

            // no point "combining" guids if all we want is some random guid - just get a new one
            // and then, because we don't want collisions, ensure that the directory does not already exist
            // (should be quite rare, but eh...)

            do
            {
                var combinedGuid = Guid.NewGuid();
                directory = GuidUtils.ToBase32String(combinedGuid, DirectoryLength); // see also HexEncoder, we may want to fragment path eg 12/e4/f3...

            } while (fileSystem.DirectoryExists(directory));

            return Path.Combine(directory, filename).Replace('\\', '/');
        }

        /// <inheritdoc />
        public string GetDeleteDirectory(IMediaFileSystem fileSystem, string filepath) => Path.GetDirectoryName(filepath);
    }
}