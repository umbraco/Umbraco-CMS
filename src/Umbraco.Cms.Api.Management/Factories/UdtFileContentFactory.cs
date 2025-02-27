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
        => _entityXmlSerializer = entityXmlSerializer;

    public FileContentResult Create(IContentType contentType)
    {
        XElement xml = _entityXmlSerializer.Serialize(contentType);
        return XmlTofile(contentType, xml);
    }

    public FileContentResult Create(IMediaType mediaType)
    {
        XElement xml = _entityXmlSerializer.Serialize(mediaType);
        return XmlTofile(mediaType, xml);
    }

    private static FileContentResult XmlTofile(IContentTypeBase contentTypeBase, XElement xml) =>
        new(Encoding.UTF8.GetBytes(xml.ToDataString()), MediaTypeNames.Application.Octet)
        {
            FileDownloadName = $"{contentTypeBase.Alias}.udt"
        };
}
