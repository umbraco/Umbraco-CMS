using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace Umbraco.Cms.Core.Packaging
{
    public static class PackageMigrationResource
    {
        public static XDocument GetEmbeddedPackageDataManifest(Type planType)
        {
            // lookup the embedded resource by convention            
            Assembly currentAssembly = planType.Assembly;
            var fileName = $"{planType.Namespace}.package.xml";
            Stream stream = currentAssembly.GetManifestResourceStream(fileName);
            if (stream == null)
            {
                throw new FileNotFoundException("Cannot find the embedded file.", fileName);
            }
            XDocument xml;
            using (stream)
            {
                xml = XDocument.Load(stream);
            }
            return xml;
        }
    }
}
