using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Api.Common.OpenApi;

public class OperationIdSelector : OpenApiSelectorBase, IOperationIdSelector
{
    private readonly IEnumerable<IOperationIdHandler> _operationIdHandlers;

    [Obsolete("Use non-obsolete constructor. This will be removed in Umbraco 15.")]
    public OperationIdSelector()
        : this(Enumerable.Empty<IOperationIdHandler>())
    { }

    [Obsolete("Use non obsolete constructor instead. Will be removed in v16.")]
    public OperationIdSelector(IEnumerable<IOperationIdHandler> operationIdHandlers)
        : this(
            StaticServiceProvider.Instance.GetRequiredService<IOptions<GlobalSettings>>(),
            StaticServiceProvider.Instance.GetRequiredService<IHostingEnvironment>(),
            StaticServiceProvider.Instance.GetRequiredService<IHttpContextAccessor>(),
            operationIdHandlers)
    { }

    public OperationIdSelector(
        IOptions<GlobalSettings> settings,
        IHostingEnvironment hostingEnvironment,
        IHttpContextAccessor httpContextAccessor,
        IEnumerable<IOperationIdHandler> operationIdHandlers)
        : base(settings, hostingEnvironment, httpContextAccessor)
        => _operationIdHandlers = operationIdHandlers;

    [Obsolete("Use overload that only takes ApiDescription instead. This will be removed in Umbraco 15.")]
    public virtual string? OperationId(ApiDescription apiDescription, ApiVersioningOptions apiVersioningOptions) => OperationId(apiDescription);

    public virtual string? OperationId(ApiDescription apiDescription)
    {
        var documentName = ResolveOpenApiDocumentName();
        if (!string.IsNullOrEmpty(documentName))
        {
            IOperationIdHandler? handler = _operationIdHandlers.FirstOrDefault(h => h.CanHandle(apiDescription, documentName));
            return handler?.Handle(apiDescription);
        }
        return null;
    }
}
