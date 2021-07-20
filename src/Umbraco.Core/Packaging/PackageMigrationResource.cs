using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Packaging
{
    public static class PackageMigrationResource
    {
        private static Stream GetEmbeddedPackageStream(Type planType)
        {
            // lookup the embedded resource by convention
            Assembly currentAssembly = planType.Assembly;
            var fileName = $"{planType.Namespace}.package.zip";
            Stream stream = currentAssembly.GetManifestResourceStream(fileName);
            if (stream == null)
            {
                throw new FileNotFoundException("Cannot find the embedded file.", fileName);
            }
            return stream;
        }

        public static string GetEmbeddedPackageDataManifestHash(Type planType)
        {
            // SEE: HashFromStreams in the benchmarks project for how fast this is. It will run
            // on every startup for every embedded package.zip. The bigger the zip, the more time it takes.
            // But it is still very fast ~303ms for a 100MB file. This will only be an issue if there are
            // several very large package.zips. 

            using Stream stream = GetEmbeddedPackageStream(planType);
            return stream.GetStreamHash();
        }

        public static ZipArchive GetEmbeddedPackageDataManifest(Type planType, out XDocument packageXml)
            => GetPackageDataManifest(GetEmbeddedPackageStream(planType), out packageXml);

        public static ZipArchive GetPackageDataManifest(Stream packageZipStream, out XDocument packageXml)
        {
            var zip = new ZipArchive(packageZipStream, ZipArchiveMode.Read);
            ZipArchiveEntry packageXmlEntry = zip.GetEntry("package.xml");
            if (packageXmlEntry == null)
            {
                throw new InvalidOperationException("Zip package does not contain the required package.xml file");
            }

            using (Stream packageXmlStream = packageXmlEntry.Open())
            using (var xmlReader = XmlReader.Create(packageXmlStream, new XmlReaderSettings
            {
                IgnoreWhitespace = true
            }))
            {
                packageXml = XDocument.Load(xmlReader);
            }

            return zip;
        }
    }
}
