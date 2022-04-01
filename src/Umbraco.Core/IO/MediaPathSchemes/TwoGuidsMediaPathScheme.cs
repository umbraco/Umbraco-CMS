using System;
using System.IO;

namespace Umbraco.Cms.Core.IO.MediaPathSchemes
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
        public string GetFilePath(MediaFileManager fileManager, Guid? itemGuid, Guid? propertyGuid, string filename)
        {
            return Path.Combine(itemGuid?.ToString("N"), propertyGuid?.ToString("N"), filename).Replace('\\', '/');
        }

        /// <inheritdoc />
        public string GetDeleteDirectory(MediaFileManager fileManager, string filepath)
        {
            return Path.GetDirectoryName(filepath)!;
        }
    }
}
