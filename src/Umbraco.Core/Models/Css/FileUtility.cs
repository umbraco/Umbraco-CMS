using System;
using System.Collections.Generic;
using System.IO;

namespace Umbraco.Core.Models.Css
{
    internal class FileUtility
    {
        #region Constants

        private static readonly char[] IllegalChars;

        #endregion Constants

        #region Init

        static FileUtility()
        {
            var chars = new List<char>(Path.GetInvalidPathChars());
            foreach (char ch in Path.GetInvalidFileNameChars())
            {
                if (!chars.Contains(ch) && ch != Path.DirectorySeparatorChar)
                {
                    chars.Add(ch);
                }
            }
            IllegalChars = chars.ToArray();
        }

        #endregion Init

        #region Methods

        /// <summary>
        /// Makes sure directory exists and if file exists is not readonly.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>if valid path</returns>
        internal static bool PrepSavePath(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                // make sure not readonly
                FileAttributes attributes = System.IO.File.GetAttributes(filename);
                attributes &= ~FileAttributes.ReadOnly;
                System.IO.File.SetAttributes(filename, attributes);
            }
            else
            {
                string dir = Path.GetDirectoryName(filename);
                if (!String.IsNullOrEmpty(dir) && dir.IndexOfAny(IllegalChars) >= 0)
                {
                    return false;
                }
                if (!String.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    // make sure path exists
                    Directory.CreateDirectory(dir);
                }
                string file = Path.GetFileName(filename);
                if (!String.IsNullOrEmpty(file) && file.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    return false;
                }
            }
            return true;
        }

        #endregion Methods
    }
}