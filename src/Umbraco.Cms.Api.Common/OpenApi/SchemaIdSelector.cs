using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Api.Common.OpenApi;

public class SchemaIdSelector : OpenApiSelectorBase, ISchemaIdSelector
{
    private readonly IEnumerable<ISchemaIdHandler> _schemaIdHandlers;

    [Obsolete("Use non obsolete constructor instead. Will be removed in v16.")]
    public SchemaIdSelector(IEnumerable<ISchemaIdHandler> schemaIdHandlers)
        : this(
            StaticServiceProvider.Instance.GetRequiredService<IOptions<GlobalSettings>>(),
            StaticServiceProvider.Instance.GetRequiredService<IHostingEnvironment>(),
            StaticServiceProvider.Instance.GetRequiredService<IHttpContextAccessor>(),
            schemaIdHandlers)
    { }

    public SchemaIdSelector(
        IOptions<GlobalSettings> settings,
        IHostingEnvironment hostingEnvironment,
        IHttpContextAccessor httpContextAccessor,
        IEnumerable<ISchemaIdHandler> schemaIdHandlers)
    : base(settings, hostingEnvironment, httpContextAccessor)
        => _schemaIdHandlers = schemaIdHandlers;

    public virtual string SchemaId(Type type)
    {
        var documentName = ResolveOpenApiDocumentName();
        if (!string.IsNullOrEmpty(documentName))
        {
            ISchemaIdHandler? handler = _schemaIdHandlers.FirstOrDefault(h => h.CanHandle(type, documentName));
            return handler?.Handle(type) ?? type.Name;
        }
        return type.Name;
    }
}
