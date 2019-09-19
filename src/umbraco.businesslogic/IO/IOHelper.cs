using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using System.Configuration;
using System.Web;
using umbraco.BusinessLogic;
using System.Text.RegularExpressions;
using umbraco.businesslogic.Exceptions;

namespace umbraco.IO
{
    [Obsolete("Use Umbraco.Core.IO.IOHelper instead")]
    public static class IOHelper
    {
        public static char DirSepChar
        {
            get
            {
                return Umbraco.Core.IO.IOHelper.DirSepChar;
            }
        }

        //helper to try and match the old path to a new virtual one
        public static string FindFile(string virtualPath)
        {
            return Umbraco.Core.IO.IOHelper.FindFile(virtualPath);
        }

        //Replaces tildes with the root dir
        public static string ResolveUrl(string virtualPath)
        {
            return Umbraco.Core.IO.IOHelper.ResolveUrl(virtualPath);
        }

        [Obsolete("Use Umbraco.Web.Templates.TemplateUtilities.ResolveUrlsFromTextString instead, this method on this class will be removed in future versions")]
        public static string ResolveUrlsFromTextString(string text)
        {
            return Umbraco.Core.IO.IOHelper.ResolveUrlsFromTextString(text);
        }

        public static string MapPath(string path, bool useHttpContext)
        {
            return Umbraco.Core.IO.IOHelper.MapPath(path, useHttpContext);
        }

        public static string MapPath(string path)
        {
            return Umbraco.Core.IO.IOHelper.MapPath(path);
        }

        //use a tilde character instead of the complete path
        [Obsolete("This method is no longer in use and will be removed in future versions")]
        public static string returnPath(string settingsKey, string standardPath, bool useTilde)
        {
            return Umbraco.Core.IO.IOHelper.ReturnPath(settingsKey, standardPath, useTilde);
        }

        [Obsolete("This method is no longer in use and will be removed in future versions")]
        public static string returnPath(string settingsKey, string standardPath)
        {
            return Umbraco.Core.IO.IOHelper.ReturnPath(settingsKey, standardPath);

        }


        /// <summary>
        /// Validates if the current filepath matches a directory where the user is allowed to edit a file
        /// </summary>
        /// <param name="filePath">filepath </param>
        /// <param name="validDir"></param>
        /// <returns>true if valid, throws a FileSecurityException if not</returns>
        public static bool ValidateEditPath(string filePath, string validDir)
        {
            return Umbraco.Core.IO.IOHelper.ValidateEditPath(filePath, validDir);
        }

        public static bool ValidateFileExtension(string filePath, List<string> validFileExtensions)
        {
            return Umbraco.Core.IO.IOHelper.ValidateFileExtension(filePath, validFileExtensions);
        }


        /// <summary>
        /// Returns the path to the root of the application, by getting the path to where the assembly where this
        /// method is included is present, then traversing until it's past the /bin directory. Ie. this makes it work
        /// even if the assembly is in a /bin/debug or /bin/release folder
        /// </summary>
        /// <returns></returns>
        private static string getRootDirectorySafe()
        {
            return Umbraco.Core.IO.IOHelper.GetRootDirectorySafe();
        }

    }
}
