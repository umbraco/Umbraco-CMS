using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Umbraco.Cms.Web.Common.Extensions
{
    public static class WebHostEnvironmentExtensions
    {
        public static string MapPathContentRoot(this IWebHostEnvironment webHostEnvironment, string path) => MapPath(webHostEnvironment.ContentRootPath, path);
        
        public static string MapPathWebRoot(this IWebHostEnvironment webHostEnvironment, string path) => MapPath(webHostEnvironment.WebRootPath, path);

        internal static string MapPath(string root, string path)
        {
            var newPath = path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

            // TODO: This is a temporary error because we switched from IOHelper.MapPath to HostingEnvironment.MapPathXXX
            // IOHelper would check if the path passed in started with the root, and not prepend the root again if it did,
            // however if you are requesting a path be mapped, it should always assume the path is relative to the root, not
            // absolute in the file system.  This error will help us find and fix improper uses, and should be removed once
            // all those uses have been found and fixed
            if (newPath.StartsWith(root))
            {
                throw new ArgumentException("The path appears to already be fully qualified.  Please remove the call to MapPath");
            }

            return Path.Combine(root, newPath.TrimStart(Core.Constants.CharArrays.TildeForwardSlashBackSlash));
        }
    }
}
