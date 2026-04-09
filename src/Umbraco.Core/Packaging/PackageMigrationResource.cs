using System.IO.Compression;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Packaging;

/// <summary>
///     Provides utility methods for accessing embedded package migration resources.
/// </summary>
public static class PackageMigrationResource
{
    /// <summary>
    ///     Gets the embedded package data manifest from the specified plan type.
    /// </summary>
    /// <param name="planType">The type of the migration plan to get the embedded package from.</param>
    /// <param name="zipArchive">When this method returns, contains the zip archive if the package is a zip file; otherwise, <c>null</c>.</param>
    /// <returns>The package XML document, or <c>null</c> if no embedded package was found.</returns>
    public static XDocument? GetEmbeddedPackageDataManifest(Type planType, out ZipArchive? zipArchive)
    {
        XDocument? packageXml;
        Stream? zipStream = GetEmbeddedPackageZipStream(planType);
        if (zipStream is not null)
        {
            zipArchive = GetPackageDataManifest(zipStream, out packageXml);
            return packageXml;
        }

        zipArchive = null;
        packageXml = GetEmbeddedPackageXmlDoc(planType);
        return packageXml;
    }

    /// <summary>
    ///     Gets the embedded package zip stream from the specified plan type.
    /// </summary>
    /// <param name="planType">The type of the migration plan.</param>
    /// <returns>The stream for the embedded zip file, or <c>null</c> if not found.</returns>
    private static Stream? GetEmbeddedPackageZipStream(Type planType)
    {
        // lookup the embedded resource by convention
        Assembly currentAssembly = planType.Assembly;
        var fileName = $"{planType.Namespace}.package.zip";
        Stream? stream = currentAssembly.GetManifestResourceStream(fileName);

        return stream;
    }

    /// <summary>
    ///     Gets the embedded package data manifest from the specified plan type.
    /// </summary>
    /// <param name="planType">The type of the migration plan to get the embedded package from.</param>
    /// <returns>The package XML document, or <c>null</c> if no embedded package was found.</returns>
    public static XDocument? GetEmbeddedPackageDataManifest(Type planType) =>
        GetEmbeddedPackageDataManifest(planType, out _);

    /// <summary>
    ///     Gets a hash of the embedded package data manifest for the specified plan type.
    /// </summary>
    /// <param name="planType">The type of the migration plan.</param>
    /// <returns>A hash string representing the package contents.</returns>
    /// <exception cref="IOException">Thrown when no embedded package files are found for the plan type.</exception>
    public static string GetEmbeddedPackageDataManifestHash(Type planType)
    {
        // SEE: HashFromStreams in the benchmarks project for how fast this is. It will run
        // on every startup for every embedded package.zip. The bigger the zip, the more time it takes.
        // But it is still very fast ~303ms for a 100MB file. This will only be an issue if there are
        // several very large package.zips.
        using Stream? stream = GetEmbeddedPackageZipStream(planType);

        if (stream is not null)
        {
            return stream.GetStreamHash();
        }

        XDocument? xml = GetEmbeddedPackageXmlDoc(planType);

        if (xml is not null)
        {
            return xml.ToString();
        }

        throw new IOException("Missing embedded files for planType: " + planType);
    }

    /// <summary>
    ///     Gets the embedded package XML document from the specified plan type.
    /// </summary>
    /// <param name="planType">The type of the migration plan.</param>
    /// <returns>The package XML document, or <c>null</c> if not found.</returns>
    private static XDocument? GetEmbeddedPackageXmlDoc(Type planType)
    {
        // lookup the embedded resource by convention
        Assembly currentAssembly = planType.Assembly;
        var fileName = $"{planType.Namespace}.package.xml";
        Stream? stream = currentAssembly.GetManifestResourceStream(fileName);
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

    /// <summary>
    ///     Attempts to get the embedded package data manifest from the specified plan type.
    /// </summary>
    /// <param name="planType">The type of the migration plan.</param>
    /// <param name="packageXml">When this method returns, contains the package XML document if found; otherwise, <c>null</c>.</param>
    /// <param name="zipArchive">When this method returns, contains the zip archive if the package is a zip file; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if an embedded package was found; otherwise, <c>false</c>.</returns>
    public static bool TryGetEmbeddedPackageDataManifest(Type planType, out XDocument? packageXml, out ZipArchive? zipArchive)
    {
        Stream? zipStream = GetEmbeddedPackageZipStream(planType);
        if (zipStream is not null)
        {
            zipArchive = GetPackageDataManifest(zipStream, out packageXml);
            return true;
        }

        zipArchive = null;
        packageXml = GetEmbeddedPackageXmlDoc(planType);
        return packageXml is not null;
    }

    /// <summary>
    ///     Gets the package data manifest from a zip stream.
    /// </summary>
    /// <param name="packageZipStream">The zip stream containing the package.</param>
    /// <param name="packageXml">When this method returns, contains the package XML document.</param>
    /// <returns>The opened <see cref="ZipArchive"/> for the package.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="packageZipStream"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the zip package does not contain the required package.xml file.</exception>
    public static ZipArchive GetPackageDataManifest(Stream packageZipStream, out XDocument packageXml)
    {
        if (packageZipStream == null)
        {
            throw new ArgumentNullException(nameof(packageZipStream));
        }

        var zip = new ZipArchive(packageZipStream, ZipArchiveMode.Read);
        ZipArchiveEntry? packageXmlEntry = zip.GetEntry("package.xml");
        if (packageXmlEntry == null)
        {
            throw new InvalidOperationException("Zip package does not contain the required package.xml file");
        }

        using (Stream packageXmlStream = packageXmlEntry.Open())
        using (var xmlReader = XmlReader.Create(packageXmlStream, new XmlReaderSettings { IgnoreWhitespace = true }))
        {
            packageXml = XDocument.Load(xmlReader);
        }

        return zip;
    }
}
