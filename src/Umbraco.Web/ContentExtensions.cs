using System;
using System.Web;
using Umbraco.Core.IO;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.Web
{
    public static class ContentExtensions
    {
        /// <summary>
        /// Sets the posted file value of a property.
        /// </summary>
        public static void SetValue(this IContentBase content, string propertyTypeAlias, HttpPostedFileBase value, string culture = null, string segment = null)
        {
            // ensure we get the filename without the path in IE in intranet mode
            // http://stackoverflow.com/questions/382464/httppostedfile-filename-different-from-ie
            var filename = value.FileName;
            var pos = filename.LastIndexOf(@"\", StringComparison.InvariantCulture);
            if (pos > 0)
                filename = filename.Substring(pos + 1);

            // strip any directory info
            pos = filename.LastIndexOf(IOHelper.DirSepChar);
            if (pos > 0)
                filename = filename.Substring(pos + 1);

            content.SetValue(propertyTypeAlias, filename, value.InputStream, culture, segment);
        }
        
    }
}
