using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IUdtFileContentFactory
{
    FileContentResult Create(IContentType contentType);

    FileContentResult Create(IMediaType mediaType);
}
