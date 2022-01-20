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
        private static Stream GetEmbeddedPackageZipStream(Type planType)
        {
            // lookup the embedded resource by convention
            Assembly currentAssembly = planType.Assembly;
            var fileName = $"{planType.Namespace}.package.zip";
            Stream stream = currentAssembly.GetManifestResourceStream(fileName);

            return stream;
        }

        public static XDocument GetEmbeddedPackageDataManifest(Type planType, out ZipArchive zipArchive)
        {
            XDocument packageXml;
            var zipStream = GetEmbeddedPackageZipStream(planType);
            if (zipStream is not null)
            {
                zipArchive = GetPackageDataManifest(zipStream, out packageXml);
                return packageXml;
            }

            zipArchive = null;
            packageXml = GetEmbeddedPackageXmlDoc(planType);
            return packageXml;
        }

        public static XDocument GetEmbeddedPackageDataManifest(Type planType)
        {
            return GetEmbeddedPackageDataManifest(planType, out _);
        }

        private static XDocument GetEmbeddedPackageXmlDoc(Type planType)
        {
            // lookup the embedded resource by convention
            Assembly currentAssembly = planType.Assembly;
            var fileName = $"{planType.Namespace}.package.xml";
            Stream stream = currentAssembly.GetManifestResourceStream(fileName);
            if (stream == null)
            {
                return null;
            }
            XDocument xml;
            using (stream)
            {
                xml = XDocument.Load(stream);
            }
            return xml;
        }

        public static string GetEmbeddedPackageDataManifestHash(Type planType)
        {
            // SEE: HashFromStreams in the benchmarks project for how fast this is. It will run
            // on every startup for every embedded package.zip. The bigger the zip, the more time it takes.
            // But it is still very fast ~303ms for a 100MB file. This will only be an issue if there are
            // several very large package.zips.

            using Stream stream = GetEmbeddedPackageZipStream(planType);

            if (stream is not null)
            {
                return stream.GetStreamHash();
            }

            var xml = GetEmbeddedPackageXmlDoc(planType);

            if (xml is not null)
            {
                return xml.ToString();
            }

            throw new IOException("Missing embedded files for planType: " + planType);
        }

        public static bool TryGetEmbeddedPackageDataManifest(Type planType, out XDocument packageXml, out ZipArchive zipArchive)
        {
            var zipStream = GetEmbeddedPackageZipStream(planType);
            if (zipStream is not null)
            {
                zipArchive = GetPackageDataManifest(zipStream, out packageXml);
                return true;
            }

            zipArchive = null;
            packageXml = GetEmbeddedPackageXmlDoc(planType);
            return packageXml is not null;
        }

        public static ZipArchive GetPackageDataManifest(Stream packageZipStream, out XDocument packageXml)
        {
            if (packageZipStream == null)
            {
                throw new ArgumentNullException(nameof(packageZipStream));
            }

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
