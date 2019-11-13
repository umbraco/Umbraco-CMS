using System;
using Umbraco.Core.IO;

namespace Umbraco.Core.Configuration
{
    public static class GlobalSettingsExtensions
    {
        private static string _mvcArea;


        /// <summary>
        /// This returns the string of the MVC Area route.
        /// </summary>
        /// <remarks>
        /// This will return the MVC area that we will route all custom routes through like surface controllers, etc...
        /// We will use the 'Path' (default ~/umbraco) to create it but since it cannot contain '/' and people may specify a path of ~/asdf/asdf/admin
        /// we will convert the '/' to '-' and use that as the path. its a bit lame but will work.
        ///
        /// We also make sure that the virtual directory (SystemDirectories.Root) is stripped off first, otherwise we'd end up with something
        /// like "MyVirtualDirectory-Umbraco" instead of just "Umbraco".
        /// </remarks>
        public static string GetUmbracoMvcArea(this IGlobalSettings globalSettings, ISystemDirectories systemDirectories)
        {
            if (_mvcArea != null) return _mvcArea;

            _mvcArea = GetUmbracoMvcAreaNoCache(globalSettings, systemDirectories);

            return _mvcArea;
        }

        internal static string GetUmbracoMvcAreaNoCache(this IGlobalSettings globalSettings, ISystemDirectories systemDirectories)
        {
            if (globalSettings.Path.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException("Cannot create an MVC Area path without the umbracoPath specified");
            }

            var path = globalSettings.Path;
            if (path.StartsWith(systemDirectories.Root)) // beware of TrimStart, see U4-2518
                path = path.Substring(systemDirectories.Root.Length);
            return path.TrimStart('~').TrimStart('/').Replace('/', '-').Trim().ToLower();
        }

    }
}
