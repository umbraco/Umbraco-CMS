using System.Net.Mime;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Provides methods to create instances of UDT (Umbraco Data Transfer) file content, typically used for handling import and export operations within the Umbraco CMS management API.
/// </summary>
public class UdtFileContentFactory : IUdtFileContentFactory
{
    private readonly IEntityXmlSerializer _entityXmlSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="UdtFileContentFactory"/> class.
    /// </summary>
    /// <param name="entityXmlSerializer">An <see cref="IEntityXmlSerializer"/> used to serialize and deserialize entity data to and from XML.</param>
    public UdtFileContentFactory(IEntityXmlSerializer entityXmlSerializer)
        => _entityXmlSerializer = entityXmlSerializer;

    /// <summary>
    /// Serializes the specified <see cref="IContentType"/> to XML and returns it as a <see cref="FileContentResult"/> for download.
    /// </summary>
    /// <param name="contentType">The content type to serialize and convert to file content.</param>
    /// <returns>A <see cref="FileContentResult"/> containing the serialized XML representation of the content type.</returns>
    public FileContentResult Create(IContentType contentType)
    {
        XElement xml = _entityXmlSerializer.Serialize(contentType);
        return XmlTofile(contentType, xml);
    }

    /// <summary>
    /// Creates a <see cref="FileContentResult"/> from the specified content type by serializing it to XML.
    /// </summary>
    /// <param name="contentType">The content type to serialize and create the file content from.</param>
    /// <returns>A <see cref="FileContentResult"/> representing the serialized content type as a file.</returns
    public FileContentResult Create(IMediaType mediaType)
    {
        XElement xml = _entityXmlSerializer.Serialize(mediaType);
        return XmlTofile(mediaType, xml);
    }

    /// <summary>
    /// Creates a <see cref="FileContentResult"/> from the specified content type.
    /// </summary>
    /// <param name="contentType">The content type to create the file content from.</param>
    /// <returns>A <see cref="FileContentResult"/> representing the serialized XML content of the content type.</returns>
    public FileContentResult Create(IMemberType memberType)
    {
        XElement xml = _entityXmlSerializer.Serialize(memberType);
        return XmlTofile(memberType, xml);
    }

    private static FileContentResult XmlTofile(IContentTypeBase contentTypeBase, XElement xml) =>
        new(Encoding.UTF8.GetBytes(xml.ToDataString()), MediaTypeNames.Application.Octet)
        {
            FileDownloadName = $"{contentTypeBase.Alias}.udt"
        };
}
