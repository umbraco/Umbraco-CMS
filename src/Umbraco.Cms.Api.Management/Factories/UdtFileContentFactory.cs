using System.Net.Mime;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class UdtFileContentFactory : IUdtFileContentFactory
{
    private readonly IEntityXmlSerializer _entityXmlSerializer;

    public UdtFileContentFactory(IEntityXmlSerializer entityXmlSerializer)
    {
        _entityXmlSerializer = entityXmlSerializer;
    }

    public FileContentResult Create(IContentType contentType)
    {
        XElement xml = _entityXmlSerializer.Serialize(contentType);
        var fileName = $"{contentType.Alias}.udt";
        return XmlTofile(fileName, xml);
    }

    public FileContentResult Create(IMediaType mediaType)
    {
        XElement xml = _entityXmlSerializer.Serialize(mediaType);
        var fileName = $"{mediaType.Alias}.udt";
        return XmlTofile(fileName, xml);
    }

    private static FileContentResult XmlTofile(string fileName, XElement xml) =>
        new(Encoding.UTF8.GetBytes(xml.ToDataString()), MediaTypeNames.Application.Octet)
        {
            FileDownloadName = fileName
        };
}
