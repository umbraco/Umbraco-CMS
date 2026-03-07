using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Defines a factory interface for creating file content related to User-Defined Tables (UDT).
/// Implementations of this interface are responsible for generating or retrieving UDT file data.
/// </summary>
public interface IUdtFileContentFactory
{
    /// <summary>
    /// Creates a <see cref="FileContentResult"/> for the specified <see cref="IContentType"/>.
    /// </summary>
    /// <param name="contentType">The content type for which to generate the file content result.</param>
    /// <returns>A <see cref="FileContentResult"/> representing the generated file content for the given content type.</returns
    FileContentResult Create(IContentType contentType);

    FileContentResult Create(IMediaType mediaType);

    FileContentResult Create(IMemberType mediaType) => throw new NotImplementedException();
}
