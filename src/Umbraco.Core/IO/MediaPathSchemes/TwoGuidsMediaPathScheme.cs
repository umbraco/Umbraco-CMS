using System;
using System.IO;

namespace Umbraco.Core.IO.MediaPathSchemes
{
    /// <summary>
    /// Implements a two-guids media path scheme.
    /// </summary>
    /// <remarks>
    /// <para>Path is "{itemGuid}/{propertyGuid}/{filename}".</para>
    /// </remarks>
    public class TwoGuidsMediaPathScheme : IMediaPathScheme
    {
        /// <inheritdoc />
        public void Initialize(IFileSystem filesystem)
        { }

        /// <inheritdoc />
        public string GetFilePath(Guid itemGuid, Guid propertyGuid, string filename, string previous = null)
        {
            return Path.Combine(itemGuid.ToString("N"), propertyGuid.ToString("N"), filename).Replace('\\', '/');
        }

        /// <inheritdoc />
        public string GetDeleteDirectory(string filepath)
        {
            return Path.GetDirectoryName(filepath);
        }
    }
}
