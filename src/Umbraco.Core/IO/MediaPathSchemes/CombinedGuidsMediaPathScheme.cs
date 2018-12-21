using System;
using System.IO;

namespace Umbraco.Core.IO.MediaPathSchemes
{
    /// <summary>
    /// Implements a combined-guids media path scheme.
    /// </summary>
    /// <remarks>
    /// <para>Path is "{combinedGuid}/{filename>}" where combinedGuid is a combination of itemGuid and propertyGuid.</para>
    /// </remarks>
    public class CombinedGuidsMediaPathScheme : IMediaPathScheme
    {
        /// <inheritdoc />
        public void Initialize(IFileSystem filesystem)
        { }

        /// <inheritdoc />
        public string GetFilePath(Guid itemGuid, Guid propertyGuid, string filename, string previous = null)
        {
            // assumes that cuid and puid keys can be trusted - and that a single property type
            // for a single content cannot store two different files with the same name
            var directory = HexEncoder.Encode(GuidUtils.Combine(itemGuid, propertyGuid).ToByteArray()/*'/', 2, 4*/); // could use ext to fragment path eg 12/e4/f2/...
            return Path.Combine(directory, filename).Replace('\\', '/');
        }

        /// <inheritdoc />
        public string GetDeleteDirectory(string filepath) => Path.GetDirectoryName(filepath);
    }
}
