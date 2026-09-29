// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common;

/// <summary>
///     An <see cref="IFormFeature" /> whose form access always throws, simulating a request that declares a form
///     content type but carries a body the framework cannot parse (e.g. a truncated multipart POST), where the
///     synchronous <see cref="HttpRequest.Form" /> getter throws.
/// </summary>
internal sealed class ThrowingFormFeature : IFormFeature
{
    private readonly Exception _exception;

    public ThrowingFormFeature(Exception exception) => _exception = exception;

    public bool HasFormContentType => true;

    public IFormCollection? Form
    {
        get => throw _exception;
        set => throw new NotSupportedException();
    }

    public IFormCollection ReadForm() => throw _exception;

    public Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken) => throw _exception;
}
