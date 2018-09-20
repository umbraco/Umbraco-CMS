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
            var directory = Combine(itemGuid, propertyGuid).ToHexString(/*'/', 2, 4*/); // could use ext to fragment path eg 12/e4/f2/...
            return Path.Combine(directory, filename).Replace('\\', '/');
        }

        /// <inheritdoc />
        public string GetDeleteDirectory(string filepath)
        {
            return Path.GetDirectoryName(filepath);
        }

        private static byte[] Combine(Guid guid1, Guid guid2)
        {
            var bytes1 = guid1.ToByteArray();
            var bytes2 = guid2.ToByteArray();
            var bytes = new byte[bytes1.Length];
            for (var i = 0; i < bytes1.Length; i++)
                bytes[i] = (byte) (bytes1[i] ^ bytes2[i]);
            return bytes;
        }
    }
}
